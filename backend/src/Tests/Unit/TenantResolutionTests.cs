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
    public async Task InvokeAsync_ShouldSetTenant_WhenXTenantIdHeaderIsPresent()
    {
        // Arrange
        var tenantGuid = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-ID"] = tenantGuid.ToString();

        var tenantContext = Substitute.For<ITenantContext>();
        var dbContext = Substitute.For<IApplicationDbContext>();

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new TenantResolutionMiddleware(next);

        // Act
        await middleware.InvokeAsync(httpContext, tenantContext, dbContext);

        // Assert
        tenantContext.Received(1).SetTenant(tenantGuid, null);
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
        var dbContext = Substitute.For<IApplicationDbContext>();

        RequestDelegate next = (ctx) => Task.CompletedTask;
        var middleware = new TenantResolutionMiddleware(next);

        // Act
        await middleware.InvokeAsync(httpContext, tenantContext, dbContext);

        // Assert
        tenantContext.Received(1).SetTenant(tenantGuid, null);
    }
}
