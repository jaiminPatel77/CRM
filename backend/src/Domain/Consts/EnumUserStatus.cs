using System;

namespace Crm.Domain.Consts;

/// <summary>
/// User status with respect to password generation/change.
/// </summary>
public enum EnumUserStatus
{
    /// <summary>
    /// User just created. It means yet to invite or created with appropriate password.
    /// </summary>
    Created = 0,

    /// <summary>
    /// User has been invited for first time login/password set. 
    /// </summary>
    Invited = 1,

    /// <summary>
    /// Administrator has send request to reset user password.
    /// </summary>
    AdminResetPassword = 3,

    /// <summary>
    /// User has successfully set or reset password.
    /// </summary>
    PasswordSetResetCompleted = 2,
}
