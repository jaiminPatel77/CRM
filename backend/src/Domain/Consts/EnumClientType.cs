using System;

namespace Crm.Domain.Consts;

/// <summary>
/// Type of the client applications.
/// </summary>
public enum EnumClientType
{
    /// <summary>
    /// Web Application or Web browser. Default value.
    /// </summary>
    Web = 0,

    /// <summary>
    /// Android mobile application.
    /// </summary>
    AndroidApp = 1,

    /// <summary>
    /// IOS mobile application.
    /// </summary>
    AppleApp = 2,
}
