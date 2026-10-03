using Crm.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Tenants.Commands.ToggleTenantStatus;

public record ToggleTenantStatusCommand(Guid TenantGuid, bool IsActive) : IRequest<bool>;

public class ToggleTenantStatusCommandHandler : IRequestHandler<ToggleTenantStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ToggleTenantStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ToggleTenantStatusCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.TenantGuid == request.TenantGuid, cancellationToken);

        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with Guid '{request.TenantGuid}' was not found.");
        }

        tenant.IsActive = request.IsActive;
        tenant.ModifiedOn = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
