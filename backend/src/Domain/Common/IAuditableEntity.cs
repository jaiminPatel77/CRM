namespace Crm.Domain.Common;

public interface IAuditableEntity
{
    DateTimeOffset CreatedOn { get; set; }
    long? CreatedById { get; set; }
    DateTimeOffset ModifiedOn { get; set; }
    long? ModifiedById { get; set; }
}
