using Crm.Application.Common.Exceptions;
using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Features;
using Crm.Application.Common.Models;
using Crm.Domain.Entities;
using FluentValidation;
using Gridify;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

// This namespace hosts all commands/queries/dto for Settings
namespace Crm.Application.Features.Settings;

#region DTOs
public class SettingDto : BaseDto<long>
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    // Add other properties if needed based on entity
}
#endregion

#region Get (By Id)
public record GetSettingQuery : IRequest<Result<SettingDto>>, IHasId<long>
{
    public long Id { get; set; }
}

public class GetSettingQueryHandler : BaseGetQueryHandler<Setting, SettingDto, GetSettingQuery>
{
    public GetSettingQueryHandler(IApplicationDbContext context) : base(context) { }

    public override SettingDto MapToDto(Setting entity) => new SettingDto 
    { 
        Id = entity.Id, Key = entity.Key, Value = entity.Value 
    };
}
#endregion

#region Get (By Key) - Custom for Settings
public record GetSettingByKeyQuery(string Key) : IRequest<Result<SettingDto>>;

public class GetSettingByKeyQueryHandler : IRequestHandler<GetSettingByKeyQuery, Result<SettingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly HybridCache _cache; // Inject HybridCache

    public GetSettingByKeyQueryHandler(IApplicationDbContext context, HybridCache cache) 
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<SettingDto>> Handle(GetSettingByKeyQuery request, CancellationToken cancellationToken)
    {
        var dto = await _cache.GetOrCreateAsync(
            $"setting:{request.Key}", 
            async cancel => 
            {
                var entity = await _context.Settings.FirstOrDefaultAsync(x => x.Key == request.Key, cancel);
                if (entity == null) throw new NotFoundException(nameof(Setting), request.Key);
                return new SettingDto { Id = entity.Id, Key = entity.Key, Value = entity.Value };
            },
            cancellationToken: cancellationToken
        );

        return Result<SettingDto>.Success(dto);
    }
}
#endregion

#region List
public class GetSettingsWithPaginationQuery : GridifyQuery, IRequest<Result<Paging<SettingDto>>> { }

public class GetSettingsWithPaginationQueryHandler : BaseListQueryHandler<Setting, SettingDto, GetSettingsWithPaginationQuery>
{
    public GetSettingsWithPaginationQueryHandler(IApplicationDbContext context) : base(context) { }

    public override SettingDto MapToDto(Setting entity) => new SettingDto 
    { 
        Id = entity.Id, Key = entity.Key, Value = entity.Value 
    };
}
#endregion

#region Create
public record CreateSettingCommand : BaseCommand<Result<long>>
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public class CreateSettingCommandValidator : AbstractValidator<CreateSettingCommand>
{
    public CreateSettingCommandValidator()
    {
        RuleFor(v => v.Key).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Value).NotEmpty();
    }
}

public class CreateSettingCommandHandler : BaseCreateCommandHandler<Setting, CreateSettingCommand>
{
    public CreateSettingCommandHandler(IApplicationDbContext context) : base(context) { }

    public override Setting MapToEntity(CreateSettingCommand command) => new Setting 
    { 
        Key = command.Key, Value = command.Value 
    };
}
#endregion

#region Update
public record UpdateSettingCommand : BaseIdCommand<long, Result<long>>
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}

public class UpdateSettingCommandValidator : AbstractValidator<UpdateSettingCommand>
{
    public UpdateSettingCommandValidator()
    {
        RuleFor(v => v.Key).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Value).NotEmpty();
    }
}

public class UpdateSettingCommandHandler : BaseUpdateCommandHandler<Setting, UpdateSettingCommand>
{
    public UpdateSettingCommandHandler(IApplicationDbContext context) : base(context) { }

    public override void MapToEntity(UpdateSettingCommand command, Setting entity)
    {
        entity.Key = command.Key;
        entity.Value = command.Value;
    }
}
#endregion

#region Delete
public record DeleteSettingCommand : BaseIdCommand<long, Result<long>>;

public class DeleteSettingCommandHandler : BaseDeleteCommandHandler<Setting, DeleteSettingCommand>
{
    public DeleteSettingCommandHandler(IApplicationDbContext context) : base(context) { }
}
#endregion

#region SMTP Setting
public class SMTPSettingDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public record GetSMTPSettingQuery : IRequest<Result<SettingDto>> { }

public class GetSMTPSettingQueryHandler : IRequestHandler<GetSMTPSettingQuery, Result<SettingDto>>
{
    private readonly IApplicationDbContext _context;
    private const string SMTPKey = "SMTP_SETTING";

    public GetSMTPSettingQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<SettingDto>> Handle(GetSMTPSettingQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Settings.FirstOrDefaultAsync(x => x.Key == SMTPKey, cancellationToken);
        if (entity == null)
        {
            return Result<SettingDto>.Success(new SettingDto { Key = SMTPKey, Value = "{}" });
        }
        return Result<SettingDto>.Success(new SettingDto { Id = entity.Id, Key = entity.Key, Value = entity.Value });
    }
}

public record UpdateSMTPSettingCommand : IRequest<Result<SettingDto>>
{
    public string Value { get; init; } = string.Empty;
}

public class UpdateSMTPSettingCommandHandler : IRequestHandler<UpdateSMTPSettingCommand, Result<SettingDto>>
{
    private readonly IApplicationDbContext _context;
    private const string SMTPKey = "SMTP_SETTING";

    public UpdateSMTPSettingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<SettingDto>> Handle(UpdateSMTPSettingCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Settings.FirstOrDefaultAsync(x => x.Key == SMTPKey, cancellationToken);
        if (entity == null)
        {
            entity = new Setting { Key = SMTPKey, Value = request.Value };
            _context.Settings.Add(entity);
        }
        else
        {
            entity.Value = request.Value;
        }
        await _context.SaveChangesAsync(cancellationToken);
        return Result<SettingDto>.Success(new SettingDto { Id = entity.Id, Key = entity.Key, Value = entity.Value });
    }
}
#endregion

