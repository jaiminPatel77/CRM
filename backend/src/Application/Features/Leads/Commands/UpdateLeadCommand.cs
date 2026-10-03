using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Leads.DTOs;
using Crm.Domain.Consts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Leads.Commands;

public record UpdateLeadCommand : IRequest<LeadDto>
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public decimal? EstimatedValue { get; init; }
    public EnumLeadStatus Status { get; init; }
    public string? Source { get; init; }
    public long? AssignedToUserId { get; init; }
    public long? CustomerId { get; init; }
    public string? Notes { get; init; }
}

public class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadCommandValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0).WithMessage("Valid Lead Id is required.");
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Lead title is required.")
            .MaximumLength(150).WithMessage("Lead title must not exceed 150 characters.");

        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(v => !string.IsNullOrEmpty(v.Email));
    }
}

public class UpdateLeadCommandHandler : IRequestHandler<UpdateLeadCommand, LeadDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateLeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeadDto> Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (lead == null)
        {
            throw new KeyNotFoundException($"Lead with Id {request.Id} not found.");
        }

        lead.Title = request.Title.Trim();
        lead.FirstName = request.FirstName?.Trim();
        lead.LastName = request.LastName?.Trim();
        lead.Email = request.Email?.Trim();
        lead.Phone = request.Phone?.Trim();
        lead.Company = request.Company?.Trim();
        lead.EstimatedValue = request.EstimatedValue;
        lead.Status = request.Status;
        lead.Source = request.Source?.Trim();
        lead.AssignedToUserId = request.AssignedToUserId;
        lead.CustomerId = request.CustomerId;
        lead.Notes = request.Notes?.Trim();
        lead.ModifiedOn = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new LeadDto
        {
            Id = lead.Id,
            TenantId = lead.TenantId,
            Title = lead.Title,
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            Email = lead.Email,
            Phone = lead.Phone,
            Company = lead.Company,
            EstimatedValue = lead.EstimatedValue,
            Status = lead.Status,
            Source = lead.Source,
            AssignedToUserId = lead.AssignedToUserId,
            CustomerId = lead.CustomerId,
            Notes = lead.Notes,
            CreatedOn = lead.CreatedOn,
            ModifiedOn = lead.ModifiedOn
        };
    }
}
