using Crm.Api.Models;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

public static class ApiControllerExtensions
{
    public static IActionResult OkResponse(this ControllerBase controller, EnumEntityType entityType, EnumEntityEvents eventCode, object? data = null)
    {
        return controller.Ok(new ApiOkResponse(entityType, eventCode, data));
    }

    public static IActionResult CreateBadRequest(this ControllerBase controller, EnumEntityType entityType, EnumEntityEvents eventCode, object? error = null)
    {
        return controller.BadRequest(new ApiBadRequestResponse(entityType, eventCode, error));
    }
    
    public static IActionResult CreateBadRequest(this ControllerBase controller, EnumEntityType entityType, EnumEntityEvents eventCode, string error)
    {
        return controller.BadRequest(new ApiBadRequestResponse(entityType, eventCode, error));
    }
}
