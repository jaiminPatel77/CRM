using Crm.Application.Common.Interfaces;

namespace Crm.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    public Guid? CurrentTenantId { get; private set; }
    public string? CurrentTenantIdentifier { get; private set; }

    public void SetTenant(Guid tenantId, string? identifier = null)
    {
        CurrentTenantId = tenantId;
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            CurrentTenantIdentifier = identifier;
        }
    }
}
