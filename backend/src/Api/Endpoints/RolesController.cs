using Crm.Application.Common.Models;
using Crm.Application.Features.Roles;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

// --------------------------------------------------------------------------------------------------
// ROLES CONTROLLER
// Handles Role Creation, Listing, and Permission Management.
// Uses Manual Handlers (Identity) via MediatR.
// --------------------------------------------------------------------------------------------------
[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/Roles")]
public class RolesController : ApiControllerBase
{
    // GET api/roles?page=1&pageSize=10
    [HttpGet]
    // [Authorize(Policy = Permissions.Roles.View)]
    public async Task<ActionResult<Result<Paging<RoleDto>>>> GetRoles([FromQuery] GetRolesWithPaginationQuery query)
    {
        return await Mediator.Send(query);
    }

    // GET api/roles/lookup-list
    [HttpGet("lookup-list")]
    public async Task<ActionResult<Result<List<RoleLookUpDto>>>> GetLookupList()
    {
        return await Mediator.Send(new GetRoleLookupListQuery());
    }

    // GET api/roles/{id}
    [HttpGet("{id}")]
    // [Authorize(Policy = Permissions.Roles.View)]
    public async Task<ActionResult<Result<RoleDto>>> GetRole(long id)
    {
        return await Mediator.Send(new GetRoleByIdQuery { Id = id });
    }

    // GET api/roles/permissions
    [HttpGet("permissions")]
    public async Task<ActionResult<Result<List<string>>>> GetPermissions()
    {
        return await Mediator.Send(new GetPermissionsQuery());
    }

    // POST api/roles
    [HttpPost]
    // [Authorize(Policy = Permissions.Roles.Create)]
    public async Task<ActionResult<Result<long>>> Create(CreateRoleCommand command)
    {
        return await Mediator.Send(command);
    }

    // PUT api/roles/{id}
    [HttpPut("{id}")]
    // [Authorize(Policy = Permissions.Roles.Edit)]
    public async Task<ActionResult<Result<long>>> Update(long id, UpdateRoleCommand command)
    {
        if (id != command.Id) return BadRequest();
        return await Mediator.Send(command);
    }

    // DELETE api/roles/{id}
    [HttpDelete("{id}")]
    // [Authorize(Policy = Permissions.Roles.Delete)]
    public async Task<ActionResult<Result<bool>>> Delete(long id)
    {
        return await Mediator.Send(new DeleteRoleCommand { Id = id });
    }

    // PUT api/roles/{id}/permissions
    [HttpPut("{id}/permissions")]
    // [Authorize(Policy = Permissions.Roles.Edit)]
    public async Task<ActionResult<Result<bool>>> UpdatePermissions(long id, List<string> permissions)
    {
        return await Mediator.Send(new UpdateRolePermissionsCommand { RoleId = id, Permissions = permissions });
    }
}
