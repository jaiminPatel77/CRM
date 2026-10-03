using Crm.Application.Common.Models;
using Crm.Application.Features.Users;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

// --------------------------------------------------------------------------------------------------
// USERS CONTROLLER
// Handles User Creation, Listing, and Role Assignment.
// Note: We are using MediatR for all operations to keep the controller clean.
// --------------------------------------------------------------------------------------------------
[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/Users")]
public class UsersController : ApiControllerBase
{
    // POST api/users
    [HttpPost]
    public async Task<ActionResult<Result<long>>> Create(CreateUserCommand command)
    {
        return await Mediator.Send(command);
    }

    // PUT api/users/{id}
    [HttpPut("{id}")]
    // [Authorize(Policy = Permissions.Users.Edit)]
    public async Task<ActionResult<Result<long>>> Update(long id, UpdateUserCommand command)
    {
        if (id != command.Id) return BadRequest();
        return await Mediator.Send(command);
    }

    // DELETE api/users/{id}
    [HttpDelete("{id}")]
    // [Authorize(Policy = Permissions.Users.Delete)]
    public async Task<ActionResult<Result<bool>>> Delete(long id)
    {
        return await Mediator.Send(new DeleteUserCommand { Id = id });
    }

    // GET api/users?page=1&pageSize=10
    [HttpGet]
    // [Authorize(Policy = Permissions.Users.View)] 
    public async Task<ActionResult<Result<Paging<UserDto>>>> GetUsers([FromQuery] GetUsersWithPaginationQuery query)
    {
        return await Mediator.Send(query);
    }

    // GET api/users/{id}
    [HttpGet("{id}")]
    // [Authorize(Policy = Permissions.Users.View)]
    public async Task<ActionResult<Result<UserDto>>> GetUser(long id)
    {
        return await Mediator.Send(new GetUserByIdQuery { Id = id });
    }

    // GET api/users/user-with-permissions/{id}
    [HttpGet("user-with-permissions/{id}")]
    public async Task<ActionResult<Result<UserWithPermissionsDto>>> GetUserWithPermissions(long id)
    {
        return await Mediator.Send(new GetUserWithPermissionsQuery { Id = id });
    }

    // POST api/users/{id}/roles
    [HttpPost("{id}/roles")]
    // [Authorize(Policy = Permissions.Users.Edit)]
    public async Task<ActionResult<Result<bool>>> AssignRoles(long id, AssignUserRolesCommand command)
    {
        if (id != command.Id) return BadRequest();
        return await Mediator.Send(command);
    }

    // GET api/users/lookup-list
    [HttpGet("lookup-list")]
    public async Task<ActionResult<Result<List<UserLookUpDto>>>> GetLookupList()
    {
        return await Mediator.Send(new GetUserLookupListQuery());
    }

    // GET api/users/user-profile/{id}
    [HttpGet("user-profile/{id}")]
    public async Task<ActionResult<Result<UserProfileDto>>> GetUserProfile(long id)
    {
        return await Mediator.Send(new GetUserProfileQuery { Id = id });
    }

    // PATCH api/users/user-profile
    [HttpPatch("user-profile")]
    public async Task<ActionResult<Result<bool>>> UpdateUserProfile(UpdateUserProfileCommand command)
    {
        return await Mediator.Send(command);
    }

    // PATCH api/users/change-password
    [HttpPatch("change-password")]
    public async Task<ActionResult<Result<bool>>> ChangePassword(ChangePasswordCommand command)
    {
        return await Mediator.Send(command);
    }

    // POST api/users/admin-invite-user
    [HttpPost("admin-invite-user")]
    public async Task<ActionResult<Result<bool>>> AdminInviteUser(AdminInviteUserCommand command)
    {
        return await Mediator.Send(command);
    }

    // POST api/users/admin-reset-password
    [HttpPost("admin-reset-password")]
    public async Task<ActionResult<Result<bool>>> AdminResetPassword(AdminResetPasswordCommand command)
    {
        return await Mediator.Send(command);
    }
}
