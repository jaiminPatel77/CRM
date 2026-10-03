using Crm.Domain.Consts;

namespace Crm.Application.Features.Activities.DTOs;

public class ActivityDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public EnumActivityType Type { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public string? Description { get; set; }
    public long? CustomerId { get; set; }
    public long? LeadId { get; set; }
    public long? OpportunityId { get; set; }
    public long? AssignedToUserId { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset ModifiedOn { get; set; }
}
