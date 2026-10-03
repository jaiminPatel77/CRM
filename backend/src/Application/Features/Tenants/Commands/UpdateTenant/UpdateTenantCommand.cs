using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Tenants.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand : IRequest<TenantDto>
{
    public Guid TenantGuid { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string SubscriptionPlan { get; init; } = "Free";
    public string? ConnectionString { get; init; }
}

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(v => v.TenantGuid)
            .NotEmpty().WithMessage("Tenant Guid is required.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters.");

        RuleFor(v => v.SubscriptionPlan)
            .NotEmpty().WithMessage("Subscription plan is required.");
    }
}

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantDto> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.TenantGuid == request.TenantGuid, cancellationToken);

        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with Guid '{request.TenantGuid}' was not found.");
        }

        tenant.Name = request.Name.Trim();
        tenant.IsActive = request.IsActive;
        tenant.SubscriptionPlan = request.SubscriptionPlan;
        tenant.ConnectionString = request.ConnectionString;
        tenant.ModifiedOn = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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
