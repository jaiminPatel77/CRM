using Crm.Application.Common.Models;
using Crm.Application.Features.Settings;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

// --------------------------------------------------------------------------------------------------
// SETTINGS CONTROLLER
// Handles Application Settings.
// Inherits from Generic Base for standard CRUD.
// Adds custom endpoint for getting settings by Key.
// --------------------------------------------------------------------------------------------------
[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class SettingsController : ApiGenericControllerBase<
    CreateSettingCommand,
    UpdateSettingCommand,
    DeleteSettingCommand,
    GetSettingQuery,
    GetSettingsWithPaginationQuery,
    SettingDto>
{
    // Custom Endpoint: Get By Key
    [HttpGet("key/{key}")]
    [ProducesResponseType(typeof(Result<SettingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<SettingDto>>> GetByKey(string key)
    {
        return await Mediator.Send(new GetSettingByKeyQuery(key));
    }

    // SMTP Setting Endpoints
    [HttpGet("smpt-setting")]
    [ProducesResponseType(typeof(Result<SettingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<SettingDto>>> GetSMTPSetting()
    {
        return await Mediator.Send(new GetSMTPSettingQuery());
    }

    [HttpPut("smpt-setting")]
    [ProducesResponseType(typeof(Result<SettingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<SettingDto>>> UpdateSMTPSetting([FromBody] UpdateSMTPSettingCommand command)
    {
        return await Mediator.Send(command);
    }
}
