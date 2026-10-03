using System.ComponentModel.DataAnnotations;
using Crm.Domain.Entities;

namespace Crm.Application.Common.Models;

public class ForgotPasswordRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SecretCode { get; set; } = string.Empty;

    [Required]
    public string ReturnUrl { get; set; } = string.Empty;
}


public class ResetPasswordDto
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}

public class RequestAccessDto
{
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Phone Number")]
    [RegularExpression(@"(^\+?[0-9\(][0-9\s\(\)-]{8,15}(?:x.+)?$)", ErrorMessage = "Not a valid Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Reason for Access")]
    public string ReasonForAccess { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Secret Code")]
    public string SecretCode { get; set; } = string.Empty;

    [Required]
    public string CallbackUrl { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}
