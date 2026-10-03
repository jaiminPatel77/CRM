using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Tenants.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid TenantGuid) : IRequest<TenantDto?>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly IApplicationDbContext _context;

    public GetTenantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantGuid == request.TenantGuid, cancellationToken);

        if (tenant == null) return null;

        return new TenantDto
        {
            Id = tenant.Id,
            TenantGuid = tenant.TenantGuid,
            Name = tenant.Name,
            Identifier = tenant.Identifier,
            IsActive = tenant.IsActive,
            ConnectionString = tenant.ConnectionString,
            SubscriptionPlan = tenant.SubscriptionPlan,
            SubscriptionExpiresAt = tenant.SubscriptionExpiresAt,
            CreatedOn = tenant.CreatedOn,
            ModifiedOn = tenant.ModifiedOn
        };
    }
}
