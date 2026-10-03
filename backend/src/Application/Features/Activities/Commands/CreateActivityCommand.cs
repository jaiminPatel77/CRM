using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Activities.DTOs;
using Crm.Domain.Consts;
using Crm.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Crm.Application.Features.Activities.Commands;

public record CreateActivityCommand : IRequest<ActivityDto>
{
    public string Subject { get; init; } = string.Empty;
    public EnumActivityType Type { get; init; } = EnumActivityType.Task;
    public DateTimeOffset? DueDate { get; init; }
    public bool IsCompleted { get; init; } = false;
    public string? Description { get; init; }
    public long? CustomerId { get; init; }
    public long? LeadId { get; init; }
    public long? OpportunityId { get; init; }
    public long? AssignedToUserId { get; init; }
}

public class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
        RuleFor(v => v.Subject)
            .NotEmpty().WithMessage("Activity subject is required.")
            .MaximumLength(200).WithMessage("Activity subject must not exceed 200 characters.");
    }
}

public class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, ActivityDto>
{
    private readonly IApplicationDbContext _context;

    public CreateActivityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = new Activity
        {
            Subject = request.Subject.Trim(),
            Type = request.Type,
            DueDate = request.DueDate,
            IsCompleted = request.IsCompleted,
            Description = request.Description?.Trim(),
            CustomerId = request.CustomerId,
            LeadId = request.LeadId,
            OpportunityId = request.OpportunityId,
            AssignedToUserId = request.AssignedToUserId,
            CreatedOn = DateTimeOffset.UtcNow,
            ModifiedOn = DateTimeOffset.UtcNow
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync(cancellationToken);

        return new ActivityDto
        {
            Id = activity.Id,
            TenantId = activity.TenantId,
            Subject = activity.Subject,
            Type = activity.Type,
            DueDate = activity.DueDate,
            IsCompleted = activity.IsCompleted,
            Description = activity.Description,
            CustomerId = activity.CustomerId,
            LeadId = activity.LeadId,
            OpportunityId = activity.OpportunityId,
            AssignedToUserId = activity.AssignedToUserId,
            CreatedOn = activity.CreatedOn,
            ModifiedOn = activity.ModifiedOn
        };
    }
}
