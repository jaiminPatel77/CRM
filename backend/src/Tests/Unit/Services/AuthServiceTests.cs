using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Models;
using Crm.Domain.Entities;
using Crm.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IOptions<JwtIssuerOptions> _jwtOptions;
    private readonly IUserRefreshTokenService _refreshTokenService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthServiceTests()
    {
        var store = Substitute.For<IUserStore<User>>();
        _userManager = Substitute.For<UserManager<User>>(store, null, null, null, null, null, null, null, null);

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<User>>();
        _signInManager = Substitute.For<SignInManager<User>>(_userManager, _httpContextAccessor, claimsFactory, null, null, null, null);

        _jwtOptions = Substitute.For<IOptions<JwtIssuerOptions>>();
        _jwtOptions.Value.Returns(new JwtIssuerOptions 
        { 
            Issuer = "testIssuer",
            Audience = "testAudience",
            ValidFor = TimeSpan.FromHours(1),
            SecretKey = "SuperSecretKey123!ThisShouldBeInConfigIsLongEnough"
        });

        _refreshTokenService = Substitute.For<IUserRefreshTokenService>();
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Substitute.For<ILogger<AuthService>>();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User { Email = "test@example.com", UserName = "test@example.com", FullName = "Test User" };
        _userManager.FindByEmailAsync("test@example.com").Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, "Password123!", false).Returns(SignInResult.Success);
        _userManager.GetRolesAsync(user).Returns(new List<string> { "User" });

        var service = new AuthService(_userManager, _signInManager, _jwtOptions, _refreshTokenService, _emailSender, _logger, _httpContextAccessor);

        // Act
        var result = await service.LoginAsync("test@example.com", "Password123!");

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeNullOrEmpty();
        await _refreshTokenService.Received(1).CreateRefreshToken(Arg.Any<UserRefreshToken>());
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        _userManager.FindByEmailAsync(Arg.Any<string>()).Returns((User)null!);
        var service = new AuthService(_userManager, _signInManager, _jwtOptions, _refreshTokenService, _emailSender, _logger, _httpContextAccessor);

        // Act
        var result = await service.LoginAsync("unknown@example.com", "password");

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("Invalid login attempt.");
    }
    
    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenValid()
    {
        // Arrange
        _userManager.CreateAsync(Arg.Any<User>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(new List<string>());

        var service = new AuthService(_userManager, _signInManager, _jwtOptions, _refreshTokenService, _emailSender, _logger, _httpContextAccessor);

        // Act
        var result = await service.RegisterAsync("new@example.com", "Password123!", "New User");

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        await _userManager.Received(1).CreateAsync(Arg.Is<User>(u => u.Email == "new@example.com"), "Password123!");
    }
}
