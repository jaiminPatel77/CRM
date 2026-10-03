using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Domain.Common;

public abstract class BaseEntity
{
    [Key]
    public long Id { get; set; }

    [NotMapped]
    public List<object> DomainEvents { get; } = new();

    public void AddDomainEvent(object domainEvent)
    {
        DomainEvents.Add(domainEvent);
    }
}
