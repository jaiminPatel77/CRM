using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Tenants.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Tenants.Queries.GetTenants;

public record GetTenantsQuery : IRequest<List<TenantDto>>;

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, List<TenantDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tenants
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedOn)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                TenantGuid = t.TenantGuid,
                Name = t.Name,
                Identifier = t.Identifier,
                IsActive = t.IsActive,
                ConnectionString = t.ConnectionString,
                SubscriptionPlan = t.SubscriptionPlan,
                SubscriptionExpiresAt = t.SubscriptionExpiresAt,
                CreatedOn = t.CreatedOn,
                ModifiedOn = t.ModifiedOn
            })
            .ToListAsync(cancellationToken);
    }
}
