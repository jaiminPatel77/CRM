using Crm.Application.Common.Models;
using Crm.Domain.Entities;

namespace Crm.Application.Common.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(string email, string password);
    Task<Result<AuthResponse>> RegisterAsync(string email, string password, string fullName);
    Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken);
    Task LogoutAsync(string token, string refreshToken);
    
    // Account Recovery & Access
    Task<Result<bool>> RequestAccessAsync(RequestAccessDto model);
    Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest model);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto model);
    Task<Result<bool>> ChangePasswordAsync(ChangePasswordDto model);
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresIn { get; set; }
    public EnumUserType UserType { get; set; }
    public string FullName { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
}
