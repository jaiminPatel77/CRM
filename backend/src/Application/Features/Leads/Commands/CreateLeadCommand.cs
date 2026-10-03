using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Leads.DTOs;
using Crm.Domain.Consts;
using Crm.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Crm.Application.Features.Leads.Commands;

public record CreateLeadCommand : IRequest<LeadDto>
{
    public string Title { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public decimal? EstimatedValue { get; init; }
    public EnumLeadStatus Status { get; init; } = EnumLeadStatus.New;
    public string? Source { get; init; }
    public long? AssignedToUserId { get; init; }
    public long? CustomerId { get; init; }
    public string? Notes { get; init; }
}

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Lead title is required.")
            .MaximumLength(150).WithMessage("Lead title must not exceed 150 characters.");

        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(v => !string.IsNullOrEmpty(v.Email));
    }
}

public class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, LeadDto>
{
    private readonly IApplicationDbContext _context;

    public CreateLeadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeadDto> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = new Lead
        {
            Title = request.Title.Trim(),
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            Company = request.Company?.Trim(),
            EstimatedValue = request.EstimatedValue,
            Status = request.Status,
            Source = request.Source?.Trim(),
            AssignedToUserId = request.AssignedToUserId,
            CustomerId = request.CustomerId,
            Notes = request.Notes?.Trim(),
            CreatedOn = DateTimeOffset.UtcNow,
            ModifiedOn = DateTimeOffset.UtcNow
        };

        _context.Leads.Add(lead);
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
