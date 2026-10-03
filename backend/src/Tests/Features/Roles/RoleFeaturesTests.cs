using System.Security.Claims;
using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Roles;
using Crm.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Gridify;

namespace Crm.Tests.Features.Roles;

public class RoleFeaturesTests
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public RoleFeaturesTests()
    {
        var roleStore = Substitute.For<IRoleStore<Role>>();
        _roleManager = Substitute.For<RoleManager<Role>>(roleStore, null, null, null, null);

        var userStore = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(userStore, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task CreateRole_ShouldReturnSuccess_WhenRoleDoesNotExist()
    {
        // Arrange
        var command = new CreateRoleCommand { Name = "NewRole", Description = "Test Description" };
        _roleManager.RoleExistsAsync(command.Name).Returns(false);
        _roleManager.CreateAsync(Arg.Any<Role>()).Returns(IdentityResult.Success);

        var handler = new CreateRoleCommandHandler(_roleManager);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        await _roleManager.Received(1).CreateAsync(Arg.Is<Role>(r => r.Name == command.Name && r.Description == command.Description));
    }

    [Fact]
    public async Task CreateRole_ShouldReturnFailure_WhenRoleAlreadyExists()
    {
        // Arrange
        var command = new CreateRoleCommand { Name = "ExistingRole" };
        _roleManager.RoleExistsAsync(command.Name).Returns(true);

        var handler = new CreateRoleCommandHandler(_roleManager);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain($"Role '{command.Name}' already exists.");
    }

    [Fact]
    public async Task GetRoleById_ShouldReturnRole_WhenRoleExists()
    {
        // Arrange
        var roleId = 1L;
        var role = new Role("TestRole") { Id = roleId, Description = "Desc" };
        _roleManager.FindByIdAsync(roleId.ToString()).Returns(role);
        _userManager.GetUsersInRoleAsync(role.Name!).Returns(new List<User>());

        var handler = new GetRoleByIdQueryHandler(_roleManager, _userManager);
        var query = new GetRoleByIdQuery { Id = roleId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.Name.Should().Be("TestRole");
    }

    [Fact]
    public async Task UpdateRolePermissions_ShouldAddPermissions_WhenValid()
    {
        // Arrange
        var roleId = 1L;
        var role = new Role("TestRole") { Id = roleId };
        var permissions = new List<string> { "Permission1", "Permission2" };
        _roleManager.FindByIdAsync(roleId.ToString()).Returns(role);
        _roleManager.GetClaimsAsync(role).Returns(new List<Claim>());
        _roleManager.RemoveClaimAsync(role, Arg.Any<Claim>()).Returns(IdentityResult.Success);
        _roleManager.AddClaimAsync(role, Arg.Any<Claim>()).Returns(IdentityResult.Success);

        var handler = new UpdateRolePermissionsCommandHandler(_roleManager);
        var command = new UpdateRolePermissionsCommand { RoleId = roleId, Permissions = permissions };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        await _roleManager.Received(2).AddClaimAsync(role, Arg.Is<Claim>(c => c.Type == "Permission" && permissions.Contains(c.Value)));
    }
}
