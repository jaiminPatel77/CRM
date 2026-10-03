namespace Crm.Application.Common.Interfaces;

public interface IHasId<TId>
{
    TId Id { get; set; }
}
