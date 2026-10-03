using Crm.Domain.Consts;

namespace Crm.Application.Features.Opportunities.DTOs;

public class OpportunityDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public EnumOpportunityStage Stage { get; set; }
    public int Probability { get; set; }
    public DateTimeOffset? ExpectedCloseDate { get; set; }
    public long? CustomerId { get; set; }
    public long? LeadId { get; set; }
    public long? AssignedToUserId { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset ModifiedOn { get; set; }
}
