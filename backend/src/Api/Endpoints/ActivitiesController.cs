using Crm.Api.Controllers;
using Crm.Application.Features.Activities.Commands;
using Crm.Application.Features.Activities.Queries;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Endpoints;

[Authorize]
public class ActivitiesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActivities()
    {
        var result = await Mediator.Send(new GetActivitiesQuery());
        return this.OkResponse(EnumEntityType.ACTIVITY, EnumEntityEvents.COMMON_LIST_ALL_ITEMS, result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetActivityById(long id)
    {
        var result = await Mediator.Send(new GetActivityByIdQuery(id));
        if (result == null)
        {
            return this.CreateBadRequest(EnumEntityType.ACTIVITY, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, "Activity not found.");
        }
        return this.OkResponse(EnumEntityType.ACTIVITY, EnumEntityEvents.COMMON_GET_ITEM, result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateActivity([FromBody] CreateActivityCommand command)
    {
        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.ACTIVITY, EnumEntityEvents.COMMON_CREATE_ITEM, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateActivity(long id, [FromBody] UpdateActivityCommand command)
    {
        if (id != command.Id)
        {
            return this.CreateBadRequest(EnumEntityType.ACTIVITY, EnumEntityEvents.COMMON_UPDATE_ITEM, "Route ID does not match request body ID.");
        }

        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.ACTIVITY, EnumEntityEvents.COMMON_UPDATE_ITEM, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteActivity(long id)
    {
        var result = await Mediator.Send(new DeleteActivityCommand(id));
        if (!result)
        {
            return this.CreateBadRequest(EnumEntityType.ACTIVITY, EnumEntityEvents.COMMON_DELETE_ITEM, "Activity not found or could not be deleted.");
        }
        return this.OkResponse(EnumEntityType.ACTIVITY, EnumEntityEvents.COMMON_DELETE_ITEM, result);
    }
}
