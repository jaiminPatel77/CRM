using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace Crm.Application.Features.Auth.Commands.RequestAccess;

public record RequestAccessCommand : IRequest<Result<bool>>
{
    public RequestAccessDto Dto { get; init; } = new();
}

public class RequestAccessCommandValidator : AbstractValidator<RequestAccessCommand>
{
    public RequestAccessCommandValidator()
    {
        RuleFor(v => v.Dto.Email).NotEmpty().EmailAddress();
        RuleFor(v => v.Dto.SecretCode).NotEmpty().WithMessage("ReCaptcha is required.");
    }
}

public class RequestAccessCommandHandler : IRequestHandler<RequestAccessCommand, Result<bool>>
{
    private readonly IReCaptchaService _reCaptchaService;
    private readonly IEmailService _emailService;

    public RequestAccessCommandHandler(IReCaptchaService reCaptchaService, IEmailService emailService)
    {
        _reCaptchaService = reCaptchaService;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(RequestAccessCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate ReCaptcha
        var isValid = await _reCaptchaService.Validate(request.Dto.SecretCode);
        if (!isValid)
        {
            return Result<bool>.Failure(new[] {"ReCaptcha validation failed."});
        }

        // 2. Logic to handle "Request Access" (e.g., Send email to Admin, or Create inactive User)
        // For this template, we'll simulate sending an email to Admin
        await _emailService.SendEmailAsync("admin@example.com", "New Access Request", 
            $"User {request.Dto.Email} has requested access.");

        return Result<bool>.Success(true);
    }
}
