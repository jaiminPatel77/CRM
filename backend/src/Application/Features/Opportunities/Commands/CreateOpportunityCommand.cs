using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Opportunities.DTOs;
using Crm.Domain.Consts;
using Crm.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Crm.Application.Features.Opportunities.Commands;

public record CreateOpportunityCommand : IRequest<OpportunityDto>
{
    public string Title { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public EnumOpportunityStage Stage { get; init; } = EnumOpportunityStage.Qualification;
    public int Probability { get; init; } = 10;
    public DateTimeOffset? ExpectedCloseDate { get; init; }
    public long? CustomerId { get; init; }
    public long? LeadId { get; init; }
    public long? AssignedToUserId { get; init; }
    public string? Notes { get; init; }
}

public class CreateOpportunityCommandValidator : AbstractValidator<CreateOpportunityCommand>
{
    public CreateOpportunityCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Opportunity title is required.")
            .MaximumLength(150).WithMessage("Opportunity title must not exceed 150 characters.");

        RuleFor(v => v.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be non-negative.");

        RuleFor(v => v.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100.");
    }
}

public class CreateOpportunityCommandHandler : IRequestHandler<CreateOpportunityCommand, OpportunityDto>
{
    private readonly IApplicationDbContext _context;

    public CreateOpportunityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OpportunityDto> Handle(CreateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = new Opportunity
        {
            Title = request.Title.Trim(),
            Amount = request.Amount,
            Stage = request.Stage,
            Probability = request.Probability,
            ExpectedCloseDate = request.ExpectedCloseDate,
            CustomerId = request.CustomerId,
            LeadId = request.LeadId,
            AssignedToUserId = request.AssignedToUserId,
            Notes = request.Notes?.Trim(),
            CreatedOn = DateTimeOffset.UtcNow,
            ModifiedOn = DateTimeOffset.UtcNow
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync(cancellationToken);

        return new OpportunityDto
        {
            Id = opportunity.Id,
            TenantId = opportunity.TenantId,
            Title = opportunity.Title,
            Amount = opportunity.Amount,
            Stage = opportunity.Stage,
            Probability = opportunity.Probability,
            ExpectedCloseDate = opportunity.ExpectedCloseDate,
            CustomerId = opportunity.CustomerId,
            LeadId = opportunity.LeadId,
            AssignedToUserId = opportunity.AssignedToUserId,
            Notes = opportunity.Notes,
            CreatedOn = opportunity.CreatedOn,
            ModifiedOn = opportunity.ModifiedOn
        };
    }
}
