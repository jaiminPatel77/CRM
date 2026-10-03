using Crm.Application.Common.Interfaces;
using Crm.Domain.Consts;
using Crm.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Tenants.Commands.ProvisionTenant;

public record ProvisionTenantCommand : IRequest<ProvisionTenantResultDto>
{
    public string TenantName { get; init; } = string.Empty;
    public string TenantIdentifier { get; init; } = string.Empty;
    public string AdminEmail { get; init; } = string.Empty;
    public string AdminFullName { get; init; } = string.Empty;
    public string AdminPassword { get; init; } = string.Empty;
    public string SubscriptionPlan { get; init; } = "Starter";
    public string? ConnectionString { get; init; }
}

public class ProvisionTenantResultDto
{
    public long TenantId { get; set; }
    public Guid TenantGuid { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string TenantIdentifier { get; set; } = string.Empty;
    public long AdminUserId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string SubscriptionPlan { get; set; } = string.Empty;
    public DateTimeOffset CreatedOn { get; set; }
}

public class ProvisionTenantCommandValidator : AbstractValidator<ProvisionTenantCommand>
{
    public ProvisionTenantCommandValidator()
    {
        RuleFor(v => v.TenantName)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters.");

        RuleFor(v => v.TenantIdentifier)
            .NotEmpty().WithMessage("Tenant identifier is required.")
            .MaximumLength(50).WithMessage("Tenant identifier must not exceed 50 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Tenant identifier must contain only lowercase letters, numbers, and hyphens.");

        RuleFor(v => v.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required.")
            .EmailAddress().WithMessage("Admin email must be a valid email address.");

        RuleFor(v => v.AdminFullName)
            .NotEmpty().WithMessage("Admin full name is required.")
            .MaximumLength(128).WithMessage("Admin full name must not exceed 128 characters.");

        RuleFor(v => v.AdminPassword)
            .NotEmpty().WithMessage("Admin password is required.")
            .MinimumLength(8).WithMessage("Admin password must be at least 8 characters long.");
    }
}

public class ProvisionTenantCommandHandler : IRequestHandler<ProvisionTenantCommand, ProvisionTenantResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public ProvisionTenantCommandHandler(
        IApplicationDbContext context,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ProvisionTenantResultDto> Handle(ProvisionTenantCommand request, CancellationToken cancellationToken)
    {
        var normalizedIdentifier = request.TenantIdentifier.Trim().ToLowerInvariant();
        var normalizedEmail = request.AdminEmail.Trim().ToLowerInvariant();

        // 1. Check Tenant Identifier Uniqueness
        var identifierExists = await _context.Tenants
            .AnyAsync(t => t.Identifier == normalizedIdentifier, cancellationToken);

        if (identifierExists)
        {
            throw new InvalidOperationException($"Tenant identifier '{normalizedIdentifier}' is already registered.");
        }

        // 2. Check Admin Email Uniqueness
        var userExists = await _userManager.FindByEmailAsync(normalizedEmail);
        if (userExists != null)
        {
            throw new InvalidOperationException($"Email address '{normalizedEmail}' is already registered to another user.");
        }

        // 3. Create Tenant Entity
        var tenant = new Tenant
        {
            TenantGuid = Guid.NewGuid(),
            Name = request.TenantName.Trim(),
            Identifier = normalizedIdentifier,
            SubscriptionPlan = request.SubscriptionPlan,
            ConnectionString = request.ConnectionString,
            IsActive = true,
            CreatedOn = DateTimeOffset.UtcNow,
            ModifiedOn = DateTimeOffset.UtcNow
        };

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Create Tenant Default Roles
        var tenantAdminRoleName = $"TenantAdmin_{tenant.Identifier}";
        var salesUserRoleName = $"SalesUser_{tenant.Identifier}";

        if (!await _roleManager.RoleExistsAsync(tenantAdminRoleName))
        {
            await _roleManager.CreateAsync(new Role
            {
                Name = tenantAdminRoleName,
                Description = $"Tenant Administrator for {tenant.Name}",
                TenantId = tenant.TenantGuid,
                RoleType = EnumRoleType.EnterpriseAdministrator,
                CreatedOn = DateTimeOffset.UtcNow,
                ModifiedOn = DateTimeOffset.UtcNow
            });
        }

        if (!await _roleManager.RoleExistsAsync(salesUserRoleName))
        {
            await _roleManager.CreateAsync(new Role
            {
                Name = salesUserRoleName,
                Description = $"Sales User for {tenant.Name}",
                TenantId = tenant.TenantGuid,
                RoleType = EnumRoleType.CustomRole,
                CreatedOn = DateTimeOffset.UtcNow,
                ModifiedOn = DateTimeOffset.UtcNow
            });
        }

        // 5. Create Tenant Admin User
        var adminUser = new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = request.AdminFullName.Trim(),
            TenantId = tenant.TenantGuid,
            UserType = EnumUserType.EnterpriseAdministrator,
            Status = EnumUserStatus.Created,
            EmailConfirmed = true,
            CreatedOn = DateTimeOffset.UtcNow,
            ModifiedOn = DateTimeOffset.UtcNow
        };

        var createResult = await _userManager.CreateAsync(adminUser, request.AdminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create tenant admin user: {errors}");
        }

        await _userManager.AddToRoleAsync(adminUser, tenantAdminRoleName);

        return new ProvisionTenantResultDto
        {
            TenantId = tenant.Id,
            TenantGuid = tenant.TenantGuid,
            TenantName = tenant.Name,
            TenantIdentifier = tenant.Identifier,
            AdminUserId = adminUser.Id,
            AdminEmail = adminUser.Email,
            SubscriptionPlan = tenant.SubscriptionPlan,
            CreatedOn = tenant.CreatedOn
        };
    }
}
