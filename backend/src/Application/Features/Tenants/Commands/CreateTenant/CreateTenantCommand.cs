using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Tenants.DTOs;
using Crm.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Tenants.Commands.CreateTenant;

public record CreateTenantCommand : IRequest<TenantDto>
{
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public string SubscriptionPlan { get; init; } = "Free";
    public string? ConnectionString { get; init; }
}

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters.");

        RuleFor(v => v.Identifier)
            .NotEmpty().WithMessage("Tenant identifier is required.")
            .MaximumLength(50).WithMessage("Tenant identifier must not exceed 50 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Tenant identifier must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(v => v.SubscriptionPlan)
            .NotEmpty().WithMessage("Subscription plan is required.");
    }
}

public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, TenantDto>
{
    private readonly IApplicationDbContext _context;

    public CreateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var normalizedIdentifier = request.Identifier.Trim().ToLowerInvariant();

        var existingTenant = await _context.Tenants
            .AnyAsync(t => t.Identifier == normalizedIdentifier, cancellationToken);

        if (existingTenant)
        {
            throw new InvalidOperationException($"Tenant identifier '{normalizedIdentifier}' is already taken.");
        }

        var tenant = new Tenant
        {
            TenantGuid = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Identifier = normalizedIdentifier,
            SubscriptionPlan = request.SubscriptionPlan,
            ConnectionString = request.ConnectionString,
            IsActive = true,
            CreatedOn = DateTimeOffset.UtcNow,
            ModifiedOn = DateTimeOffset.UtcNow
        };

        _context.Tenants.Add(tenant);
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
