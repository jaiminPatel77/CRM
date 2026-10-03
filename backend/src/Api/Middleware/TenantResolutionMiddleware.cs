using System.Security.Claims;
using Crm.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private const string TenantIdHeader = "X-Tenant-ID";
    private const string TenantSlugHeader = "X-Tenant-Key";

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, IApplicationDbContext dbContext)
    {
        // 1. Check HTTP Request Header X-Tenant-ID
        if (context.Request.Headers.TryGetValue(TenantIdHeader, out var headerTenantIdStr) &&
            Guid.TryParse(headerTenantIdStr, out var headerTenantId))
        {
            tenantContext.SetTenant(headerTenantId);
        }
        // 2. Check HTTP Request Header X-Tenant-Key / X-Tenant-Slug
        else if (context.Request.Headers.TryGetValue(TenantSlugHeader, out var headerTenantSlug) &&
                 !string.IsNullOrWhiteSpace(headerTenantSlug))
        {
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Identifier == headerTenantSlug.ToString() && t.IsActive);

            if (tenant != null)
            {
                tenantContext.SetTenant(tenant.TenantGuid, tenant.Identifier);
            }
        }
        // 3. Check JWT Claims if user is authenticated
        else if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value
                ?? context.User.FindFirst("TenantId")?.Value;

            if (!string.IsNullOrWhiteSpace(tenantClaim) && Guid.TryParse(tenantClaim, out var claimTenantId))
            {
                tenantContext.SetTenant(claimTenantId);
            }
        }
        // 4. Subdomain Host matching (e.g., acme.crm.local -> acme)
        else
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');
            if (parts.Length >= 3 && !parts[0].Equals("www", StringComparison.OrdinalIgnoreCase))
            {
                var subdomain = parts[0];
                var tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Identifier == subdomain && t.IsActive);

                if (tenant != null)
                {
                    tenantContext.SetTenant(tenant.TenantGuid, tenant.Identifier);
                }
            }
        }

        await _next(context);
    }
}
