namespace Crm.Application.Common.Interfaces;

public interface IReCaptchaService
{
    Task<bool> Validate(string secretCode);
}
