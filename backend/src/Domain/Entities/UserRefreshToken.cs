using Crm.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Domain.Entities;

public class UserRefreshToken : BaseEntity
{
    [Required]
    [StringLength(128)]
    public string RefreshToken { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset? ValidTill { get; set; }

    public bool IsExpired => ValidTill.HasValue && DateTimeOffset.UtcNow >= ValidTill;

    [Required]
    [StringLength(128)]
    public string CreatedByIp { get; set; } = string.Empty;
    
    [StringLength(128)]
    public string DeviceId { get; set; } = string.Empty; // Added

    public DateTimeOffset CreatedOn { get; set; }

    [StringLength(128)]
    public string? RevokedByIp { get; set; }

    public DateTimeOffset? Revoked { get; set; }

    public string? ReplacedByToken { get; set; }

    public bool IsActive => Revoked == null && !IsExpired;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
    public long UserId { get; set; }
}
