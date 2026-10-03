using Crm.Domain.Common;

namespace Crm.Domain.Entities;

public class UserProfile : BaseEntity
{
    // In legacy this was ModelBase, which has Id, CreatedOn etc.
    // BaseEntity mirrors ModelBase.

    /// <summary>
    /// Image information as base64. Or event one can set whatever we need as application needs.
    /// e.g. URI in case of large image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Navigation property to User Table.
    /// </summary>
    public virtual User User { get; set; } = null!;
}
