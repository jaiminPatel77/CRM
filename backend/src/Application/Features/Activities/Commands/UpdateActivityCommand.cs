using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Activities.DTOs;
using Crm.Domain.Consts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Activities.Commands;

public record UpdateActivityCommand : IRequest<ActivityDto>
{
    public long Id { get; init; }
    public string Subject { get; init; } = string.Empty;
    public EnumActivityType Type { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public bool IsCompleted { get; init; }
    public string? Description { get; init; }
    public long? CustomerId { get; init; }
    public long? LeadId { get; init; }
    public long? OpportunityId { get; init; }
    public long? AssignedToUserId { get; init; }
}

public class UpdateActivityCommandValidator : AbstractValidator<UpdateActivityCommand>
{
    public UpdateActivityCommandValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0).WithMessage("Valid Activity Id is required.");
        RuleFor(v => v.Subject)
            .NotEmpty().WithMessage("Activity subject is required.")
            .MaximumLength(200).WithMessage("Activity subject must not exceed 200 characters.");
    }
}

public class UpdateActivityCommandHandler : IRequestHandler<UpdateActivityCommand, ActivityDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateActivityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityDto> Handle(UpdateActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (activity == null)
        {
            throw new KeyNotFoundException($"Activity with Id {request.Id} not found.");
        }

        activity.Subject = request.Subject.Trim();
        activity.Type = request.Type;
        activity.DueDate = request.DueDate;
        activity.IsCompleted = request.IsCompleted;
        activity.Description = request.Description?.Trim();
        activity.CustomerId = request.CustomerId;
        activity.LeadId = request.LeadId;
        activity.OpportunityId = request.OpportunityId;
        activity.AssignedToUserId = request.AssignedToUserId;
        activity.ModifiedOn = DateTimeOffset.UtcNow;

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
