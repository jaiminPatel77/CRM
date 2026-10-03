using System.ComponentModel.DataAnnotations;
using Crm.Domain.Attributes;
using Crm.Domain.Common;

namespace Crm.Domain.Entities;

[Auditable]
public class Customer : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? Company { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Industry { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
