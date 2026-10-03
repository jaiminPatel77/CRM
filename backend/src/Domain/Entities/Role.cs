using Crm.Domain.Common;
using Crm.Domain.Consts;
using Microsoft.AspNetCore.Identity;

namespace Crm.Domain.Entities;

public class Role : IdentityRole<long>, IAuditableEntity
{
    public Role() : base() { }
    public Role(string roleName) : base(roleName) { }
    
    // Add any custom Role properties here (e.g. Description)
    public string? Description { get; set; }

    public Guid? TenantId { get; set; }

    public EnumRoleType RoleType { get; set; } = EnumRoleType.CustomRole;

    // Auditing
    public DateTimeOffset CreatedOn { get; set; }
    public long? CreatedById { get; set; }
    public DateTimeOffset ModifiedOn { get; set; }
    public long? ModifiedById { get; set; }
}
