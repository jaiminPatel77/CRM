using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string? UserId { get; set; }
    public string Type { get; set; } = string.Empty; // Create, Update, Delete
    public string TableName { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? AffectedColumns { get; set; } // JSON
    public string PrimaryKey { get; set; } = string.Empty;
}
