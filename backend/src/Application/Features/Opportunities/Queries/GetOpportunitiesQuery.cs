using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Opportunities.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Opportunities.Queries;

public record GetOpportunitiesQuery : IRequest<List<OpportunityDto>>;

public class GetOpportunitiesQueryHandler : IRequestHandler<GetOpportunitiesQuery, List<OpportunityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOpportunitiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OpportunityDto>> Handle(GetOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Opportunities
            .AsNoTracking()
            .Select(o => new OpportunityDto
            {
                Id = o.Id,
                TenantId = o.TenantId,
                Title = o.Title,
                Amount = o.Amount,
                Stage = o.Stage,
                Probability = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate,
                CustomerId = o.CustomerId,
                LeadId = o.LeadId,
                AssignedToUserId = o.AssignedToUserId,
                Notes = o.Notes,
                CreatedOn = o.CreatedOn,
                ModifiedOn = o.ModifiedOn
            })
            .ToListAsync(cancellationToken);
    }
}
