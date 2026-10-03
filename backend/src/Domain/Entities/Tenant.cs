using System.ComponentModel.DataAnnotations;
using Crm.Domain.Attributes;
using Crm.Domain.Common;

namespace Crm.Domain.Entities;

[Auditable]
public class Tenant : AuditableEntity
{
    public Guid TenantGuid { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Identifier { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [StringLength(256)]
    public string? ConnectionString { get; set; }

    [StringLength(50)]
    public string SubscriptionPlan { get; set; } = "Free";

    public DateTimeOffset? SubscriptionExpiresAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
