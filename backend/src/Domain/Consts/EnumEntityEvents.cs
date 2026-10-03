using System;

namespace Crm.Domain.Consts;

/// <summary>
/// Common and Entity specific event/function result.
/// Name of the Enum will be used as Id to handle translated messages.
/// </summary>
public enum EnumEntityEvents
{
    #region Common successful message ids or event id.

    /// <summary>
    /// Indicate that get list of the records API is successful.
    /// </summary>
    COMMON_LIST_ALL_ITEMS = 1000,

    /// <summary>
    /// Indicate the get filter list of the records API is successful.
    /// </summary>
    COMMON_LIST_FILTER_ITEMS = 1001,

    /// <summary>
    /// Indicate the get individual record API is successful.
    /// </summary>
    COMMON_GET_ITEM = 1002,

    /// <summary>
    /// Indicate that post API to create new record is successful.
    /// </summary>
    COMMON_CREATE_ITEM = 1003,

    /// <summary>
    /// Indicate that POST API to update the existing record is successful.
    /// </summary>
    COMMON_UPDATE_ITEM = 1004,

    /// <summary>
    /// Indicate that DELETE API to remove record is successful.
    /// </summary>
    COMMON_DELETE_ITEM = 1005,

    #endregion

    #region Common Error message id/event id start from 4000..
    
    /// <summary>
    /// Indicate that GET API resulted in requested record is not found. 
    /// </summary>
    COMMON_GET_ITEM_NOTFOUND = 4000,

    /// <summary>
    /// Indicate that Update API resulted in record not found.
    /// </summary>
    COMMON_UPDATE_ITEM_NOTFOUND = 4001,

    /// <summary>
    /// Indicate that DELETE API resulted in record not found.
    /// </summary>
    COMMON_DELETE_ITEM_NOTFOUND = 4002,

    /// <summary>
    /// Get list of record API failed because of an exception. Refer exception for detail.
    /// </summary>
    COMMON_LIST_EXCEPTION = 4003,

    /// <summary>
    /// Get record API failed because of an exception. Refer exception for detail.
    /// </summary>
    COMMON_GET_EXCEPTION = 4004,

    /// <summary>
    /// Create record API failed because of an exception. Refer exception for detail.
    /// </summary>
    COMMON_CREATE_EXCEPTION = 4005,

    /// <summary>
    /// update record API failed because of an exception. Refer exception for detail.
    /// </summary>
    COMMON_PUT_EXCEPTION = 4006,

    /// <summary>
    /// DELETE record API failed because of an exception. Refer exception for detail.
    /// </summary>
    COMMON_DELETE_EXCEPTION = 4007,

    /// <summary>
    /// POST API failed because of an exception. Refer exception for detail.
    /// </summary>
    COMMON_POST_EXCEPTION = 4008,

    /// <summary>
    /// Validation error occurred. Check errors for details.
    /// </summary>
    COMMON_VALIDATION_ERROR = 4009,

    /// <summary>
    /// Access denied. User does not have permission.
    /// </summary>
    COMMON_ACCESS_DENIED = 4010,

    #endregion

    #region Entity specific event id/ message id in form xxxyyyy.

    // Entity specific Operations are in "xxxyyyy" - format, where 
    // xxx = Entity Type Refer EnumEntityType enum for the same.
    // yyyy = entity specific event id or api return value message id.
    // Any error related event id will be start from 501 for any entity.

    #region User entity specific API result

    /// <summary>
    /// User Login API successful.
    /// </summary>
    USER_LOGIN = 0010001,

    /// <summary>
    /// Integrate that Register new User API is successful.
    /// </summary>
    USER_REGISTER = 0010002,

    /// <summary>
    /// Integrate that user renew token API is successful.
    /// </summary>
    USER_RENEW_TOKEN = 0010003,

    /// <summary>
    /// Integrate that user forgot password API is successful.
    /// </summary>
    USER_FORGOT_PASSWORD = 0010004,

    /// <summary>
    /// Integrate that reset forgot password APU is successful.
    /// </summary>
    USER_RESET_PASSWORD = 0010005,

    /// <summary>
    /// User Invite API is successful.
    /// </summary>
    USER_INVITE_USER = 0010006,

    /// <summary>
    /// User email verification email send successful to user.
    /// </summary>
    USER_EMAIL_VERIFICATION_SENT = 0010007,

    /// <summary>
    /// User email verification successful.
    /// </summary>
    USER_EMAIL_VERIFICATION_SUCCESS = 0010008,

    /// <summary>
    /// User access request email sent to administrator successful.
    /// </summary>
    USER_ACCESSREQUEST_SUCCESS = 0010009,

    /// <summary>
    /// User logout successfully.
    /// </summary>
    USER_LOGOUT_SUCCESS = 0010010,

    /// <summary>
    /// User login API is failed.
    /// </summary>
    USER_LOGIN_FAILED = 0010501,

    /// <summary>
    /// User renew token API failed.
    /// </summary>
    USER_RENEW_TOKEN_FAILED = 0010502,

    /// <summary>
    /// User register API failed.
    /// </summary>
    USER_REGISTER_FAILED = 0010503,

    /// <summary>
    /// User forgot password API failed.
    /// </summary>
    USER_FORGOT_PASSWORD_FAILED = 0010504,

    /// <summary>
    /// User reset password API failed.
    /// </summary>
    USER_RESET_PASSWORD_FAILED = 0010515,

    /// <summary>
    /// User change password API failed.
    /// </summary>
    USER_CHANGE_PASSWORD_FAILED = 0010505,

    /// <summary>
    /// User change password resulted in match with last 4 password error.
    /// </summary>
    USER_LAST4_PASSWORD_FAILED = 0010506,

    /// <summary>
    /// User invalid password.
    /// </summary>
    USER_INVALID_PASSWORD = 0010507,

    /// <summary>
    /// User invite API is failed.
    /// </summary>
    USER_INVITE_USER_FAILED = 0010508,

    /// <summary>
    /// User email verification failed.
    /// </summary>
    USER_EMAIL_VERIFICATION_FAILED = 0010509,

    /// <summary>
    /// User access request API failed.
    /// </summary>
    USER_ACCESSREQUEST_FAILED = 0010510,

    /// <summary>
    /// User login failed because invitation is yet not verified.
    /// </summary>
    USER_LOGIN_FAILED_INVITATION = 0010511,

    /// <summary>
    /// User login failed due to reset password is pending.
    /// </summary>
    USER_LOGIN_FAILED_RESET = 0010512,

    /// <summary>
    /// User logout API failed.
    /// </summary>
    USER_LOGOUT_FAILED = 0010513,

    /// <summary>
    /// User email duplicate
    /// </summary>
    USER_EMAIL_DUPLICATE = 0010514,
    #endregion

    #region Setting entity specific API result.

    // 003 = entity type

    /// <summary>
    /// SMTP setting not found.
    /// </summary>
    SETTING_NOSMTP_SETTING = 0030501,

    /// <summary>
    /// send email operation failed.
    /// </summary>
    SETTING_SENDEMAIL_FAILD = 0030502,

    #endregion

    #endregion
}
