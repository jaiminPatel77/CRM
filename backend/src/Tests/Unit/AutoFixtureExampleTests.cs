using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Crm.Application.Common.Interfaces;
using Crm.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Unit;

public class AutoFixtureExampleTests
{
    private readonly IFixture _fixture;

    public AutoFixtureExampleTests()
    {
        // Setup AutoFixture to use NSubstitute for interface implementation
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        var userStore = Substitute.For<Microsoft.AspNetCore.Identity.IUserStore<Domain.Entities.User>, Microsoft.AspNetCore.Identity.IUserEmailStore<Domain.Entities.User>>();
        _fixture.Inject(userStore);
    }

    [Fact]
    public void AutoFixture_ShouldGenerateComplexObjects()
    {
        // Arrange
        // AutoFixture automatically fills all properties with random data
        var command = _fixture.Create<Application.Features.Users.CreateUserCommand>();

        // Assert
        Assert.False(string.IsNullOrEmpty(command.Email));
        Assert.False(string.IsNullOrEmpty(command.FullName));
        Assert.False(string.IsNullOrEmpty(command.Password));
    }

    [Fact]
    public async Task AutoFixture_WithNSubstitute_ShouldSimplifyDependencyInjection()
    {
        // Arrange
        // Create an instance of AuthService where all dependencies are automatically mocked by NSubstitute
        var service = _fixture.Create<AuthService>();
        var userEmail = _fixture.Create<string>() + "@example.com";
        var password = _fixture.Create<string>();

        // Setup the mock that was automatically created
        var userManager = _fixture.Freeze<Microsoft.AspNetCore.Identity.UserManager<Domain.Entities.User>>();
        userManager.FindByEmailAsync(userEmail).Returns((Domain.Entities.User)null!);

        // Act
        var result = await service.LoginAsync(userEmail, password);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid login attempt.", result.Errors);
    }
}
