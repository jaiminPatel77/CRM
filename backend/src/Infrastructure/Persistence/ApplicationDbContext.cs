using Crm.Application.Common.Interfaces;
using Crm.Domain.Common;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

namespace Crm.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, long>, IApplicationDbContext
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor;
    private readonly ITenantContext? _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor,
        ITenantContext? tenantContext = null)
        : base(options)
    {
        _auditableEntityInterceptor = auditableEntityInterceptor;
        _tenantContext = tenantContext;
    }

    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    
    public DbSet<Setting> Settings { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Lead> Leads { get; set; } = null!;
    public DbSet<Opportunity> Opportunities { get; set; } = null!;
    public DbSet<Activity> Activities { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntityInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Identity configuration
        
        // --- Tenant Entity Configuration ---
        builder.Entity<Tenant>()
            .HasIndex(t => t.Identifier)
            .IsUnique();

        builder.Entity<Tenant>()
            .HasIndex(t => t.TenantGuid)
            .IsUnique();

        builder.Entity<User>()
            .HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .HasPrincipalKey(t => t.TenantGuid)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Multi-Tenant Global Query Filters ---
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(ConfigureTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                    .MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { builder });
            }
        }

        // --- Legacy Parity Mappings ---

        // User <-> UserProfile (Shared PK)
        builder.Entity<User>()
            .HasOne(u => u.UserProfile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // User <-> UserRefreshToken
        builder.Entity<UserRefreshToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.UserRefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Entity<UserRefreshToken>()
            .HasIndex(t => t.RefreshToken)
            .IsUnique();

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Ensure decimal precision
        foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }
    }

    private void ConfigureTenantFilter<TEntity>(ModelBuilder builder) where TEntity : class, ITenantEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => _tenantContext == null || !_tenantContext.CurrentTenantId.HasValue || _tenantContext.CurrentTenantId.Value == Guid.Empty || e.TenantId == _tenantContext.CurrentTenantId.Value);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantIdToEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTenantIdToEntities();
        return base.SaveChanges();
    }

    private void ApplyTenantIdToEntities()
    {
        if (_tenantContext?.CurrentTenantId.HasValue == true && _tenantContext.CurrentTenantId.Value != Guid.Empty)
        {
            var currentTenantId = _tenantContext.CurrentTenantId.Value;
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = currentTenantId;
                }
            }
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }
}
