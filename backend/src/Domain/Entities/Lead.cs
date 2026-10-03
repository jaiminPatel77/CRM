using System.ComponentModel.DataAnnotations;
using Crm.Domain.Attributes;
using Crm.Domain.Common;
using Crm.Domain.Consts;

namespace Crm.Domain.Entities;

[Auditable]
public class Lead : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? Company { get; set; }

    public decimal? EstimatedValue { get; set; }

    public EnumLeadStatus Status { get; set; } = EnumLeadStatus.New;

    [StringLength(100)]
    public string? Source { get; set; }

    public long? AssignedToUserId { get; set; }
    public virtual User? AssignedToUser { get; set; }

    public long? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
