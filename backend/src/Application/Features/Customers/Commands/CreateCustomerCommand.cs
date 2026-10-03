using Crm.Application.Common.Interfaces;
using Crm.Application.Features.Customers.DTOs;
using Crm.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Crm.Application.Features.Customers.Commands;

public record CreateCustomerCommand : IRequest<CustomerDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public string? Address { get; init; }
    public string? Industry { get; init; }
    public string? Notes { get; init; }
}

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(150).WithMessage("Customer name must not exceed 150 characters.");

        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(v => !string.IsNullOrEmpty(v.Email));
    }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            Company = request.Company?.Trim(),
            Address = request.Address?.Trim(),
            Industry = request.Industry?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedOn = DateTimeOffset.UtcNow,
            ModifiedOn = DateTimeOffset.UtcNow
        };

        _context.Customers.Add(customer);
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
