using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Leads.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Leads.Queries;

public record GetLeadsQuery : IRequest<List<LeadDto>>;

public class GetLeadsQueryHandler : IRequestHandler<GetLeadsQuery, List<LeadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLeadsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeadDto>> Handle(GetLeadsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Leads
            .AsNoTracking()
            .Select(l => new LeadDto
            {
                Id = l.Id,
                TenantId = l.TenantId,
                Title = l.Title,
                FirstName = l.FirstName,
                LastName = l.LastName,
                Email = l.Email,
                Phone = l.Phone,
                Company = l.Company,
                EstimatedValue = l.EstimatedValue,
                Status = l.Status,
                Source = l.Source,
                AssignedToUserId = l.AssignedToUserId,
                CustomerId = l.CustomerId,
                Notes = l.Notes,
                CreatedOn = l.CreatedOn,
                ModifiedOn = l.ModifiedOn
            })
            .ToListAsync(cancellationToken);
    }
}
