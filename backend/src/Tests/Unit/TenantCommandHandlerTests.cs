using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Tenants.Commands.CreateTenant;
using Crm.Application.Features.Tenants.Commands.ToggleTenantStatus;
using Crm.Application.Features.Tenants.Commands.UpdateTenant;
using Crm.Application.Features.Tenants.Queries.GetTenantById;
using Crm.Application.Features.Tenants.Queries.GetTenants;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Unit;

public class TenantCommandHandlerTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var auditableInterceptor = new AuditableEntityInterceptor(
            Substitute.For<ICurrentUserService>(),
            Substitute.For<IDateTime>());

        return new ApplicationDbContext(options, auditableInterceptor);
    }

    [Fact]
    public async Task CreateTenantCommand_ShouldCreateNewTenant()
    {
        // Arrange
        using var context = GetDbContext();
        var handler = new CreateTenantCommandHandler(context);
        var command = new CreateTenantCommand
        {
            Name = "Acme Corp",
            Identifier = "acme-corp",
            SubscriptionPlan = "Pro"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Acme Corp", result.Name);
        Assert.Equal("acme-corp", result.Identifier);
        Assert.True(result.IsActive);
        Assert.NotEqual(Guid.Empty, result.TenantGuid);
    }

    [Fact]
    public async Task GetTenantsQuery_ShouldReturnAllTenants()
    {
        // Arrange
        using var context = GetDbContext();
        context.Tenants.Add(new Tenant { Name = "Tenant 1", Identifier = "t1", TenantGuid = Guid.NewGuid() });
        context.Tenants.Add(new Tenant { Name = "Tenant 2", Identifier = "t2", TenantGuid = Guid.NewGuid() });
        await context.SaveChangesAsync();

        var handler = new GetTenantsQueryHandler(context);

        // Act
        var results = await handler.Handle(new GetTenantsQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task UpdateTenantCommand_ShouldUpdateExistingTenant()
    {
        // Arrange
        using var context = GetDbContext();
        var tenantGuid = Guid.NewGuid();
        context.Tenants.Add(new Tenant
        {
            Name = "Old Name",
            Identifier = "tenant-slug",
            TenantGuid = tenantGuid,
            SubscriptionPlan = "Free",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new UpdateTenantCommandHandler(context);
        var command = new UpdateTenantCommand
        {
            TenantGuid = tenantGuid,
            Name = "New Name",
            SubscriptionPlan = "Enterprise",
            IsActive = true
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Name", result.Name);
        Assert.Equal("Enterprise", result.SubscriptionPlan);
    }

    [Fact]
    public async Task ToggleTenantStatusCommand_ShouldDeactivateTenant()
    {
        // Arrange
        using var context = GetDbContext();
        var tenantGuid = Guid.NewGuid();
        context.Tenants.Add(new Tenant
        {
            Name = "Active Tenant",
            Identifier = "active-slug",
            TenantGuid = tenantGuid,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new ToggleTenantStatusCommandHandler(context);

        // Act
        var success = await handler.Handle(new ToggleTenantStatusCommand(tenantGuid, false), CancellationToken.None);

        // Assert
        Assert.True(success);
        var updatedTenant = await context.Tenants.FirstAsync(t => t.TenantGuid == tenantGuid);
        Assert.False(updatedTenant.IsActive);
    }
}
