using Crm.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Opportunities.Commands;

public record DeleteOpportunityCommand(long Id) : IRequest<bool>;

public class DeleteOpportunityCommandHandler : IRequestHandler<DeleteOpportunityCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteOpportunityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (opportunity == null)
        {
            return false;
        }

        _context.Opportunities.Remove(opportunity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
