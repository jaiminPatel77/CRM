using System;
using System.Net.Mail;

namespace Crm.Domain.Models.Settings;

/// <summary>
/// SMTP Setting for any Email to be send from application.
/// </summary>
public class SMTPSetting
{
    public SMTPSetting()
    {
        IsSSL = true;
        ServerAddress = string.Empty;
        UserName = string.Empty;
        Password = string.Empty;
        FromEmail = string.Empty;
        PickupDirectoryLocation = string.Empty;
    }

    /// <summary>
    /// SMTP UserName
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Associated password.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// SMTP server address.
    /// </summary>
    public string ServerAddress { get; set; }

    /// <summary>
    /// SMTP port to connect to email server.
    /// </summary>
    public int ServerPort { get; set; }

    /// <summary>
    /// Indicate whether to use SSL or not while sending email.
    /// </summary>
    public bool IsSSL { get; set; }

    /// <summary>
    /// Form e-mail address which is to be used, while sending any e-mail alert.
    /// </summary>
    public string FromEmail { get; set; }

    /// <summary>
    ///Added few more SMTP related settings.
    /// </summary>
    public bool UseDefaultCredentials { get; set; }
    public SmtpDeliveryFormat DeliveryFormat { get; set; }
    public SmtpDeliveryMethod DeliveryMethod { get; set; }

    public string PickupDirectoryLocation { get; set; }
}
