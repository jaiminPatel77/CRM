using Crm.Api.Controllers;
using Crm.Application.Features.Tenants.Commands.CreateTenant;
using Crm.Application.Features.Tenants.Commands.ToggleTenantStatus;
using Crm.Application.Features.Tenants.Commands.UpdateTenant;
using Crm.Application.Features.Tenants.DTOs;
using Crm.Application.Features.Tenants.Queries.GetTenantById;
using Crm.Application.Features.Tenants.Queries.GetTenants;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Endpoints;

[Authorize(Roles = "GlobalAdministrator")]
public class TenantsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTenants()
    {
        var result = await Mediator.Send(new GetTenantsQuery());
        return this.OkResponse(EnumEntityType.TENANT, EnumEntityEvents.COMMON_LIST_ALL_ITEMS, result);
    }

    [HttpGet("{tenantGuid:guid}")]
    public async Task<IActionResult> GetTenantById(Guid tenantGuid)
    {
        var result = await Mediator.Send(new GetTenantByIdQuery(tenantGuid));
        if (result == null)
        {
            return this.CreateBadRequest(EnumEntityType.TENANT, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, "Tenant not found.");
        }
        return this.OkResponse(EnumEntityType.TENANT, EnumEntityEvents.COMMON_GET_ITEM, result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.TENANT, EnumEntityEvents.COMMON_CREATE_ITEM, result);
    }

    [HttpPost("provision")]
    public async Task<IActionResult> ProvisionTenant([FromBody] Crm.Application.Features.Tenants.Commands.ProvisionTenant.ProvisionTenantCommand command)
    {
        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.TENANT, EnumEntityEvents.COMMON_CREATE_ITEM, result);
    }

    [HttpPut("{tenantGuid:guid}")]
    public async Task<IActionResult> UpdateTenant(Guid tenantGuid, [FromBody] UpdateTenantCommand command)
    {
        if (tenantGuid != command.TenantGuid)
        {
            return this.CreateBadRequest(EnumEntityType.TENANT, EnumEntityEvents.COMMON_UPDATE_ITEM, "Route ID does not match request body ID.");
        }

        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.TENANT, EnumEntityEvents.COMMON_UPDATE_ITEM, result);
    }

    [HttpPatch("{tenantGuid:guid}/status")]
    public async Task<IActionResult> ToggleTenantStatus(Guid tenantGuid, [FromBody] bool isActive)
    {
        var result = await Mediator.Send(new ToggleTenantStatusCommand(tenantGuid, isActive));
        return this.OkResponse(EnumEntityType.TENANT, EnumEntityEvents.COMMON_UPDATE_ITEM, result);
    }
}
