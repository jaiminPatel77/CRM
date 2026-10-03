using System.ComponentModel.DataAnnotations;
using Crm.Domain.Attributes;
using Crm.Domain.Common;
using Crm.Domain.Consts;

namespace Crm.Domain.Entities;

[Auditable]
public class Activity : AuditableEntity, ITenantEntity
{
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    public EnumActivityType Type { get; set; } = EnumActivityType.Task;

    public DateTimeOffset? DueDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    [StringLength(2000)]
    public string? Description { get; set; }

    public long? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    public long? LeadId { get; set; }
    public virtual Lead? Lead { get; set; }

    public long? OpportunityId { get; set; }
    public virtual Opportunity? Opportunity { get; set; }

    public long? AssignedToUserId { get; set; }
    public virtual User? AssignedToUser { get; set; }
}
