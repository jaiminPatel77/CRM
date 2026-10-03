using Crm.Application.Common.Models;
using Crm.Application.Features.Auth.Commands.RequestAccess;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ApiControllerBase
{
    private readonly Application.Common.Interfaces.IAuthService _authService;

    public AuthController(Application.Common.Interfaces.IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Application.Features.Auth.Models.LoginModel model)
    {
        var result = await _authService.LoginAsync(model.Email, model.Password);
        if (result.Succeeded)
        {
             return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN, result.Data);
        }
        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED, result.Errors);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Application.Features.Auth.Models.RegisterModel model)
    {
        var result = await _authService.RegisterAsync(model.Email, model.Password, model.FullName);
        if (result.Succeeded)
        {
             return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_REGISTER, result.Data);
        }
        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_REGISTER_FAILED, result.Errors);
    }

    [HttpPost("renew-token")]
    public async Task<IActionResult> RenewToken([FromBody] Application.Features.Auth.Models.JwtInfo model)
    {
         var result = await _authService.RefreshTokenAsync(model.access_token, model.refresh_token);
         if (result.Succeeded)
         {
             return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_RENEW_TOKEN, result.Data);
         }
         return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_RENEW_TOKEN_FAILED, result.Errors);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] Application.Features.Auth.Models.JwtInfo model)
    {
         await _authService.LogoutAsync(model.access_token, model.refresh_token);
         return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_LOGOUT_SUCCESS, true);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
    {
        var result = await _authService.ForgotPasswordAsync(model);
        if (result.Succeeded)
        {
            return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD, result.Data);
        }
        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD_FAILED, result.Errors);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var result = await _authService.ResetPasswordAsync(model);
        if (result.Succeeded)
        {
            return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_RESET_PASSWORD, result.Data);
        }
        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_RESET_PASSWORD_FAILED, result.Errors);
    }

    [HttpPost("request-access")]
    public async Task<IActionResult> RequestAccess([FromBody] RequestAccessDto model)
    {
        var result = await _authService.RequestAccessAsync(model);
        if (result.Succeeded)
        {
            return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_SUCCESS, result.Data);
        }
        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_FAILED, result.Errors);
    }
}

