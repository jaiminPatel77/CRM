using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Models;
using Crm.Application.Features.Auth.Commands.RequestAccess;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Crm.Tests.Features.Auth.Commands.RequestAccess;

public class RequestAccessCommandTests
{
    private readonly IReCaptchaService _reCaptchaService;
    private readonly IEmailService _emailService;
    private readonly RequestAccessCommandHandler _handler;

    public RequestAccessCommandTests()
    {
        _reCaptchaService = Substitute.For<IReCaptchaService>();
        _emailService = Substitute.For<IEmailService>();
        _handler = new RequestAccessCommandHandler(_reCaptchaService, _emailService);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenReCaptchaValidationFails()
    {
        // Arrange
        var command = new RequestAccessCommand
        {
            Dto = new RequestAccessDto { Email = "test@example.com", SecretCode = "invalid" }
        };
        _reCaptchaService.Validate(Arg.Any<string>()).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain("ReCaptcha validation failed.");
        await _emailService.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenReCaptchaValidationSucceeds()
    {
        // Arrange
        var email = "user@example.com";
        var command = new RequestAccessCommand
        {
            Dto = new RequestAccessDto { Email = email, SecretCode = "valid" }
        };
        _reCaptchaService.Validate(Arg.Any<string>()).Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeTrue();
        await _emailService.Received(1).SendEmailAsync(
            "admin@example.com", 
            "New Access Request", 
            Arg.Is<string>(m => m.Contains(email)));
    }
}
