using System.Threading.Tasks;
using Crm.Domain.Entities;

namespace Crm.Application.Common.Interfaces;

/// <summary>
/// Email sender service interface!
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Send Invitation Email to given user with link to callback URL.
    /// </summary>
    Task SendUserInvitationEmailAsync(User toUser, User fromUser, string callbackUrl);

    /// <summary>
    /// Send reset password request email to User from Administrator.
    /// </summary>
    Task SendUserAdminResetPasswordEmailAsync(User toUser, User fromUser, string callbackUrl);

    /// <summary>
    /// Send reset password request email to user as user forgot his password.
    /// </summary>
    Task SendUserResetPasswordEmailAsync(User toUser, string callbackUrl);

    /// <summary>
    /// Send Authentication code email for 2-factor login if enabled.
    /// </summary>
    Task SendAuthenticationCodeEmailAsync(User toUser, string code);

    /// <summary>
    /// Send change email address verification email to user.
    /// </summary>
    /// <summary>
    /// Send change email address verification email to user.
    /// </summary>
    Task SendChangeEmailAddressEmailAsync(User user, string newEmail, string code);

    /// <summary>
    /// Send access request email to administrator from new user.
    /// </summary>
    Task SendAccessRequestEmailAsync(Application.Common.Models.RequestAccessDto model);
    
    /// <summary>
    /// Send ad-hock email to given email address using given subject and text message. 
    /// </summary>
    Task SendEmailAsync(string email, string subject, string textMessage);

    /// <summary>
    /// Send alert email to given user.
    /// </summary>
    Task SendAlertEmailAsync(User toUser, string subject, string message);
}
