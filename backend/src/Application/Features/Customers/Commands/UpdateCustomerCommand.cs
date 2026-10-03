using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Customers.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crm.Application.Features.Customers.Commands;

public record UpdateCustomerCommand : IRequest<CustomerDto>
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public string? Address { get; init; }
    public string? Industry { get; init; }
    public string? Notes { get; init; }
}

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0).WithMessage("Valid Customer Id is required.");
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(150).WithMessage("Customer name must not exceed 150 characters.");

        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(v => !string.IsNullOrEmpty(v.Email));
    }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with Id {request.Id} not found.");
        }

        customer.Name = request.Name.Trim();
        customer.Email = request.Email?.Trim();
        customer.Phone = request.Phone?.Trim();
        customer.Company = request.Company?.Trim();
        customer.Address = request.Address?.Trim();
        customer.Industry = request.Industry?.Trim();
        customer.Notes = request.Notes?.Trim();
        customer.ModifiedOn = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerDto
        {
            Id = customer.Id,
            TenantId = customer.TenantId,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Company = customer.Company,
            Address = customer.Address,
            Industry = customer.Industry,
            Notes = customer.Notes,
            CreatedOn = customer.CreatedOn,
            ModifiedOn = customer.ModifiedOn
        };
    }
}
