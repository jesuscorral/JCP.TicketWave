using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.BookingService.Domain.Entities;
using JCP.TicketWave.BookingService.Infrastructure.Data.Configurations;

namespace JCP.TicketWave.BookingService.Infrastructure.Persistence;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingConfiguration).Assembly);
        
        // Set schema for booking service
        modelBuilder.HasDefaultSchema("booking");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This will be overridden by DI configuration in Program.cs
            optionsBuilder.UseSqlServer();
        }
        
        // Enable sensitive data logging only in development
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isDevelopment = environment == "Development";
        
        optionsBuilder.EnableSensitiveDataLogging(isDevelopment);
        optionsBuilder.EnableDetailedErrors(isDevelopment);
    }
    
    // Optimized SaveChanges for audit fields
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamp fields before saving
        foreach (var entry in ChangeTracker.Entries<Booking>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(Booking.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
        }
        
        foreach (var entry in ChangeTracker.Entries<Ticket>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(Ticket.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}