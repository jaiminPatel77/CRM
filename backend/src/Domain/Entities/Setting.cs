using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class Setting : AuditableEntity
{
    public string Key { get; set; } = string.Empty; // e.g., "SMTP", "SMS"
    public string Value { get; set; } = string.Empty; // JSON content
    public string? Description { get; set; }
}
