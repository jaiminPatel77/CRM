using Crm.Api.Controllers;
using Crm.Application.Features.Opportunities.Commands;
using Crm.Application.Features.Opportunities.Queries;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Endpoints;

[Authorize]
public class OpportunitiesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOpportunities()
    {
        var result = await Mediator.Send(new GetOpportunitiesQuery());
        return this.OkResponse(EnumEntityType.OPPORTUNITY, EnumEntityEvents.COMMON_LIST_ALL_ITEMS, result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetOpportunityById(long id)
    {
        var result = await Mediator.Send(new GetOpportunityByIdQuery(id));
        if (result == null)
        {
            return this.CreateBadRequest(EnumEntityType.OPPORTUNITY, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, "Opportunity not found.");
        }
        return this.OkResponse(EnumEntityType.OPPORTUNITY, EnumEntityEvents.COMMON_GET_ITEM, result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOpportunity([FromBody] CreateOpportunityCommand command)
    {
        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.OPPORTUNITY, EnumEntityEvents.COMMON_CREATE_ITEM, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateOpportunity(long id, [FromBody] UpdateOpportunityCommand command)
    {
        if (id != command.Id)
        {
            return this.CreateBadRequest(EnumEntityType.OPPORTUNITY, EnumEntityEvents.COMMON_UPDATE_ITEM, "Route ID does not match request body ID.");
        }

        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.OPPORTUNITY, EnumEntityEvents.COMMON_UPDATE_ITEM, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteOpportunity(long id)
    {
        var result = await Mediator.Send(new DeleteOpportunityCommand(id));
        if (!result)
        {
            return this.CreateBadRequest(EnumEntityType.OPPORTUNITY, EnumEntityEvents.COMMON_DELETE_ITEM, "Opportunity not found or could not be deleted.");
        }
        return this.OkResponse(EnumEntityType.OPPORTUNITY, EnumEntityEvents.COMMON_DELETE_ITEM, result);
    }
}
