using System;

namespace Crm.Domain.Consts;

/// <summary>
/// Indicate how setting are stored in database as string or JSON object
/// </summary>
public enum EnumSettingStorageFormate
{
    /// <summary>
    /// String, default value.
    /// </summary>
    String = 0,

    /// <summary>
    /// JSON object.
    /// </summary>
    JSON = 1,
}
