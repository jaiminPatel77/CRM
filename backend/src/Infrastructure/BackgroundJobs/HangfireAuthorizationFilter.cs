using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http; // Required for GetHttpContext
using System.Security.Claims;

namespace Crm.Infrastructure.BackgroundJobs;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow all authenticated users with Admin role
        // Ideally checking for specific permission "Hangfire.View" if RBAC is granular.
        return httpContext.User.Identity?.IsAuthenticated == true && 
               httpContext.User.IsInRole("Admin");
    }
}
