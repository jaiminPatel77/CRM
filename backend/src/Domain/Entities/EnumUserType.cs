using System;

namespace Crm.Domain.Entities; // Keeping namespace for now to minimize refactor

/// <summary>
/// Type of user. Mainly used to hide system generated record to be listed.
/// </summary>
public enum EnumUserType
{
    /// <summary>
    /// Custom role base user. No specific type of user!
    /// Allowed permission are as per rights selection of that role.
    /// </summary>
    CustomRoleBase = 0,

    /// <summary>
    /// User is global Administrator. Has access to everything.
    /// </summary>
    GlobalAdministrator = 1,

    /// <summary>
    /// Indicate that user is Enterprise Administrator and has all access to
    /// associated Enterprise/tenant
    /// </summary>
    EnterpriseAdministrator = 5,
}
