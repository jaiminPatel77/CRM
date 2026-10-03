using Crm.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Crm.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<UserRefreshToken> UserRefreshTokens { get; }
    DbSet<UserProfile> UserProfiles { get; }
    
    DbSet<Setting> Settings { get; }
    DbSet<Tenant> Tenants { get; }
    
    DbSet<Customer> Customers { get; }
    DbSet<Lead> Leads { get; }
    DbSet<Opportunity> Opportunities { get; }
    DbSet<Activity> Activities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
    DbSet<T> Set<T>() where T : class;
    
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
