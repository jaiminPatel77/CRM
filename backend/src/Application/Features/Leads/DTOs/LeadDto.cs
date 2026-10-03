using Crm.Domain.Consts;

namespace Crm.Application.Features.Leads.DTOs;

public class LeadDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public decimal? EstimatedValue { get; set; }
    public EnumLeadStatus Status { get; set; }
    public string? Source { get; set; }
    public long? AssignedToUserId { get; set; }
    public long? CustomerId { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset ModifiedOn { get; set; }
}
