using Crm.Application.Common.Interfaces;

namespace Crm.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // For authenticated requests, TenantId comes ONLY from the validated JWT claim.
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value
                ?? context.User.FindFirst("TenantId")?.Value;

            if (!string.IsNullOrWhiteSpace(tenantClaim) && Guid.TryParse(tenantClaim, out var claimTenantId))
            {
                tenantContext.SetTenant(claimTenantId);
            }
        }

        await _next(context);
    }
}
