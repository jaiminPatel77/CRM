using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Models;
using Crm.Application.Common.Exceptions;
using Crm.Domain.Entities;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Crm.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtIssuerOptions _jwtOptions;
    private readonly IUserRefreshTokenService _refreshTokenService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptions<JwtIssuerOptions> jwtOptions,
        IUserRefreshTokenService refreshTokenService,
        IEmailSender emailSender,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtOptions = jwtOptions.Value;
        _refreshTokenService = refreshTokenService;
        _emailSender = emailSender;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<AuthResponse>> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return Result<AuthResponse>.Failure(new[] { "Invalid login attempt." });

        if (user.Disabled) return Result<AuthResponse>.Failure(new[] { "User account is disabled." });
        if (user.Status == EnumUserStatus.Invited) return Result<AuthResponse>.Failure(new[] { "User account is disabled." }); // Legacy maps specific error but here using generic message or I can throw exception to be caught and mapped to specific EnumEntityEvent if I want strict parity.
        // Legacy Controller returns: CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED_INVITATION, _logger);
        // My Service returns Result<AuthResponse>. Controller maps success/fail. 
        // To support specific errors, I might need to return specific error codes or message strings that Controller interprets.
        // For now, I will return specific strings.
        if (user.Status == EnumUserStatus.Invited) return Result<AuthResponse>.Failure(new[] { "Userinvited" }); 
        if (user.Status == EnumUserStatus.AdminResetPassword) return Result<AuthResponse>.Failure(new[] { "UserResetPassword" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded) return Result<AuthResponse>.Failure(new[] { "Invalid login attempt." });

        var authResponse = await GenerateJwtToken(user);
        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result<AuthResponse>> RegisterAsync(string email, string password, string fullName)
    {
        var user = new User { UserName = email, Email = email, FullName = fullName };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return Result<AuthResponse>.Failure(result.Errors.Select(e => e.Description));
        }

        var authResponse = await GenerateJwtToken(user);
        return Result<AuthResponse>.Success(authResponse);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var storedToken = await _refreshTokenService.GetRefreshToken(refreshToken);
        if (storedToken == null) return Result<AuthResponse>.Failure(new[] { "Invalid refresh token." });

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return Result<AuthResponse>.Failure(new[] { "Invalid access token." });

        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Result<AuthResponse>.Failure(new[] { "Invalid token claims." });

        var userId = long.Parse(userIdClaim.Value);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null || user.Id != storedToken.UserId) 
            return Result<AuthResponse>.Failure(new[] { "Invalid token ownership." });

        // Generate new tokens
        var authResponse = await GenerateAuthResponse(user, false); // Don't save yet, we'll rotate
        
        // Rotate refresh token (deletes old, creates new)
        await _refreshTokenService.RefreshToken(storedToken, authResponse.RefreshToken, DateTime.UtcNow.Add(TimeSpan.FromHours(4))); // Standardize to 4 hours

        return Result<AuthResponse>.Success(authResponse);
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user, bool saveRefreshToken = true)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserName ?? user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("id", user.Id.ToString()),
            new("fullName", user.FullName),
            new("userType", user.UserType.ToString())
        };

        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim("tenant_id", user.TenantId.Value.ToString()));
            claims.Add(new Claim("TenantId", user.TenantId.Value.ToString()));
        }

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(_jwtOptions.ValidFor),
            signingCredentials: creds
        );

        var refreshToken = Guid.NewGuid().ToString("N");
        
        if (saveRefreshToken)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "N/A";
            
            await _refreshTokenService.CreateRefreshToken(new UserRefreshToken
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                ValidTill = DateTimeOffset.UtcNow.AddHours(4),
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedByIp = ipAddress
            });
        }

        return new AuthResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            ExpiresIn = token.ValidTo,
            UserType = user.UserType,
            FullName = user.FullName,
            TenantId = user.TenantId
        };
    }

    private Task<AuthResponse> GenerateJwtToken(User user) => GenerateAuthResponse(user, true);

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            return null;

        return principal;
    }

    public async Task<Result<bool>> RequestAccessAsync(RequestAccessDto model)
    {
        try 
        {
            await _emailSender.SendAccessRequestEmailAsync(model);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting access");
            return Result<bool>.Failure(new[] { "Failed to send access request." });
        }
    }

    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedCode = System.Net.WebUtility.UrlEncode(code);
            var callbackUrl = $"{model.ReturnUrl}?code={encodedCode}";
            await _emailSender.SendUserResetPasswordEmailAsync(user, callbackUrl);
        }
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Result<bool>.Failure(new[] { "Invalid request." });

        try { ValidatePassword(user, model.Password); }
        catch (Exception ex) { return Result<bool>.Failure(new[] { ex.Message }); }

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (!result.Succeeded) return Result<bool>.Failure(result.Errors.Select(e => e.Description));

        if (user.Status != EnumUserStatus.PasswordSetResetCompleted)
        {
            user.Status = EnumUserStatus.PasswordSetResetCompleted;
            await _userManager.UpdateAsync(user);
        }
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordDto model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null) return Result<bool>.Failure(new[] { "User not found." });

        var check = await _userManager.CheckPasswordAsync(user, model.OldPassword);
        if (!check) return Result<bool>.Failure(new[] { "Invalid old password." });

        try { ValidatePassword(user, model.NewPassword); }
        catch (Exception ex) { return Result<bool>.Failure(new[] { ex.Message }); }

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (result.Succeeded)
        {
             // History update
             user.PreviousPassword3 = user.PreviousPassword2;
             user.PreviousPassword2 = user.PreviousPassword1;
             user.PreviousPasswordDate = DateTimeOffset.UtcNow;
             await _userManager.UpdateAsync(user);
             return Result<bool>.Success(true);
        }
        return Result<bool>.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task LogoutAsync(string token, string refreshToken)
    {
        await _refreshTokenService.RemoveRefreshToken(refreshToken);
       // await _signInManager.SignOutAsync();
    }

    private void ValidatePassword(User user, string newPassword)
    {
        var hasher = new PasswordHasher<User>();
        if (!string.IsNullOrEmpty(user.PreviousPassword1) && hasher.VerifyHashedPassword(user, user.PreviousPassword1, newPassword) != PasswordVerificationResult.Failed)
             throw new CrmApplicationException("PASSWORD_REUSE", "Password cannot be same as previous one.");
        if (!string.IsNullOrEmpty(user.PreviousPassword2) && hasher.VerifyHashedPassword(user, user.PreviousPassword2, newPassword) != PasswordVerificationResult.Failed)
             throw new CrmApplicationException("PASSWORD_REUSE", "Password cannot be same as previous one.");
    }
}
