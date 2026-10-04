using System.Security.Claims;
using Crm.Api.Middleware;
using Crm.Application.Common.Interfaces;
using Crm.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Unit;

public class TenantResolutionTests
{
    [Fact]
    public async Task InvokeAsync_ShouldIgnoreHeader_WhenHeaderIsPresent()
    {
        // Arrange
        var tenantGuid = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-ID"] = tenantGuid.ToString();

        var tenantContext = Substitute.For<ITenantContext>();

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new TenantResolutionMiddleware(next);

        // Act
        await middleware.InvokeAsync(httpContext, tenantContext);

        // Assert
        tenantContext.DidNotReceive().SetTenant(Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetTenant_WhenJwtTenantClaimIsPresent()
    {
        // Arrange
        var tenantGuid = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var claims = new[] { new Claim("tenant_id", tenantGuid.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);

        var tenantContext = Substitute.For<ITenantContext>();

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new TenantResolutionMiddleware(next);

        // Act
        await middleware.InvokeAsync(httpContext, tenantContext);

        // Assert
        tenantContext.Received(1).SetTenant(tenantGuid, null);
    }
}
