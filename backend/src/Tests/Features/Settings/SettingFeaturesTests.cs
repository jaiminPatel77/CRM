using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Settings;
using Crm.Domain.Entities;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Features.Settings;

public class SettingFeaturesTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly HybridCache _cache;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public SettingFeaturesTests()
    {
        _currentUserService = Substitute.For<ICurrentUserService>();
        _dateTime = Substitute.For<IDateTime>();
        _dateTime.Now.Returns(DateTimeOffset.Now);
        _currentUserService.UserId.Returns("1");

        var interceptor = new AuditableEntityInterceptor(_currentUserService, _dateTime);
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, interceptor);

        // Setup real HybridCache with MemoryCache
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. 
        services.AddMemoryCache();
        services.AddHybridCache();
#pragma warning restore EXTEXP0018
        var serviceProvider = services.BuildServiceProvider();
        _cache = serviceProvider.GetRequiredService<HybridCache>();
    }

    [Fact]
    public async Task GetSettingByKey_ShouldReturnFromCacheOrDb()
    {
        // Arrange
        var setting = new Setting { Key = "TestKey", Value = "TestValue" };
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync();

        var handler = new GetSettingByKeyQueryHandler(_context, _cache);
        var query = new GetSettingByKeyQuery("TestKey");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("TestValue", result.Data!.Value);

        // Verify it was cached (by deleting from DB and fetching again)
        _context.Settings.Remove(setting);
        await _context.SaveChangesAsync();

        var cachedResult = await handler.Handle(query, CancellationToken.None);
        Assert.True(cachedResult.Succeeded);
        Assert.Equal("TestValue", cachedResult.Data!.Value);
    }

    [Fact]
    public async Task CreateSetting_ShouldAddEntry()
    {
        // Arrange
        var handler = new CreateSettingCommandHandler(_context);
        var command = new CreateSettingCommand { Key = "NewKey", Value = "NewValue" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Single(_context.Settings.Where(s => s.Key == "NewKey"));
    }

    [Fact]
    public async Task UpdateSMTPSetting_ShouldUpdateExistingOrAddCorrectKey()
    {
        // Arrange
        var handler = new UpdateSMTPSettingCommandHandler(_context);
        var command = new UpdateSMTPSettingCommand { Value = "{\"Host\":\"smtp.test.com\"}" };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var setting = await _context.Settings.FirstOrDefaultAsync(x => x.Key == "SMTP_SETTING");
        Assert.NotNull(setting);
        Assert.Contains("smtp.test.com", setting!.Value);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
