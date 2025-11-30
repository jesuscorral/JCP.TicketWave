# ADR-008: Security and Compliance

## Status
Accepted

## Date
2025-11-26

## Context
A ticket sales system handles sensitive information requiring robust protection:

- **Payment Data**: Card numbers, CVV, banking information
- **Personal Information**: Names, emails, addresses, phones
- **User Credentials**: Passwords, session tokens
- **Business Data**: Prices, inventories, sales metrics
- **System Logs**: May contain PII or sensitive data

**Compliance Requirements:**
- **PCI DSS**: Secure credit card processing
- **GDPR**: Protection of European users' personal data
- **SOX**: Financial controls for public companies (future)
- **ISO 27001**: Information security management

**Main Threats:**
- DDoS attacks during high-demand events
- SQL injection and XSS
- Credential stuffing and brute force
- Man-in-the-middle attacks
- Data breaches and data exfiltration

## Decision
We implement a **layered security architecture** (Defense in Depth) with:

### 1. Authentication and Authorization (Identity Layer)
- **JWT Bearer Tokens** with refresh token rotation
- **OAuth 2.0/OpenID Connect** with Azure AD B2C
- **Multi-factor Authentication** for critical operations
- **Role-Based Access Control (RBAC)** granular

### 2. Data Protection (Data Layer)
- **Encryption at Rest**: AES-256 for sensitive data
- **Encryption in Transit**: TLS 1.3 for all communications
- **PII Tokenization**: Personal data tokenized
- **Key Management**: Azure Key Vault for secrets

### 3. Network Security (Network Layer)
- **API Gateway**: Single entry point with rate limiting
- **DDoS Protection**: Azure DDoS Protection Standard
- **WAF**: Web Application Firewall with OWASP rules
- **Network Segmentation**: Private subnets for services

### 4. Application Security (Application Layer)
- **Input Validation**: Sanitization of all inputs
- **OWASP Top 10**: Mitigation of common vulnerabilities
- **Dependency Scanning**: Third-party library analysis
- **SAST/DAST**: Static and dynamic code analysis

### 5. Monitoring and Response (Monitoring Layer)
- **Security Information and Event Management (SIEM)**
- **Behavioral Analytics**: Anomaly detection
- **Audit Logging**: Complete action traceability
- **Incident Response**: Automated procedures

## Detailed Implementation

### 1. Authentication and Authorization

#### JWT Configuration con Security Headers
```csharp
public static class SecurityConfiguration
{
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // JWT Authentication
        var jwtSettings = configuration.GetSection("JWT").Get<JwtSettings>();
        var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true; // Enforce HTTPS
            options.SaveToken = false; // Don't store token in AuthenticationProperties
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1), // Minimal clock skew
                RequireExpirationTime = true,
                RequireSignedTokens = true
            };
            
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    // Additional validation logic
                    var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                    var userId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    // Check if user is still active
                    if (userId != null && !userService.IsUserActive(userId))
                    {
                        context.Fail("User account is deactivated");
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });
        
        // Authorization policies
        services.AddAuthorization(options =>
        {
            // Require authenticated user by default
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
                
            // Role-based policies
            options.AddPolicy("AdminOnly", policy => 
                policy.RequireRole("Admin"));
                
            options.AddPolicy("BookingAccess", policy => 
                policy.RequireRole("User", "Admin"));
                
            // Resource-based policies
            options.AddPolicy("BookingOwner", policy =>
                policy.Requirements.Add(new BookingOwnerRequirement()));
                
            // MFA for sensitive operations
            options.AddPolicy("RequiresMFA", policy =>
                policy.RequireClaim("amr", "mfa"));
        });
        
        return services;
    }
}

// Custom authorization requirement
public class BookingOwnerRequirement : IAuthorizationRequirement { }

public class BookingOwnerHandler : AuthorizationHandler<BookingOwnerRequirement>
{
    private readonly IBookingService _bookingService;
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BookingOwnerRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;
        var bookingIdStr = httpContext?.Request.RouteValues["bookingId"]?.ToString();
        
        if (Guid.TryParse(bookingIdStr, out var bookingId))
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            
            if (booking?.UserId == userId || context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
            }
        }
        
        context.Fail();
    }
}
```

#### Secure Token Service
```csharp
public class SecureTokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly ILogger<SecureTokenService> _logger;
    private readonly IMemoryCache _tokenCache;
    
    public async Task<TokenResponse> GenerateTokensAsync(string userId, List<string> roles)
    {
        var accessToken = await GenerateAccessToken(userId, roles);
        var refreshToken = await GenerateRefreshToken(userId);
        
        // Store refresh token hash (not the token itself)
        var refreshTokenHash = HashRefreshToken(refreshToken);
        await _userService.StoreRefreshTokenAsync(userId, refreshTokenHash, DateTime.UtcNow.AddDays(30));
        
        // Add to blacklist cache for rotation tracking
        var jti = GenerateJti();
        _tokenCache.Set($"token_blacklist_{jti}", false, TimeSpan.FromHours(24));
        
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 1 hour
            TokenType = "Bearer"
        };
    }
    
    private async Task<string> GenerateAccessToken(string userId, List<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JWT").Get<JwtSettings>();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, await _userService.GetUserEmailAsync(userId)),
            new("jti", GenerateJti()), // Token ID for revocation
            new("iat", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("auth_time", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        
        // Add roles as claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1), // Short-lived access token
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private string GenerateJti() => Guid.NewGuid().ToString("N")[..16]; // Short unique ID
    
    private string HashRefreshToken(string refreshToken)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(hashedBytes);
    }
}
```

### 2. Data Protection y Encryption

#### Encryption Service
```csharp
public interface IEncryptionService
{
    Task<string> EncryptAsync(string plainText);
    Task<string> DecryptAsync(string cipherText);
    Task<string> HashPasswordAsync(string password);
    Task<bool> VerifyPasswordAsync(string password, string hash);
}

public class AzureKeyVaultEncryptionService : IEncryptionService
{
    private readonly KeyClient _keyClient;
    private readonly CryptographyClient _cryptoClient;
    private readonly ILogger<AzureKeyVaultEncryptionService> _logger;
    private const string KeyName = "ticketwave-data-encryption-key";
    
    public async Task<string> EncryptAsync(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));
        
        try
        {
            var data = Encoding.UTF8.GetBytes(plainText);
            var encryptResult = await _cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, data);
            
            return Convert.ToBase64String(encryptResult.Ciphertext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt data");
            throw new SecurityException("Encryption failed", ex);
        }
    }
    
    public async Task<string> DecryptAsync(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("Cipher text cannot be null or empty", nameof(cipherText));
        
        try
        {
            var data = Convert.FromBase64String(cipherText);
            var decryptResult = await _cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, data);
            
            return Encoding.UTF8.GetString(decryptResult.Plaintext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt data");
            throw new SecurityException("Decryption failed", ex);
        }
    }
    
    public async Task<string> HashPasswordAsync(string password)
    {
        // Use Argon2id for password hashing (more secure than bcrypt)
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = GenerateSalt(),
            DegreeOfParallelism = 1,
            Iterations = 3,
            MemorySize = 65536 // 64 MB
        };
        
        var hashBytes = await Task.Run(() => argon2.GetBytes(32));
        var saltAndHash = new byte[16 + 32]; // 16 bytes salt + 32 bytes hash
        
        Array.Copy(argon2.Salt, 0, saltAndHash, 0, 16);
        Array.Copy(hashBytes, 0, saltAndHash, 16, 32);
        
        return Convert.ToBase64String(saltAndHash);
    }
    
    public async Task<bool> VerifyPasswordAsync(string password, string hash)
    {
        try
        {
            var saltAndHash = Convert.FromBase64String(hash);
            var salt = saltAndHash[..16];
            var originalHash = saltAndHash[16..];
            
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 1,
                Iterations = 3,
                MemorySize = 65536
            };
            
            var computedHash = await Task.Run(() => argon2.GetBytes(32));
            return CryptographicOperations.FixedTimeEquals(originalHash, computedHash);
        }
        catch
        {
            return false;
        }
    }
    
    private static byte[] GenerateSalt()
    {
        var salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }
}

// Entity-level encryption
public abstract class EncryptedEntity
{
    [NotMapped]
    private readonly IEncryptionService _encryptionService;
    
    // Example: Encrypted email field
    [Column("email_encrypted")]
    public string EmailEncrypted { get; set; } = string.Empty;
    
    [NotMapped]
    public string Email
    {
        get => _encryptionService?.DecryptAsync(EmailEncrypted).GetAwaiter().GetResult() ?? string.Empty;
        set => EmailEncrypted = _encryptionService?.EncryptAsync(value).GetAwaiter().GetResult() ?? string.Empty;
    }
}

// Entity Framework value converter for automatic encryption
public class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(IEncryptionService encryptionService) 
        : base(
            v => encryptionService.EncryptAsync(v).GetAwaiter().GetResult(),
            v => encryptionService.DecryptAsync(v).GetAwaiter().GetResult())
    {
    }
}
```

### 3. Input Validation y Sanitization

#### Comprehensive Input Validation
```csharp
public static class SecurityMiddleware
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Security headers
            context.Response.Headers.Add("X-Frame-Options", "DENY");
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Add("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
            
            // HSTS (only in production)
            if (!context.Request.Host.Host.Contains("localhost"))
            {
                context.Response.Headers.Add("Strict-Transport-Security", 
                    "max-age=31536000; includeSubDomains; preload");
            }
            
            // Content Security Policy
            context.Response.Headers.Add("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self'; " +
                "font-src 'self'; " +
                "object-src 'none'; " +
                "media-src 'self'; " +
                "frame-src 'none';");
            
            await next();
        });
        
        return app;
    }
    
    public static IApplicationBuilder UseRequestValidation(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Rate limiting by IP
            var clientIp = GetClientIpAddress(context);
            if (await IsRateLimited(clientIp, context.Request.Path))
            {
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Rate limit exceeded");
                return;
            }
            
            // Content length validation
            if (context.Request.ContentLength > 10_000_000) // 10MB limit
            {
                context.Response.StatusCode = 413; // Payload Too Large
                await context.Response.WriteAsync("Request too large");
                return;
            }
            
            // Suspicious patterns detection
            if (ContainsSuspiciousPatterns(context.Request))
            {
                // Log security event
                LogSecurityEvent(context, "Suspicious request pattern detected");
                
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync("Invalid request");
                return;
            }
            
            await next();
        });
        
        return app;
    }
    
    private static bool ContainsSuspiciousPatterns(HttpRequest request)
    {
        var suspiciousPatterns = new[]
        {
            // SQL Injection patterns
            @"('|(\\')|(;|(\\\;)|(\\'))|((\\\'')|(\\\')|(\\')|(\\;))",
            @"((\%27)|(\'))((\%6F)|o|(\%4F))((\%72)|r|(\%52))",
            @"\b(union|select|insert|delete|update|drop|create|alter|exec|execute)\b",
            
            // XSS patterns
            @"<\s*script[^>]*>.*?<\s*/\s*script\s*>",
            @"javascript\s*:",
            @"on\w+\s*=",
            
            // Path traversal
            @"(\.\./)|(\.\.\\)",
            @"(\.\.%2f)|(\.\.%5c)",
            
            // Command injection
            @"[;&|`]",
            @"\$\(.*\)",
            @"`.*`"
        };
        
        var allValues = new List<string>();
        
        // Check query parameters
        foreach (var param in request.Query)
        {
            allValues.Add(param.Key);
            allValues.AddRange(param.Value);
        }
        
        // Check headers (common injection points)
        var headersToCheck = new[] { "User-Agent", "Referer", "X-Forwarded-For" };
        foreach (var header in headersToCheck)
        {
            if (request.Headers.TryGetValue(header, out var values))
            {
                allValues.AddRange(values);
            }
        }
        
        // Check for suspicious patterns
        foreach (var value in allValues)
        {
            foreach (var pattern in suspiciousPatterns)
            {
                if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}

// Model validation with security annotations
public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("Invalid event ID format");
        
        RuleFor(x => x.UserId)
            .NotEmpty()
            .MaximumLength(450) // Standard ASP.NET Identity length
            .Matches(@"^[a-zA-Z0-9@._-]+$") // Alphanumeric and safe chars only
            .WithMessage("Invalid user ID format");
        
        RuleFor(x => x.TicketCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(10) // Reasonable limit
            .WithMessage("Ticket count must be between 1 and 10");
            
        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254) // RFC 5321 limit
            .WithMessage("Valid email address required");
            
        RuleFor(x => x.CustomerPhone)
            .Matches(@"^\+?[1-9]\d{1,14}$") // E.164 format
            .When(x => !string.IsNullOrEmpty(x.CustomerPhone))
            .WithMessage("Invalid phone number format");
    }
    
    private bool BeValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}
```

### 4. Security Monitoring y Logging

#### Security Event Logging
```csharp
public interface ISecurityEventLogger
{
    Task LogSecurityEventAsync(SecurityEvent securityEvent);
    Task LogAuthenticationEventAsync(AuthenticationEvent authEvent);
    Task LogAuthorizationEventAsync(AuthorizationEvent authEvent);
}

public class SecurityEventLogger : ISecurityEventLogger
{
    private readonly ILogger<SecurityEventLogger> _logger;
    private readonly IAuditService _auditService;
    
    public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        // Structure logging for SIEM ingestion
        using (LogContext.PushProperty("EventType", "Security"))
        using (LogContext.PushProperty("SecurityEventType", securityEvent.EventType))
        using (LogContext.PushProperty("Severity", securityEvent.Severity))
        using (LogContext.PushProperty("ClientIP", securityEvent.ClientIpAddress))
        using (LogContext.PushProperty("UserAgent", securityEvent.UserAgent))
        using (LogContext.PushProperty("UserId", securityEvent.UserId))
        {
            _logger.LogWarning("Security event: {EventType} - {Description}", 
                securityEvent.EventType, securityEvent.Description);
        }
        
        // Send to audit service for retention and compliance
        await _auditService.RecordSecurityEventAsync(securityEvent);
        
        // Real-time alerting for critical events
        if (securityEvent.Severity >= SecurityEventSeverity.High)
        {
            await SendSecurityAlert(securityEvent);
        }
    }
    
    public async Task LogAuthenticationEventAsync(AuthenticationEvent authEvent)
    {
        using (LogContext.PushProperty("EventType", "Authentication"))
        using (LogContext.PushProperty("AuthResult", authEvent.Result))
        using (LogContext.PushProperty("AuthMethod", authEvent.Method))
        using (LogContext.PushProperty("ClientIP", authEvent.ClientIpAddress))
        using (LogContext.PushProperty("UserId", authEvent.UserId))
        {
            if (authEvent.Result == AuthenticationResult.Success)
            {
                _logger.LogInformation("User {UserId} authenticated successfully via {Method}",
                    authEvent.UserId, authEvent.Method);
            }
            else
            {
                _logger.LogWarning("Authentication failed for user {UserId} via {Method}. Reason: {Reason}",
                    authEvent.UserId, authEvent.Method, authEvent.FailureReason);
            }
        }
        
        // Track failed login attempts for brute force detection
        if (authEvent.Result == AuthenticationResult.Failed)
        {
            await TrackFailedLogin(authEvent);
        }
    }
    
    private async Task TrackFailedLogin(AuthenticationEvent authEvent)
    {
        var key = $"failed_login_{authEvent.ClientIpAddress}_{authEvent.UserId}";
        var attempts = await _cache.GetAsync<int>(key) + 1;
        
        await _cache.SetAsync(key, attempts, TimeSpan.FromMinutes(15));
        
        if (attempts >= 5) // Threshold for brute force
        {
            await LogSecurityEventAsync(new SecurityEvent
            {
                EventType = "BruteForceAttempt",
                Severity = SecurityEventSeverity.High,
                Description = $"Multiple failed login attempts detected for user {authEvent.UserId}",
                ClientIpAddress = authEvent.ClientIpAddress,
                UserId = authEvent.UserId
            });
        }
    }
}

// Custom middleware for security event capture
public class SecurityAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISecurityEventLogger _securityLogger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        catch (SecurityException ex)
        {
            await _securityLogger.LogSecurityEventAsync(new SecurityEvent
            {
                EventType = "SecurityException",
                Severity = SecurityEventSeverity.High,
                Description = ex.Message,
                ClientIpAddress = GetClientIpAddress(context),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                RequestPath = context.Request.Path,
                RequestMethod = context.Request.Method
            });
            
            throw; // Re-throw to maintain error handling flow
        }
        finally
        {
            stopwatch.Stop();
            
            // Log all security-sensitive operations
            if (IsSecuritySensitiveEndpoint(context.Request.Path))
            {
                await LogSecuritySensitiveOperation(context, stopwatch.ElapsedMilliseconds);
            }
        }
    }
    
    private bool IsSecuritySensitiveEndpoint(string path)
    {
        var sensitivePatterns = new[]
        {
            "/api/auth/",
            "/api/bookings/",
            "/api/payments/",
            "/api/admin/"
        };
        
        return sensitivePatterns.Any(pattern => path.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
```

### 5. PCI DSS Compliance

#### Secure Payment Processing
```csharp
public class PCICompliantPaymentService : IPaymentService
{
    // Never store full PAN (Primary Account Number)
    // Use tokenization for any PAN reference
    
    public async Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentCommand command)
    {
        // Validate PAN format without storing
        if (!IsValidPAN(command.CardNumber))
        {
            return PaymentResult.Failed("Invalid card number format");
        }
        
        // Immediately tokenize PAN
        var cardToken = await TokenizeCardNumber(command.CardNumber);
        
        // Clear sensitive data from memory
        command = command with { CardNumber = "****" };
        
        // Process payment with external provider
        var paymentRequest = new ExternalPaymentRequest
        {
            Amount = command.Amount,
            Currency = command.Currency,
            CardToken = cardToken, // Use token, not PAN
            MerchantId = _configuration["Payment:MerchantId"],
            // Never include CVV in logs or storage
        };
        
        var result = await _externalPaymentProvider.ProcessAsync(paymentRequest);
        
        // Audit payment attempt (without sensitive data)
        await _auditLogger.LogPaymentAttempt(new PaymentAuditEvent
        {
            Amount = command.Amount,
            Currency = command.Currency,
            CardTokenLast4 = cardToken[^4..], // Only last 4 digits
            UserId = command.UserId,
            Result = result.Success ? "Success" : "Failed",
            Timestamp = DateTime.UtcNow,
            TransactionId = result.TransactionId
        });
        
        return result;
    }
    
    private async Task<string> TokenizeCardNumber(string cardNumber)
    {
        // Use format-preserving encryption or external tokenization service
        // This example uses Azure Key Vault for tokenization
        var tokenRequest = new
        {
            pan = cardNumber,
            tokenFormat = "numerical" // Maintains PAN format
        };
        
        var response = await _tokenizationClient.PostAsync("/tokenize", 
            new StringContent(JsonSerializer.Serialize(tokenRequest)));
            
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenResponse.Token;
    }
    
    private bool IsValidPAN(string cardNumber)
    {
        // Luhn algorithm validation
        if (string.IsNullOrWhiteSpace(cardNumber)) return false;
        
        cardNumber = cardNumber.Replace(" ", "").Replace("-", "");
        
        if (!Regex.IsMatch(cardNumber, @"^\d{13,19}$")) return false;
        
        return LuhnCheck(cardNumber);
    }
    
    private static bool LuhnCheck(string cardNumber)
    {
        int sum = 0;
        bool alternate = false;
        
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(cardNumber[i].ToString());
            
            if (alternate)
            {
                n *= 2;
                if (n > 9) n = (n % 10) + 1;
            }
            
            sum += n;
            alternate = !alternate;
        }
        
        return (sum % 10) == 0;
    }
}

// Secure configuration for PCI compliance
public class PCIConfiguration
{
    public static void ConfigureForPCI(WebApplicationBuilder builder)
    {
        // Enforce HTTPS
        builder.Services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            options.HttpsPort = 443;
        });
        
        // Secure cookie configuration
        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.HttpOnly = HttpOnlyPolicy.Always;
            options.Secure = CookieSecurePolicy.Always;
            options.SameSite = SameSiteMode.Strict;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
        });
        
        // Data protection for PCI compliance
        builder.Services.AddDataProtection()
            .PersistKeysToAzureBlobStorage(_keyVaultUri)
            .ProtectKeysWithAzureKeyVault(_keyVaultKeyId, _credential)
            .SetApplicationName("TicketWave")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90)); // Regular key rotation
    }
}
```

## Monitoring y Compliance

### GDPR Compliance
```csharp
public interface IGDPRService
{
    Task<DataPortabilityResult> ExportUserDataAsync(string userId);
    Task<DeletionResult> DeleteUserDataAsync(string userId);
    Task<ConsentResult> RecordConsentAsync(string userId, ConsentType type, bool granted);
    Task<ConsentStatus> GetConsentStatusAsync(string userId);
}

public class GDPRService : IGDPRService
{
    public async Task<DataPortabilityResult> ExportUserDataAsync(string userId)
    {
        var userData = new
        {
            PersonalInformation = await GetPersonalInformation(userId),
            BookingHistory = await GetBookingHistory(userId),
            PaymentHistory = await GetPaymentHistory(userId),
            ConsentRecords = await GetConsentRecords(userId),
            ExportedAt = DateTime.UtcNow,
            Format = "JSON",
            Version = "1.0"
        };
        
        // Create secure download link
        var exportToken = await CreateSecureExportToken(userId);
        
        return new DataPortabilityResult
        {
            Success = true,
            DownloadToken = exportToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }
    
    public async Task<DeletionResult> DeleteUserDataAsync(string userId)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        
        try
        {
            // Anonymize instead of hard delete for compliance
            await AnonymizeUserData(userId);
            await AnonymizeBookingData(userId);
            await DeletePersonalFiles(userId);
            
            // Keep minimal data for legal requirements
            await CreateDeletionRecord(userId);
            
            await transaction.CommitAsync();
            
            return new DeletionResult { Success = true };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new GDPRException("Failed to delete user data", ex);
        }
    }
}
```

### Security Metrics y Alerting
```csharp
public static class SecurityMetrics
{
    public static readonly Counter AuthenticationAttempts = Metrics
        .CreateCounter("auth_attempts_total", "Authentication attempts", "result", "method");
    
    public static readonly Counter SecurityEvents = Metrics
        .CreateCounter("security_events_total", "Security events", "event_type", "severity");
    
    public static readonly Histogram PaymentProcessingDuration = Metrics
        .CreateHistogram("payment_processing_duration_seconds", "Payment processing time");
    
    public static readonly Gauge ActiveSessions = Metrics
        .CreateGauge("active_sessions", "Number of active user sessions");
    
    public static readonly Counter DataProtectionOperations = Metrics
        .CreateCounter("data_protection_operations_total", "Data protection operations", "operation");
}

// Alerting rules para security events
/*
groups:
- name: ticketwave.security
  rules:
  - alert: HighAuthenticationFailureRate
    expr: rate(auth_attempts_total{result="failed"}[5m]) > 10
    for: 2m
    labels:
      severity: warning
    annotations:
      summary: "High authentication failure rate detected"
      
  - alert: SecurityEventCritical
    expr: security_events_total{severity="critical"} > 0
    for: 0s
    labels:
      severity: critical
    annotations:
      summary: "Critical security event detected: {{ $labels.event_type }}"
      
  - alert: PaymentProcessingAnomalies
    expr: histogram_quantile(0.95, payment_processing_duration_seconds) > 30
    for: 5m
    labels:
      severity: warning
    annotations:
      summary: "Payment processing latency anomaly detected"
      
  - alert: UnauthorizedDataAccess
    expr: increase(security_events_total{event_type="unauthorized_access"}[1h]) > 5
    for: 0s
    labels:
      severity: critical
    annotations:
      summary: "Multiple unauthorized access attempts detected"
*/
```

## Testing de Seguridad

### Security Testing Framework
```csharp
[TestFixture]
public class SecurityTests
{
    [Test]
    public async Task Authentication_ShouldReject_ExpiredTokens()
    {
        // Arrange
        var expiredToken = GenerateExpiredJWT();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);
        
        // Act
        var response = await _httpClient.GetAsync("/api/bookings");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Headers.Should().ContainKey("Token-Expired");
    }
    
    [Test]
    public async Task InputValidation_ShouldReject_SqlInjectionAttempts()
    {
        // Arrange
        var maliciousInput = "'; DROP TABLE Users; --";
        
        // Act
        var response = await _httpClient.GetAsync($"/api/events?search={maliciousInput}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task Payment_ShouldNever_LogSensitiveData()
    {
        // Arrange
        var paymentCommand = new ProcessPaymentCommand
        {
            CardNumber = "4111111111111111",
            CVV = "123",
            Amount = 100
        };
        
        // Act
        await _paymentService.ProcessPaymentAsync(paymentCommand);
        
        // Assert
        var logs = GetCapturedLogs();
        logs.Should().NotContain(log => log.Contains("4111111111111111"));
        logs.Should().NotContain(log => log.Contains("123"));
    }
    
    [Test]
    public async Task RateLimit_ShouldBlock_ExcessiveRequests()
    {
        // Arrange & Act
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => _httpClient.GetAsync("/api/events"))
            .ToArray();
            
        var responses = await Task.WhenAll(tasks);
        
        // Assert
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
    }
}
```

## Consequences

### Advantages
- **Compliance Ready**: PCI DSS, GDPR, SOX compliance frameworks
- **Defense in Depth**: Multiple security layers reduce single point of failure
- **Proactive Security**: Real-time monitoring and threat detection
- **Auditability**: Complete audit trails for compliance
- **Scalable Security**: Scalable patterns with growth

### Disadvantages
- **Performance Impact**: Encryption and validation add latency
- **Complexity**: Multiple security layers increase complexity
- **Cost**: Key Vault, WAF, monitoring tools require investment
- **Maintenance**: Security updates and certificate rotation

### Residual Risks
- **Zero-day Exploits**: Unknown vulnerabilities
- **Insider Threats**: Malicious internal access
- **Social Engineering**: Attacks outside the technical system
- **Supply Chain**: Vulnerabilities in dependencies

## Roadmap de Implementación

### Fase 1: Foundation Security ✅
- HTTPS enforcement
- Basic authentication/authorization
- Input validation
- Security headers

### Fase 2: Advanced Protection
- WAF deployment
- Key Vault integration
- Advanced threat detection
- SIEM integration

### Fase 3: Compliance
- PCI DSS certification
- GDPR implementation
- SOX controls preparation
- Third-party security audit

### Fase 4: Continuous Security
- Automated security testing
- Penetration testing program
- Security awareness training
- Incident response automation