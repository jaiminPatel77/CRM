using System.Security.Claims;
using Crm.Application.Common.Exceptions;
using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Features;
using Crm.Application.Common.Models;
using Crm.Application.Common.Security;
using Crm.Domain.Consts;
using Crm.Domain.Entities;
using FluentValidation;
using Gridify;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Crm.Application.Features.Users; // For UserDto

// This namespace hosts all commands/queries/dto for Roles
namespace Crm.Application.Features.Roles;

#region DTOs
public class RoleDto : BaseDto<long>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // Permissions are usually fetched separately or via detail view, not list
    public IList<UserDto> Users { get; set; } = new List<UserDto>();
}

public class RoleLookUpDto : BaseDto<long>
{
    public string Name { get; set; } = string.Empty;
    public EnumRoleType RoleType { get; set; }
}
#endregion

#region Lookup List
public record GetRoleLookupListQuery : IRequest<Result<List<RoleLookUpDto>>> { }

public class GetRoleLookupListQueryHandler : IRequestHandler<GetRoleLookupListQuery, Result<List<RoleLookUpDto>>>
{
    private readonly RoleManager<Role> _roleManager;
    public GetRoleLookupListQueryHandler(RoleManager<Role> roleManager) => _roleManager = roleManager;

    public async Task<Result<List<RoleLookUpDto>>> Handle(GetRoleLookupListQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .Select(r => new RoleLookUpDto
            {
                Id = r.Id,
                Name = r.Name!,
                RoleType = r.RoleType
            })
            .ToListAsync(cancellationToken);

        return Result<List<RoleLookUpDto>>.Success(roles);
    }
}
#endregion

#region List (Manual)
public class GetRolesWithPaginationQuery : GridifyQuery, IRequest<Result<Paging<RoleDto>>> { }

public class GetRolesWithPaginationQueryHandler : IRequestHandler<GetRolesWithPaginationQuery, Result<Paging<RoleDto>>>
{
    private readonly RoleManager<Role> _roleManager;
    public GetRolesWithPaginationQueryHandler(RoleManager<Role> roleManager) => _roleManager = roleManager;

    public async Task<Result<Paging<RoleDto>>> Handle(GetRolesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.OrderBy))
        {
            request.OrderBy = "Id desc";
        }

        var filteredQuery = _roleManager.Roles.ApplyFiltering(request);
        var count = await filteredQuery.CountAsync(cancellationToken);
        var list = await filteredQuery.ApplyOrdering(request).ApplyPaging(request).ToListAsync(cancellationToken);
        
        var dtos = list.Select(x => new RoleDto { Id = x.Id, Name = x.Name!, Description = x.Description }).ToList();
        return Result<Paging<RoleDto>>.Success(new Paging<RoleDto>(count, dtos));
    }
}
#endregion

#region Get By Id
public record GetRoleByIdQuery : BaseIdCommand<long, Result<RoleDto>> { }

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public GetRoleByIdQueryHandler(RoleManager<Role> roleManager, UserManager<User> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        if (role == null) throw new NotFoundException(nameof(Role), request.Id);

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

        var dto = new RoleDto 
        { 
            Id = role.Id, 
            Name = role.Name!, 
            Description = role.Description,
            Users = usersInRole.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email!,
                FullName = u.FullName,
                UserType = u.UserType
            }).ToList()
        };
        return Result<RoleDto>.Success(dto);
    }
}
#endregion

#region Get Permissions List
public record GetPermissionsQuery : IRequest<Result<List<string>>>;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<List<string>>>
{
    public Task<Result<List<string>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        // Reflection to get all constants from Permissions class
        return Task.FromResult(Result<List<string>>.Success(Permissions.GetAll()));
    }
}
#endregion

#region Create (Manual)
public record CreateRoleCommand : IRequest<Result<long>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(50);
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<long>>
{
    private readonly RoleManager<Role> _roleManager;
    public CreateRoleCommandHandler(RoleManager<Role> roleManager) => _roleManager = roleManager;

    public async Task<Result<long>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await _roleManager.RoleExistsAsync(request.Name))
        {
            return Result<long>.Failure(new[] { $"Role '{request.Name}' already exists." });
        }

        var role = new Role(request.Name) { Description = request.Description };
        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded) return Result<long>.Success(role.Id);
        return Result<long>.Failure(result.Errors.Select(e => e.Description));
    }
}
#endregion

#region Update
public record UpdateRoleCommand : BaseIdCommand<long, Result<long>>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(50);
    }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<long>>
{
    private readonly RoleManager<Role> _roleManager;
    public UpdateRoleCommandHandler(RoleManager<Role> roleManager) => _roleManager = roleManager;

    public async Task<Result<long>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        if (role == null) throw new NotFoundException(nameof(Role), request.Id);

        role.Name = request.Name;
        role.Description = request.Description;

        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded) return Result<long>.Success(role.Id);
        return Result<long>.Failure(result.Errors.Select(e => e.Description));
    }
}
#endregion

#region Delete
public record DeleteRoleCommand : BaseIdCommand<long, Result<bool>> { }

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<bool>>
{
    private readonly RoleManager<Role> _roleManager;
    public DeleteRoleCommandHandler(RoleManager<Role> roleManager) => _roleManager = roleManager;

    public async Task<Result<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        if (role == null) throw new NotFoundException(nameof(Role), request.Id);

        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded) return Result<bool>.Success(true);
        return Result<bool>.Failure(result.Errors.Select(e => e.Description));
    }
}
#endregion

#region Update Permissions (Manual)
public record UpdateRolePermissionsCommand : IRequest<Result<bool>>
{
    public long RoleId { get; init; }
    public List<string> Permissions { get; init; } = new();
}

public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, Result<bool>>
{
    private readonly RoleManager<Role> _roleManager;
    public UpdateRolePermissionsCommandHandler(RoleManager<Role> roleManager) => _roleManager = roleManager;

    public async Task<Result<bool>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
        if (role == null) throw new NotFoundException(nameof(Role), request.RoleId);

        // 1. Get existing claims
        var claims = await _roleManager.GetClaimsAsync(role);
        var permissionClaims = claims.Where(c => c.Type == "Permission").ToList();

        // 2. Remove existing permissions
        foreach (var claim in permissionClaims)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }

        // 3. Add new permissions
        foreach (var permission in request.Permissions)
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
        }

        return Result<bool>.Success(true);
    }
}
#endregion
