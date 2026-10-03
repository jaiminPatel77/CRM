using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Opportunities.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Opportunities.Queries;

public record GetOpportunityByIdQuery(long Id) : IRequest<OpportunityDto?>;

public class GetOpportunityByIdQueryHandler : IRequestHandler<GetOpportunityByIdQuery, OpportunityDto?>
{
    private readonly IApplicationDbContext _context;

    public GetOpportunityByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OpportunityDto?> Handle(GetOpportunityByIdQuery request, CancellationToken cancellationToken)
    {
        var opportunity = await _context.Opportunities
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (opportunity == null) return null;

        return new OpportunityDto
        {
            Id = opportunity.Id,
            TenantId = opportunity.TenantId,
            Title = opportunity.Title,
            Amount = opportunity.Amount,
            Stage = opportunity.Stage,
            Probability = opportunity.Probability,
            ExpectedCloseDate = opportunity.ExpectedCloseDate,
            CustomerId = opportunity.CustomerId,
            LeadId = opportunity.LeadId,
            AssignedToUserId = opportunity.AssignedToUserId,
            Notes = opportunity.Notes,
            CreatedOn = opportunity.CreatedOn,
            ModifiedOn = opportunity.ModifiedOn
        };
    }
}
