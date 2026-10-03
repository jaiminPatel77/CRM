using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Tenants.Commands.ProvisionTenant;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Unit;

public class ProvisionTenantCommandTests
{
    private (ApplicationDbContext dbContext, UserManager<User> userManager, RoleManager<Role> roleManager) GetSetup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options, new AuditableEntityInterceptor(
            Substitute.For<ICurrentUserService>(),
            Substitute.For<IDateTime>()));

        var userStore = Substitute.For<IUserStore<User>>();
        var userManager = Substitute.For<UserManager<User>>(
            userStore, null, null, null, null, null, null, null, null);

        var roleStore = Substitute.For<IRoleStore<Role>>();
        var roleManager = Substitute.For<RoleManager<Role>>(
            roleStore, null, null, null, null);

        // Configure default userManager behaviour
        userManager.FindByEmailAsync(Arg.Any<string>()).Returns(Task.FromResult<User?>(null));
        userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);

        // Configure default roleManager behaviour
        roleManager.RoleExistsAsync(Arg.Any<string>()).Returns(false);
        roleManager.CreateAsync(Arg.Any<Role>()).Returns(IdentityResult.Success);

        return (dbContext, userManager, roleManager);
    }

    [Fact]
    public async Task Handle_ShouldProvisionTenantAndCreateAdminUser()
    {
        // Arrange
        var (dbContext, userManager, roleManager) = GetSetup();
        var handler = new ProvisionTenantCommandHandler(dbContext, userManager, roleManager);
        var command = new ProvisionTenantCommand
        {
            TenantName = "Beta Ltd",
            TenantIdentifier = "beta-ltd",
            AdminEmail = "admin@beta.com",
            AdminFullName = "Beta Admin",
            AdminPassword = "SecurePassword123!",
            SubscriptionPlan = "Pro"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Beta Ltd", result.TenantName);
        Assert.Equal("beta-ltd", result.TenantIdentifier);
        Assert.Equal("admin@beta.com", result.AdminEmail);
        Assert.Equal("Pro", result.SubscriptionPlan);
        Assert.NotEqual(Guid.Empty, result.TenantGuid);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenIdentifierAlreadyExists()
    {
        // Arrange
        var (dbContext, userManager, roleManager) = GetSetup();
        dbContext.Tenants.Add(new Tenant
        {
            Name = "Existing",
            Identifier = "existing-slug",
            TenantGuid = Guid.NewGuid()
        });
        await dbContext.SaveChangesAsync();

        var handler = new ProvisionTenantCommandHandler(dbContext, userManager, roleManager);
        var command = new ProvisionTenantCommand
        {
            TenantName = "Duplicate",
            TenantIdentifier = "existing-slug",
            AdminEmail = "newadmin@beta.com",
            AdminFullName = "Admin",
            AdminPassword = "Password123!"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("already registered", ex.Message);
    }
}
