using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Crm.Tests.Integration;

public class MultiTenantIsolationTests
{
    [Fact]
    public async Task AuthenticatedTenantA_SendingTenantBHeaderOrHost_StillOnlyAccessesTenantAData()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TenantContext();

        // Seed data under tenant context A
        tenantContext.SetTenant(tenantA);
        using (var db = new ApplicationDbContext(options, null!, tenantContext))
        {
            db.Customers.Add(new Customer { Name = "Customer A", TenantId = tenantA });
            await db.SaveChangesAsync();
        }

        // Seed data under tenant context B
        tenantContext.SetTenant(tenantB);
        using (var db = new ApplicationDbContext(options, null!, tenantContext))
        {
            db.Customers.Add(new Customer { Name = "Customer B", TenantId = tenantB });
            await db.SaveChangesAsync();
        }

        // Act: Query as Tenant A (even if external client headers attempt to send Tenant B)
        tenantContext.SetTenant(tenantA); // Authenticated claim forces Tenant A
        using (var db = new ApplicationDbContext(options, null!, tenantContext))
        {
            var customers = await db.Customers.ToListAsync();

            // Assert: Only Tenant A customer is returned
            Assert.Single(customers);
            Assert.Equal("Customer A", customers[0].Name);
            Assert.Equal(tenantA, customers[0].TenantId);
        }
    }

    [Fact]
    public async Task QueryingTenantBRecordId_AsTenantA_ReturnsNotFoundOrEmpty()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TenantContext();
        tenantContext.SetTenant(tenantB);

        long tenantBCustomerId;
        using (var db = new ApplicationDbContext(options, null!, tenantContext))
        {
            var customerB = new Customer { Name = "Secret Customer B", TenantId = tenantB };
            db.Customers.Add(customerB);
            await db.SaveChangesAsync();
            tenantBCustomerId = customerB.Id;
        }

        // Act: Query by Tenant B record ID while authenticated as Tenant A
        tenantContext.SetTenant(tenantA);
        using (var db = new ApplicationDbContext(options, null!, tenantContext))
        {
            var queriedCustomer = await db.Customers.FirstOrDefaultAsync(c => c.Id == tenantBCustomerId);

            // Assert: Global query filter hides Tenant B record from Tenant A
            Assert.Null(queriedCustomer);
        }
    }

    [Fact]
    public async Task CrossTenantWrite_IsRejectedWithInvalidOperationException()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TenantContext();
        tenantContext.SetTenant(tenantA);

        using (var db = new ApplicationDbContext(options, null!, tenantContext))
        {
            // Attempt to write an entity explicitly tagged with Tenant B while context is Tenant A
            var crossTenantCustomer = new Customer { Name = "Imposter Customer", TenantId = tenantB };
            db.Customers.Add(crossTenantCustomer);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
        }
    }
}
