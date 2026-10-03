using Crm.Application.Common.Interfaces;
using MediatR;

namespace Crm.Application.Common.Models;

public abstract record BaseCommand<TResult> : IRequest<TResult>;

public abstract record BaseIdCommand<TId, TResult> : BaseCommand<TResult>, IHasId<TId>
{
    public TId Id { get; set; } = default!;
}
