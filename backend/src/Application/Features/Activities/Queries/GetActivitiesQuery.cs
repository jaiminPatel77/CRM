using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Activities.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Activities.Queries;

public record GetActivitiesQuery : IRequest<List<ActivityDto>>;

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, List<ActivityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActivitiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActivityDto>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Activities
            .AsNoTracking()
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                TenantId = a.TenantId,
                Subject = a.Subject,
                Type = a.Type,
                DueDate = a.DueDate,
                IsCompleted = a.IsCompleted,
                Description = a.Description,
                CustomerId = a.CustomerId,
                LeadId = a.LeadId,
                OpportunityId = a.OpportunityId,
                AssignedToUserId = a.AssignedToUserId,
                CreatedOn = a.CreatedOn,
                ModifiedOn = a.ModifiedOn
            })
            .ToListAsync(cancellationToken);
    }
}
