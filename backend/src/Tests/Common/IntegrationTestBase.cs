using Crm.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Crm.Tests.Common;

[Collection("IntegrationTests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected IServiceScope _scope;
    protected ApplicationDbContext _context;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        // Initial setup for context access
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public virtual async Task InitializeAsync()
    {
        // Reset DB state before each test
        await _factory.RespawnerResetAsync();

        // Refresh scope and context after reset
        _scope.Dispose();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public virtual async Task DisposeAsync()
    {
        _scope.Dispose();
        await Task.CompletedTask;
    }
}
