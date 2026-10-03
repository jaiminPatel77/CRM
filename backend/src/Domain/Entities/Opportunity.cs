using System.ComponentModel.DataAnnotations;
using Crm.Domain.Attributes;
using Crm.Domain.Common;
using Crm.Domain.Consts;

namespace Crm.Domain.Entities;

[Auditable]
public class Opportunity : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public EnumOpportunityStage Stage { get; set; } = EnumOpportunityStage.Qualification;

    public int Probability { get; set; } = 10;

    public DateTimeOffset? ExpectedCloseDate { get; set; }

    public long? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    public long? LeadId { get; set; }
    public virtual Lead? Lead { get; set; }

    public long? AssignedToUserId { get; set; }
    public virtual User? AssignedToUser { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
