# ADR-010: Central Package Management Implementation

## Status
Accepted - December 2, 2024

## Context
Managing NuGet package versions across a multi-project solution can become complex and error-prone. Different projects may end up using different versions of the same package, leading to:

- Version conflicts and dependency resolution issues
- Inconsistent behavior across services
- Difficulty in updating packages across the solution
- Security vulnerabilities from outdated packages in some projects
- Increased maintenance overhead

## Decision
We will implement .NET's Central Package Management (CPM) feature to centralize all package version definitions.

### Implementation Details

#### Directory.Packages.props
- Created at solution root with `ManagePackageVersionsCentrally=true`
- All package versions defined using `<PackageVersion>` items
- Centralized target framework definition as `$(NetCoreTargetVersion)`

#### Project Files
- All `.csproj` files reference packages without version attributes
- Consistent use of `$(NetCoreTargetVersion)` for target framework
- Simplified package reference declarations

### Package Categories
- **ASP.NET Core**: Microsoft.AspNetCore.OpenApi, Swashbuckle.AspNetCore
- **Entity Framework**: Microsoft.EntityFrameworkCore.SqlServer, .Tools, .Design
- **Extensions**: Microsoft.Extensions.Hosting

## Benefits

### Consistency
- Guaranteed same package versions across all projects
- Eliminates version drift between services
- Consistent target framework across solution

### Maintainability
- Single location for version updates
- Easier security patch management
- Simplified dependency auditing

### Development Experience
- Faster restore operations
- Clear dependency management
- Reduced configuration errors

## Implementation Status
- ✅ Directory.Packages.props created and configured
- ✅ All project files updated to use centralized versions
- ✅ Target framework centralization implemented
- ✅ Solution builds successfully with CPM enabled

## Consequences

### Positive
- **Simplified Maintenance**: Package updates require changes in only one file
- **Version Consistency**: All projects automatically use the same package versions
- **Security**: Easier to identify and update vulnerable packages
- **Build Reliability**: Reduced chance of version conflicts

### Negative
- **Learning Curve**: Team needs to understand CPM workflow
- **Flexibility**: Cannot easily use different versions per project (by design)

## Compliance
This ADR supports:
- **ADR-002**: Clean Architecture - Simplified dependency management
- **ADR-009**: Repository Pattern - Consistent EF Core versions across repositories