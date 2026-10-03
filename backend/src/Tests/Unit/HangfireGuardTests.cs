using Crm.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Unit;

public class HangfireGuardTests
{
    [Fact]
    public void Hangfire_ShouldThrowException_WhenInMemoryUsedInProduction()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "UseInMemoryDatabase", "true" },
            { "Authentication:JwtIssuerOptions:SecretKey", "SuperSecretKeyForTestingAtLeast32CharsLong!" },
            { "Authentication:JwtIssuerOptions:Issuer", "CrmApi" },
            { "Authentication:JwtIssuerOptions:Audience", "CrmClient" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var mockEnv = Substitute.For<IHostEnvironment>();
        mockEnv.EnvironmentName.Returns(Environments.Production);

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(mockEnv);

        services.AddInfrastructureServices(configuration);
        var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            provider.GetRequiredService<Hangfire.JobStorage>();
        });

        Assert.Contains("Hangfire in-memory storage fallback is strictly forbidden outside Development and Testing environments.", ex.Message);
    }

    [Fact]
    public void Hangfire_ShouldAllowInMemory_WhenInDevelopment()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "UseInMemoryDatabase", "true" },
            { "Authentication:JwtIssuerOptions:SecretKey", "SuperSecretKeyForTestingAtLeast32CharsLong!" },
            { "Authentication:JwtIssuerOptions:Issuer", "CrmApi" },
            { "Authentication:JwtIssuerOptions:Audience", "CrmClient" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var mockEnv = Substitute.For<IHostEnvironment>();
        mockEnv.EnvironmentName.Returns(Environments.Development);

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(mockEnv);

        services.AddInfrastructureServices(configuration);
        var provider = services.BuildServiceProvider();

        // Act
        var storage = provider.GetService<Hangfire.JobStorage>();

        // Assert
        Assert.NotNull(storage);
    }
}
