namespace Crm.Domain.Common;

public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    public DateTimeOffset CreatedOn { get; set; }
    public long? CreatedById { get; set; }
    public DateTimeOffset ModifiedOn { get; set; }
    public long? ModifiedById { get; set; }
}
