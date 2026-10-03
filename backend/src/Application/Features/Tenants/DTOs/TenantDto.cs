namespace Crm.Application.Features.Tenants.DTOs;

public class TenantDto
{
    public long Id { get; set; }
    public Guid TenantGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? ConnectionString { get; set; }
    public string SubscriptionPlan { get; set; } = "Free";
    public DateTimeOffset? SubscriptionExpiresAt { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset ModifiedOn { get; set; }
}
