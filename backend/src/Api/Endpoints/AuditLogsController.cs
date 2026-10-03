using Crm.Application.Common.Models;
using Crm.Application.Features.AuditLogs;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Authorize] // Probably restrict to Admin, but for now simple Authorize
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuditLogsController : ApiControllerBase
{
    // GET api/auditlogs?page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<Result<Paging<AuditLogDto>>>> GetAuditLogs([FromQuery] GetAuditLogsWithPaginationQuery query)
    {
        return await Mediator.Send(query);
    }
}
