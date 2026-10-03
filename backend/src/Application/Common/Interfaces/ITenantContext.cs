namespace Crm.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid? CurrentTenantId { get; }
    string? CurrentTenantIdentifier { get; }
    void SetTenant(Guid tenantId, string? identifier = null);
}
