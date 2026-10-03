namespace Crm.Application.Common.Models;

public abstract class BaseDto<TId>
{
    public TId Id { get; set; } = default!;
}

public abstract class BaseAuditableDto<TId> : BaseDto<TId>
{
    public long? CreatedById { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public long? ModifiedById { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
}
