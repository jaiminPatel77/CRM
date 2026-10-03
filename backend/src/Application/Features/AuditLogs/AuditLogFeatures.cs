using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Features;
using Crm.Application.Common.Models;
using Crm.Domain.Entities;
using Gridify;
using MediatR;

namespace Crm.Application.Features.AuditLogs;

#region DTOs
public class AuditLogDto : BaseDto<long>
{
    public string? UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string PrimaryKey { get; set; } = string.Empty;
}
#endregion

#region List
public class GetAuditLogsWithPaginationQuery : GridifyQuery, IRequest<Result<Paging<AuditLogDto>>> { }

public class GetAuditLogsWithPaginationQueryHandler : BaseListQueryHandler<AuditLog, AuditLogDto, GetAuditLogsWithPaginationQuery>
{
    public GetAuditLogsWithPaginationQueryHandler(IApplicationDbContext context) : base(context) { }

    public override AuditLogDto MapToDto(AuditLog entity) => new AuditLogDto
    {
        Id = entity.Id,
        UserId = entity.UserId,
        Type = entity.Type,
        TableName = entity.TableName,
        DateTime = entity.DateTime,
        OldValues = entity.OldValues,
        NewValues = entity.NewValues,
        AffectedColumns = entity.AffectedColumns,
        PrimaryKey = entity.PrimaryKey
    };
}
#endregion
