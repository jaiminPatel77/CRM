using Microsoft.IdentityModel.Tokens;

namespace Crm.Application.Common.Models;

public class JwtIssuerOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public DateTime NotBefore => DateTime.UtcNow;
    public DateTime IssuedAt => DateTime.UtcNow;
    public TimeSpan ValidFor { get; set; } = TimeSpan.FromMinutes(30);
    public string SecretKey { get; set; } = string.Empty; // Added for binding from appsettings

    public Func<Task<string>> JtiGenerator =>
      () => Task.FromResult(Guid.NewGuid().ToString());

    public SigningCredentials? SigningCredentials { get; set; }

    public DateTime Expiration => IssuedAt.Add(ValidFor);
}
