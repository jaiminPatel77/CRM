using Crm.Domain.Attributes;
using Crm.Domain.Common;
using Crm.Domain.Consts; // Added
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Crm.Domain.Entities;

[Auditable]
public class User : IdentityUser<long>, IAuditableEntity
{
    // IdentityUser has Id, Email, UserName, etc.

    public Guid? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required]
    [StringLength(128)]
    [ProtectedPersonalData]
    public string FullName { get; set; } = string.Empty;

    public EnumUserType UserType { get; set; } = EnumUserType.CustomRoleBase;

    [StringLength(128)]
    [ProtectedPersonalData]
    public string? Title { get; set; }

    public bool IsImageAvailable { get; set; }
    public bool Disabled { get; set; }
    public DateTimeOffset? EnabledDisabledOn { get; set; }
    public DateTimeOffset? PreviousPasswordDate { get; set; }

    [StringLength(1024)]
    public string? PreviousPassword1 { get; set; }
    [StringLength(1024)]
    public string? PreviousPassword2 { get; set; }
    [StringLength(1024)]
    public string? PreviousPassword3 { get; set; }

    // Auditing properties (replicating AuditableEntity structure since we can't multiple inherit)
    public DateTimeOffset CreatedOn { get; set; }
    public long? CreatedById { get; set; }
    public DateTimeOffset ModifiedOn { get; set; }
    public long? ModifiedById { get; set; }

    public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; } = new List<UserRefreshToken>();
    
    // Legacy Parity
    public EnumUserStatus Status { get; set; } = EnumUserStatus.Created;
    
    public virtual UserProfile? UserProfile { get; set; }
    
    // Explicit Navigation required for strict parity if lazy loading roles
    public virtual ICollection<IdentityUserRole<long>> UserRoles { get; set; } = new List<IdentityUserRole<long>>();
}
