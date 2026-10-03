using Crm.Application.Common.Interfaces;
using Crm.Application.Common.Models;
using Crm.Domain.Entities;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Crm.Infrastructure.Services;

public class SmtpEmailService : IEmailSender, IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendUserInvitationEmailAsync(User toUser, User fromUser, string callbackUrl)
    {
        var replacements = new Dictionary<string, string>
        {
            { "userFullName", toUser.FullName ?? toUser.Email! },
            { "fromUserFullName", fromUser.FullName ?? fromUser.Email! },
            { "appTitle", "CRM" },
            { "callbackUrl", callbackUrl },
            { "fromUserEmail", fromUser.Email! }
        };
        await SendEmailWithTemplateAsync(toUser.Email!, "Invitation to join", "user-invitation", replacements);
    }

    public async Task SendUserAdminResetPasswordEmailAsync(User toUser, User fromUser, string callbackUrl)
    {
        // Re-using reset password template for now, or could use a specific one if it existed.
        // Template 'user-resetpassword.html' implies self-request, but we can adapt or just use it.
        var replacements = new Dictionary<string, string>
        {
            { "userFullName", toUser.FullName ?? toUser.Email! },
            { "appTitle", "CRM" },
            { "callbackUrl", callbackUrl },
            { "fromUserEmail", fromUser.Email! } 
        };
        await SendEmailWithTemplateAsync(toUser.Email!, "Password Reset Request", "user-resetpassword", replacements);
    }

    public async Task SendUserResetPasswordEmailAsync(User toUser, string callbackUrl)
    {
        var replacements = new Dictionary<string, string>
        {
            { "userFullName", toUser.FullName ?? toUser.Email! },
            { "appTitle", "CRM" },
            { "callbackUrl", callbackUrl },
            { "fromUserEmail", _configuration["EmailSettings:From"] ?? "noreply@example.com" }
        };
        await SendEmailWithTemplateAsync(toUser.Email!, "Reset Password", "user-resetpassword", replacements);
    }

    public async Task SendAuthenticationCodeEmailAsync(User toUser, string code)
    {
        var replacements = new Dictionary<string, string>
        {
            { "userFullName", toUser.FullName ?? toUser.Email! },
            { "appTitle", "CRM" },
            { "code", code }
        };
        await SendEmailWithTemplateAsync(toUser.Email!, "Authentication Code", "user-authentication-code", replacements);
    }

    public async Task SendChangeEmailAddressEmailAsync(User user, string newEmail, string code)
    {
        var replacements = new Dictionary<string, string>
        {
            { "userFullName", user.FullName ?? user.Email! },
            { "appTitle", "CRM" },
            { "code", code }
        };
        await SendEmailWithTemplateAsync(newEmail, "Change Email Confirmation", "user-change-email", replacements);
    }

    public async Task SendAccessRequestEmailAsync(RequestAccessDto model)
    {
        var adminEmail = _configuration["EmailSettings:From"]; // Or separate AdminEmail setting
        if (string.IsNullOrEmpty(adminEmail)) return;

        var replacements = new Dictionary<string, string>
        {
            { "userEmail", model.Email },
            { "appTitle", "CRM" },
            { "message", "User requested access." }
        };
        
        // Ensure template exists, otherwise fallback to simple text
        await SendEmailWithTemplateAsync(adminEmail, "New Access Request", "user-access-request", replacements);
    }

    public async Task SendAlertEmailAsync(User toUser, string subject, string message)
    {
        var replacements = new Dictionary<string, string>
        {
            { "userFullName", toUser.FullName ?? toUser.Email! },
            { "appTitle", "CRM" },
            { "message", message },
            { "subject", subject }
        };
        await SendEmailWithTemplateAsync(toUser.Email!, subject, "user-alert-email", replacements);
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Direct Send (No Template)
        return SendEmailInternalAsync(to, subject, body);
    }

    // Helper to load template and embed images
    private async Task SendEmailWithTemplateAsync(string to, string subject, string templateName, Dictionary<string, string> replacements)
    {
        try 
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "EmailTemplate", $"{templateName}.html");
            if (!File.Exists(templatePath))
            {
                 // Fallback if template missing
                 _logger.LogWarning("Email Template {Name} not found at {Path}", templateName, templatePath);
                 await SendEmailInternalAsync(to, subject, $"Subject: {subject}. (Template missing).");
                 return;
            }

            var htmlContent = await File.ReadAllTextAsync(templatePath);
            foreach (var kvp in replacements)
            {
                htmlContent = htmlContent.Replace($"${{{kvp.Key}}}", kvp.Value); // Replace ${key}
            }
            
            await SendEmailInternalAsync(to, subject, htmlContent, true);
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Error preparing email template {Name}", templateName);
             throw; // Or handle gracefully
        }
    }

    private async Task SendEmailInternalAsync(string email, string subject, string content, bool isHtmlAndHasImages = false)
    {
        try
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var host = emailSettings["Host"];
            var port = int.Parse(emailSettings["Port"] ?? "587");
            var username = emailSettings["Username"];
            var password = emailSettings["Password"];
            var from = emailSettings["From"] ?? "noreply@example.com";

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("CRM", from));
            mimeMessage.To.Add(new MailboxAddress("", email));
            mimeMessage.Subject = subject;

            var builder = new BodyBuilder();
            if (isHtmlAndHasImages)
            {
                 var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "EmailTemplate", "AppLogo.png");
                 if (File.Exists(logoPath))
                 {
                     var image = builder.LinkedResources.Add(logoPath);
                     image.ContentId = "AppLogo"; // Matches cid:AppLogo in HTML
                 }
                 builder.HtmlBody = content;
            }
            else
            {
                 builder.HtmlBody = content; // Assuming content is HTML for SendEmailAsync(to,sub,body) usage too, or separate TextBody logic
            }

            mimeMessage.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            throw; // Re-throw so caller knows email failed (critical for password reset, invitations, etc.)
        }
    }
}
