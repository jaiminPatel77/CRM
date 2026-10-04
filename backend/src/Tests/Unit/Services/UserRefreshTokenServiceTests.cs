using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Interceptors;
using Crm.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Unit.Services;

public class UserRefreshTokenServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRefreshTokenService _service;

    public UserRefreshTokenServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var interceptor = new AuditableEntityInterceptor(Substitute.For<ICurrentUserService>(), Substitute.For<IDateTime>());
        _context = new ApplicationDbContext(options, interceptor);

        _service = new UserRefreshTokenService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateRefreshToken_ShouldAddTokenToDatabase()
    {
        // Arrange
        var token = new UserRefreshToken { UserId = 1, RefreshToken = "token123", ValidTill = DateTimeOffset.UtcNow.AddHours(1) };

        // Act
        await _service.CreateRefreshToken(token);

        // Assert
        var stored = await _context.UserRefreshTokens.FirstOrDefaultAsync(t => t.RefreshToken == "token123");
        Assert.NotNull(stored);
        Assert.Equal(1, stored!.UserId);
    }

    [Fact]
    public async Task GetRefreshToken_ShouldReturnToken_WhenValid()
    {
        // Arrange
        var token = new UserRefreshToken 
        { 
            UserId = 1, 
            RefreshToken = "valid_token", 
            ValidTill = DateTimeOffset.UtcNow.AddHours(1) 
        };
        _context.UserRefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRefreshToken("valid_token");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("valid_token", result!.RefreshToken);
    }

    [Fact]
    public async Task GetRefreshToken_ShouldDeleteAndReturnNull_WhenExpired()
    {
        // Arrange
        var token = new UserRefreshToken 
        { 
            UserId = 1, 
            RefreshToken = "expired_token", 
            ValidTill = DateTimeOffset.UtcNow.AddHours(-1) 
        };
        _context.UserRefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetRefreshToken("expired_token");

        // Assert
        Assert.Null(result);
        var stored = await _context.UserRefreshTokens.FirstOrDefaultAsync(t => t.RefreshToken == "expired_token");
        Assert.Null(stored); // Should be deleted
    }

    [Fact]
    public async Task RemoveRefreshToken_ShouldDeleteToken()
    {
        // Arrange
        var token = new UserRefreshToken 
        { 
            UserId = 1, 
            RefreshToken = "remove_me",
            ValidTill = DateTimeOffset.UtcNow.AddHours(1)
        };
        _context.UserRefreshTokens.Add(token);
        await _context.SaveChangesAsync();

        // Act
        await _service.RemoveRefreshToken("remove_me");

        // Assert
        var stored = await _context.UserRefreshTokens.FirstOrDefaultAsync(t => t.RefreshToken == "remove_me");
        Assert.Null(stored);
    }
}
