using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Opportunities.DTOs;
using Crm.Domain.Consts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Opportunities.Commands;

public record UpdateOpportunityCommand : IRequest<OpportunityDto>
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public EnumOpportunityStage Stage { get; init; }
    public int Probability { get; init; }
    public DateTimeOffset? ExpectedCloseDate { get; init; }
    public long? CustomerId { get; init; }
    public long? LeadId { get; init; }
    public long? AssignedToUserId { get; init; }
    public string? Notes { get; init; }
}

public class UpdateOpportunityCommandValidator : AbstractValidator<UpdateOpportunityCommand>
{
    public UpdateOpportunityCommandValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0).WithMessage("Valid Opportunity Id is required.");
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Opportunity title is required.")
            .MaximumLength(150).WithMessage("Opportunity title must not exceed 150 characters.");

        RuleFor(v => v.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be non-negative.");

        RuleFor(v => v.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100.");
    }
}

public class UpdateOpportunityCommandHandler : IRequestHandler<UpdateOpportunityCommand, OpportunityDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateOpportunityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OpportunityDto> Handle(UpdateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (opportunity == null)
        {
            throw new KeyNotFoundException($"Opportunity with Id {request.Id} not found.");
        }

        opportunity.Title = request.Title.Trim();
        opportunity.Amount = request.Amount;
        opportunity.Stage = request.Stage;
        opportunity.Probability = request.Probability;
        opportunity.ExpectedCloseDate = request.ExpectedCloseDate;
        opportunity.CustomerId = request.CustomerId;
        opportunity.LeadId = request.LeadId;
        opportunity.AssignedToUserId = request.AssignedToUserId;
        opportunity.Notes = request.Notes?.Trim();
        opportunity.ModifiedOn = DateTimeOffset.UtcNow;

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
