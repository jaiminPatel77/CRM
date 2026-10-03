using System;

namespace Crm.Domain.Consts;

/// <summary>
/// Status of Application Settings
/// </summary>
public enum EnumSettingStatus
{
    /// <summary>
    /// Active, default value.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Archived.
    /// </summary>
    Archived = 1,

    /// <summary>
    /// Disabled.
    /// </summary>
    Disabled = 2
}
