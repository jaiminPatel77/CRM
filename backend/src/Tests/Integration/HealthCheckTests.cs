using Crm.Tests.Common;
using System.Net;
using Xunit;

namespace Crm.Tests.Integration;

public class HealthCheckTests : IntegrationTestBase
{
    public HealthCheckTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        Assert.True(await _context.Database.CanConnectAsync());
    }
}
