using Crm.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Leads.Commands;

public record DeleteLeadCommand(long Id) : IRequest<bool>;

public class DeleteLeadCommandHandler : IRequestHandler<DeleteLeadCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteLeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (lead == null)
        {
            return false;
        }

        _context.Leads.Remove(lead);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
