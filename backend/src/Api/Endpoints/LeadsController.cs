using Crm.Api.Controllers;
using Crm.Application.Features.Leads.Commands;
using Crm.Application.Features.Leads.Queries;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Endpoints;

[Authorize]
public class LeadsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLeads()
    {
        var result = await Mediator.Send(new GetLeadsQuery());
        return this.OkResponse(EnumEntityType.LEAD, EnumEntityEvents.COMMON_LIST_ALL_ITEMS, result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetLeadById(long id)
    {
        var result = await Mediator.Send(new GetLeadByIdQuery(id));
        if (result == null)
        {
            return this.CreateBadRequest(EnumEntityType.LEAD, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, "Lead not found.");
        }
        return this.OkResponse(EnumEntityType.LEAD, EnumEntityEvents.COMMON_GET_ITEM, result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLead([FromBody] CreateLeadCommand command)
    {
        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.LEAD, EnumEntityEvents.COMMON_CREATE_ITEM, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateLead(long id, [FromBody] UpdateLeadCommand command)
    {
        if (id != command.Id)
        {
            return this.CreateBadRequest(EnumEntityType.LEAD, EnumEntityEvents.COMMON_UPDATE_ITEM, "Route ID does not match request body ID.");
        }

        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.LEAD, EnumEntityEvents.COMMON_UPDATE_ITEM, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteLead(long id)
    {
        var result = await Mediator.Send(new DeleteLeadCommand(id));
        if (!result)
        {
            return this.CreateBadRequest(EnumEntityType.LEAD, EnumEntityEvents.COMMON_DELETE_ITEM, "Lead not found or could not be deleted.");
        }
        return this.OkResponse(EnumEntityType.LEAD, EnumEntityEvents.COMMON_DELETE_ITEM, result);
    }
}
