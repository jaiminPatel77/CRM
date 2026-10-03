using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Activities.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Activities.Queries;

public record GetActivityByIdQuery(long Id) : IRequest<ActivityDto?>;

public class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, ActivityDto?>
{
    private readonly IApplicationDbContext _context;

    public GetActivityByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityDto?> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        var activity = await _context.Activities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (activity == null) return null;

        return new ActivityDto
        {
            Id = activity.Id,
            TenantId = activity.TenantId,
            Subject = activity.Subject,
            Type = activity.Type,
            DueDate = activity.DueDate,
            IsCompleted = activity.IsCompleted,
            Description = activity.Description,
            CustomerId = activity.CustomerId,
            LeadId = activity.LeadId,
            OpportunityId = activity.OpportunityId,
            AssignedToUserId = activity.AssignedToUserId,
            CreatedOn = activity.CreatedOn,
            ModifiedOn = activity.ModifiedOn
        };
    }
}
