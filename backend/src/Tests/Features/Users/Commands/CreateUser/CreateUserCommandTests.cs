using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Users;
using Crm.Domain.Consts;
using Crm.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Features.Users;

public class CreateUserCommandTests
{
    private readonly UserManager<User> _userManager;

    public CreateUserCommandTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        var options = Substitute.For<IOptions<IdentityOptions>>();
        options.Value.Returns(new IdentityOptions());
        _userManager = Substitute.For<UserManager<User>>(store, options, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenValid()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = "Password123!",
            Title = "Mr"
        };

        _userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);

        // Handler only takes UserManager
        var handler = new CreateUserCommandHandler(_userManager);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(0); 
    }

    [Fact]
    public async Task Handle_ShouldAssignRoles_WhenRolesProvided()
    {
        // Arrange
        var roles = new List<string> { "Administrator", "Staff" };
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = "Password123!",
            Roles = roles
        };

        _userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        _userManager.AddToRolesAsync(Arg.Any<User>(), roles).Returns(IdentityResult.Success);

        var handler = new CreateUserCommandHandler(_userManager);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        await _userManager.Received(1).AddToRolesAsync(Arg.Any<User>(), roles);
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToCreated()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = "Password123!"
        };

        User? capturedUser = null;
        _userManager.CreateAsync(Arg.Do<User>(u => capturedUser = u), Arg.Any<string>())
            .Returns(IdentityResult.Success);

        var handler = new CreateUserCommandHandler(_userManager);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Status.Should().Be(EnumUserStatus.Created);
    }

    [Fact]
    public async Task Handle_ShouldGeneratePassword_WhenNoneProvided()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = null // No password
        };

        string? capturedPassword = null;
        _userManager.CreateAsync(Arg.Any<User>(), Arg.Do<string>(p => capturedPassword = p))
            .Returns(IdentityResult.Success);

        var handler = new CreateUserCommandHandler(_userManager);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPassword.Should().NotBeNullOrEmpty();
        capturedPassword.Should().HaveLength(12); // Default generated length
    }
}
