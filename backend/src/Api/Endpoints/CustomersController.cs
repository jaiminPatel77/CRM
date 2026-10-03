using Crm.Api.Controllers;
using Crm.Application.Features.Customers.Commands;
using Crm.Application.Features.Customers.Queries;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Endpoints;

[Authorize]
public class CustomersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var result = await Mediator.Send(new GetCustomersQuery());
        return this.OkResponse(EnumEntityType.CUSTOMER, EnumEntityEvents.COMMON_LIST_ALL_ITEMS, result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetCustomerById(long id)
    {
        var result = await Mediator.Send(new GetCustomerByIdQuery(id));
        if (result == null)
        {
            return this.CreateBadRequest(EnumEntityType.CUSTOMER, EnumEntityEvents.COMMON_GET_ITEM_NOTFOUND, "Customer not found.");
        }
        return this.OkResponse(EnumEntityType.CUSTOMER, EnumEntityEvents.COMMON_GET_ITEM, result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.CUSTOMER, EnumEntityEvents.COMMON_CREATE_ITEM, result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateCustomer(long id, [FromBody] UpdateCustomerCommand command)
    {
        if (id != command.Id)
        {
            return this.CreateBadRequest(EnumEntityType.CUSTOMER, EnumEntityEvents.COMMON_UPDATE_ITEM, "Route ID does not match request body ID.");
        }

        var result = await Mediator.Send(command);
        return this.OkResponse(EnumEntityType.CUSTOMER, EnumEntityEvents.COMMON_UPDATE_ITEM, result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteCustomer(long id)
    {
        var result = await Mediator.Send(new DeleteCustomerCommand(id));
        if (!result)
        {
            return this.CreateBadRequest(EnumEntityType.CUSTOMER, EnumEntityEvents.COMMON_DELETE_ITEM, "Customer not found or could not be deleted.");
        }
        return this.OkResponse(EnumEntityType.CUSTOMER, EnumEntityEvents.COMMON_DELETE_ITEM, result);
    }
}
