using System;

namespace Crm.Domain.Consts;

/// <summary>
/// Unique Id for each Entity/Table we support for an Application.
/// </summary>
public enum EnumEntityType
{
    /// <summary>
    /// Default and which is not used!
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Common/System level entity - for generic exceptions.
    /// </summary>
    COMMON = 99,

    /// <summary>
    /// User entity/table unique id.
    /// </summary>
    USER = 1,

    /// <summary>
    /// Role entity/table unique id.
    /// </summary>
    ROLE = 2,

    /// <summary>
    /// Setting entity/table unique id.
    /// </summary>
    SETTING = 3,

    /// <summary>
    /// User refresh token entity/table unique id.
    /// </summary>
    USERREFRESHTOKEN = 4,

    /// <summary>
    /// Tenant entity/table unique id.
    /// </summary>
    TENANT = 5,

    /// <summary>
    /// Customer entity/table unique id.
    /// </summary>
    CUSTOMER = 6,

    /// <summary>
    /// Lead entity/table unique id.
    /// </summary>
    LEAD = 7,

    /// <summary>
    /// Opportunity entity/table unique id.
    /// </summary>
    OPPORTUNITY = 8,

    /// <summary>
    /// Activity entity/table unique id.
    /// </summary>
    ACTIVITY = 9,
}
