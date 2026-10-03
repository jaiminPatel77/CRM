using System;

namespace Crm.Domain.Consts;

/// <summary>
/// Type of roles
/// </summary>
public enum EnumRoleType
{
    /// <summary>
    /// Custom role. Allowed permission are as per rights selection of that role.
    /// </summary>
    CustomRole = 0,

    /// <summary>
    /// Global Administrator role. Has access to everything.
    /// </summary>
    GlobalAdministrator = 1,

    /// <summary>
    /// Tenant Administrator role. Has access to associated Tenant.
    /// </summary>
    EnterpriseAdministrator = 5,
}
