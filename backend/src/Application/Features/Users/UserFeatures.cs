using Crm.Application.Common.Exceptions;
using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Features;
using Crm.Application.Common.Models;
using Crm.Domain.Consts;
using Crm.Domain.Entities;
using FluentValidation;
using Gridify;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

// This namespace hosts all commands/queries/dto for Users
// Note: Users often require UserManager for identity operations (password hashing, roles),
// so we manually implement Create/Update handlers instead of using GenericBaseFeatures.
namespace Crm.Application.Features.Users;

#region DTOs
public class UserDto : BaseDto<long>
{
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public EnumUserType UserType { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public string? Title { get; set; }
}

public class UserLookUpDto : BaseDto<long>
{
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsImageAvailable { get; set; }
    public bool IsDisabled { get; set; }
    public EnumUserType UserType { get; set; }
}

public class UserProfileDto : BaseDto<long>
{
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsImageAvailable { get; set; }
    public string? Image { get; set; }
}
#endregion

#region Lookup List
public record GetUserLookupListQuery : IRequest<Result<List<UserLookUpDto>>> { }

public class GetUserLookupListQueryHandler : IRequestHandler<GetUserLookupListQuery, Result<List<UserLookUpDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetUserLookupListQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<UserLookUpDto>>> Handle(GetUserLookupListQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Select(u => new UserLookUpDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Title = u.Title,
                PhoneNumber = u.PhoneNumber,
                IsDisabled = u.Disabled,
                UserType = u.UserType
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserLookUpDto>>.Success(users);
    }
}
#endregion

#region Get User Profile
public record GetUserProfileQuery : BaseIdCommand<long, Result<UserProfileDto>> { }

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly IApplicationDbContext _context;

    public GetUserProfileQueryHandler(UserManager<User> userManager, IApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new NotFoundException(nameof(User), request.Id);

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        var dto = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Title = user.Title,
            PhoneNumber = user.PhoneNumber,
            IsImageAvailable = profile?.Image != null,
            Image = profile?.Image
        };

        return Result<UserProfileDto>.Success(dto);
    }
}
#endregion

#region Change Password
public record ChangePasswordCommand : IRequest<Result<bool>>
{
    public string UserId { get; init; } = string.Empty;
    public string OldPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IAuthService _authService;

    public ChangePasswordCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var changeDto = new ChangePasswordDto
        {
            UserId = request.UserId,
            OldPassword = request.OldPassword,
            NewPassword = request.NewPassword
        };
        return await _authService.ChangePasswordAsync(changeDto);
    }
}
#endregion

#region Admin Invite User
public record AdminInviteUserCommand : IRequest<Result<bool>>
{
    public long UserId { get; init; }
    public string CallbackUrl { get; init; } = string.Empty;
}

public class AdminInviteUserCommandHandler : IRequestHandler<AdminInviteUserCommand, Result<bool>>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    public AdminInviteUserCommandHandler(UserManager<User> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task<Result<bool>> Handle(AdminInviteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) throw new NotFoundException(nameof(User), request.UserId);

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedCode = System.Net.WebUtility.UrlEncode(code);
        var callbackUrl = $"{request.CallbackUrl}?code={encodedCode}&email={System.Net.WebUtility.UrlEncode(user.Email!)}";

        await _emailSender.SendUserInvitationEmailAsync(user, user, callbackUrl);
        
        // Update user status to Invited
        user.Status = EnumUserStatus.Invited;
        await _userManager.UpdateAsync(user);

        return Result<bool>.Success(true);
    }
}
#endregion

#region Admin Reset Password
public record AdminResetPasswordCommand : IRequest<Result<bool>>
{
    public long UserId { get; init; }
    public string CallbackUrl { get; init; } = string.Empty;
}

public class AdminResetPasswordCommandHandler : IRequestHandler<AdminResetPasswordCommand, Result<bool>>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    public AdminResetPasswordCommandHandler(UserManager<User> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task<Result<bool>> Handle(AdminResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) throw new NotFoundException(nameof(User), request.UserId);

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedCode = System.Net.WebUtility.UrlEncode(code);
        var callbackUrl = $"{request.CallbackUrl}?code={encodedCode}&email={System.Net.WebUtility.UrlEncode(user.Email!)}";

        await _emailSender.SendUserResetPasswordEmailAsync(user, callbackUrl);
        
        // Update user status
        user.Status = EnumUserStatus.AdminResetPassword;
        await _userManager.UpdateAsync(user);

        return Result<bool>.Success(true);
    }
}
#endregion

#region List
public class GetUsersWithPaginationQuery : GridifyQuery, IRequest<Result<Paging<UserDto>>> { }

// Manual implementation because User inherits IdentityUser, not BaseEntity
public class GetUsersWithPaginationQueryHandler : IRequestHandler<GetUsersWithPaginationQuery, Result<Paging<UserDto>>>
{
    private readonly IApplicationDbContext _context;
    public GetUsersWithPaginationQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<Paging<UserDto>>> Handle(GetUsersWithPaginationQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.OrderBy))
        {
            request.OrderBy = "Id desc";
        }

        var filteredQuery = _context.Users.ApplyFiltering(request);
        var count = await filteredQuery.CountAsync(cancellationToken);
        var list = await filteredQuery.ApplyOrdering(request).ApplyPaging(request).ToListAsync(cancellationToken);
        
        var dtos = list.Select(entity => new UserDto 
        { 
            Id = entity.Id, Email = entity.Email, FullName = entity.FullName, UserType = entity.UserType, Title = entity.Title
        }).ToList();

        return Result<Paging<UserDto>>.Success(new Paging<UserDto>(count, dtos));
    }
}
#endregion

#region Get By Id
public record GetUserByIdQuery : BaseIdCommand<long, Result<UserDto>> { }

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public GetUserByIdQueryHandler(UserManager<User> userManager) => _userManager = userManager;

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _userManager.FindByIdAsync(request.Id.ToString());
        if (entity == null) throw new NotFoundException(nameof(User), request.Id);

        var roles = await _userManager.GetRolesAsync(entity);

        var dto = new UserDto
        {
            Id = entity.Id,
            Email = entity.Email,
            FullName = entity.FullName,
            UserType = entity.UserType,
            Title = entity.Title,
            Roles = roles
        };

        return Result<UserDto>.Success(dto);
    }
}
#endregion

#region Get User With Permissions
public class UserWithPermissionsDto : UserDto
{
    public IList<string> PermissionsList { get; set; } = new List<string>();
}

public record GetUserWithPermissionsQuery : BaseIdCommand<long, Result<UserWithPermissionsDto>> { }

public class GetUserWithPermissionsQueryHandler : IRequestHandler<GetUserWithPermissionsQuery, Result<UserWithPermissionsDto>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public GetUserWithPermissionsQueryHandler(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<UserWithPermissionsDto>> Handle(GetUserWithPermissionsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new NotFoundException(nameof(User), request.Id);

        var rolesStr = await _userManager.GetRolesAsync(user);
        
        // Optimized: Fetch all roles and their claims in parallel
        var rolesTasks = rolesStr.Select(async roleName => 
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return Enumerable.Empty<string>();
            var claims = await _roleManager.GetClaimsAsync(role);
            return claims.Where(c => c.Type == "Permission").Select(c => c.Value);
        });
        
        var allPermissionSets = await Task.WhenAll(rolesTasks);
        var permissions = allPermissionSets.SelectMany(p => p).Distinct().ToList();

        var dto = new UserWithPermissionsDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            UserType = user.UserType,
            Title = user.Title,
            Roles = rolesStr,
            PermissionsList = permissions
        };

        return Result<UserWithPermissionsDto>.Success(dto);
    }
}
#endregion

#region Create
public record CreateUserCommand : IRequest<Result<long>>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Password { get; init; }
    public string? Title { get; init; }
    public EnumUserType UserType { get; init; } = EnumUserType.CustomRoleBase;
    public List<string> Roles { get; init; } = new();
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.FullName).MaximumLength(128).NotEmpty();
        RuleFor(v => v.Email).NotEmpty().EmailAddress();
        RuleFor(v => v.Password).MinimumLength(6).When(v => !string.IsNullOrEmpty(v.Password));
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<long>>
{
    private readonly UserManager<User> _userManager;

    public CreateUserCommandHandler(UserManager<User> userManager) => _userManager = userManager;

    public async Task<Result<long>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            Title = request.Title,
            UserType = request.UserType,
            SecurityStamp = Guid.NewGuid().ToString(),
            Status = EnumUserStatus.Created
        };

        var password = request.Password;
        if (string.IsNullOrEmpty(password))
        {
            password = GenerateRandomPassword();
        }

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            if (request.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!roleResult.Succeeded)
                {
                    return Result<long>.Failure(roleResult.Errors.Select(e => e.Description));
                }
            }
            return Result<long>.Success(user.Id);
        }
        return Result<long>.Failure(result.Errors.Select(e => e.Description));
    }

    private string GenerateRandomPassword()
    {
        var options = _userManager.Options.Password;

        int length = options.RequiredLength < 12 ? 12 : options.RequiredLength;

        bool nonAlphanumeric = options.RequireNonAlphanumeric;
        bool digit = options.RequireDigit;
        bool lowercase = options.RequireLowercase;
        bool uppercase = options.RequireUppercase;

        StringBuilder password = new StringBuilder();
        Random random = new Random();

        while (password.Length < length)
        {
            char c = (char)random.Next(33, 126);

            if (char.IsDigit(c) && digit) digit = false;
            else if (char.IsLower(c) && lowercase) lowercase = false;
            else if (char.IsUpper(c) && uppercase) uppercase = false;
            else if (!char.IsLetterOrDigit(c) && nonAlphanumeric) nonAlphanumeric = false;
            else if (char.IsLetterOrDigit(c)) { } // Accept letters/digits
            else continue;

            password.Append(c);
        }

        return password.ToString();
    }
}
#endregion

#region Update
public record UpdateUserCommand : BaseIdCommand<long, Result<long>>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Title { get; init; }
    public EnumUserType UserType { get; init; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<long>>
{
    private readonly UserManager<User> _userManager;

    public UpdateUserCommandHandler(UserManager<User> userManager) => _userManager = userManager;

    public async Task<Result<long>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new NotFoundException(nameof(User), request.Id);

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.UserName = request.Email; // Keep username synced with email
        user.Title = request.Title;
        user.UserType = request.UserType;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded) return Result<long>.Success(user.Id);
        return Result<long>.Failure(result.Errors.Select(e => e.Description));
    }
}
#endregion

#region Delete
public record DeleteUserCommand : BaseIdCommand<long, Result<bool>> { }

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    private readonly UserManager<User> _userManager;

    public DeleteUserCommandHandler(UserManager<User> userManager) => _userManager = userManager;

    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new NotFoundException(nameof(User), request.Id);

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded) return Result<bool>.Success(true);
        return Result<bool>.Failure(result.Errors.Select(e => e.Description));
    }
}
#endregion

#region Assign Roles
public record AssignUserRolesCommand : BaseIdCommand<long, Result<bool>>
{
    public List<string> Roles { get; init; } = new();
}

public class AssignUserRolesCommandHandler : IRequestHandler<AssignUserRolesCommand, Result<bool>>
{
    private readonly UserManager<User> _userManager;

    public AssignUserRolesCommandHandler(UserManager<User> userManager) => _userManager = userManager;

    public async Task<Result<bool>> Handle(AssignUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new NotFoundException(nameof(User), request.Id);

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return Result<bool>.Failure(removeResult.Errors.Select(e => e.Description));

        var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
        if (!addResult.Succeeded) return Result<bool>.Failure(addResult.Errors.Select(e => e.Description));

        return Result<bool>.Success(true);
    }
}
#endregion

#region Update Profile
public record UpdateUserProfileCommand : IRequest<Result<bool>>
{
    public long Id { get; init; } // UserId
    public string FullName { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Image { get; init; } // Base64 or URL
}

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public UpdateUserProfileCommandHandler(IApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new NotFoundException(nameof(User), request.Id);

        // Update User Fields
        user.FullName = request.FullName;
        user.Title = request.Title;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return Result<bool>.Failure(result.Errors.Select(e => e.Description));

        // Update UserProfile Fields
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (profile == null)
        {
            // Create if missing (Shared PK)
            profile = new UserProfile { Id = request.Id, Image = request.Image };
            _context.UserProfiles.Add(profile);
        }
        else
        {
            profile.Image = request.Image;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
#endregion
