using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Leads.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Leads.Queries;

public record GetLeadByIdQuery(long Id) : IRequest<LeadDto?>;

public class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDto?>
{
    private readonly IApplicationDbContext _context;

    public GetLeadByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeadDto?> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (lead == null) return null;

        return new LeadDto
        {
            Id = lead.Id,
            TenantId = lead.TenantId,
            Title = lead.Title,
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            Email = lead.Email,
            Phone = lead.Phone,
            Company = lead.Company,
            EstimatedValue = lead.EstimatedValue,
            Status = lead.Status,
            Source = lead.Source,
            AssignedToUserId = lead.AssignedToUserId,
            CustomerId = lead.CustomerId,
            Notes = lead.Notes,
            CreatedOn = lead.CreatedOn,
            ModifiedOn = lead.ModifiedOn
        };
    }
}
