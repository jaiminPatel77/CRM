using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Models;
using Crm.Application.Features.Users;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class AccountController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IReCaptchaService _reCaptchaService;
    private readonly IConfiguration _configuration;

    public AccountController(
        IAuthService authService, 
        IReCaptchaService reCaptchaService,
        IConfiguration configuration)
    {
        _authService = authService;
        _reCaptchaService = reCaptchaService;
        _configuration = configuration;
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest viewModel)
    {
        if (ModelState.IsValid)
        {
            var isValid = await _reCaptchaService.Validate(viewModel.SecretCode);
            if (!isValid)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_FAILED, "Captcha validation failed");
            }
            var result = await _authService.ForgotPasswordAsync(viewModel);
            if (result.Succeeded)
                 return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD, true);
            else
                 return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD_FAILED, result.Errors);
        }
        return BadRequest(ModelState);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.ResetPasswordAsync(model);
             if (result.Succeeded)
                 return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_RESET_PASSWORD, true);
             else
                 return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_CHANGE_PASSWORD_FAILED, result.Errors);
        }
        return BadRequest(ModelState);
    }

    [HttpPost("request-access")]
    public async Task<IActionResult> RequestAccess([FromBody] RequestAccessDto model)
    {
        if (ModelState.IsValid)
        {
            var isValid = await _reCaptchaService.Validate(model.SecretCode);
            if (!isValid)
            {
                 return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_FAILED, "Captcha validation failed");
            }
            var result = await _authService.RequestAccessAsync(model);
             if (result.Succeeded)
                 return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_SUCCESS, true);
             else
                 return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_FAILED, result.Errors);
        }
        return BadRequest(ModelState);
    }

    [HttpPost("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(UpdateUserProfileCommand command)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(userIdStr, out long userId))
        {
            var actualCommand = command with { Id = userId };
            var result = await Mediator.Send(actualCommand);
            if (result.Succeeded)
            {
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.COMMON_UPDATE_ITEM, true);
            }
            return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.COMMON_PUT_EXCEPTION, result.Errors);
        }
        return Unauthorized();
    }
}
