using Microsoft.AspNetCore.Identity;
using AltensorGlobalHealthCare.Domain.Common;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity, ISoftDelete
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string PreferredLanguage { get; set; } = "az";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual PatientProfile? PatientProfile { get; set; }
    public virtual Doctor? DoctorProfile { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<EmailVerificationCode> VerificationCodes { get; set; } = new List<EmailVerificationCode>();
}

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? ReplacedByToken { get; set; }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAtUtc;
}

public class EmailVerificationCode : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public string CodeHash { get; set; } = string.Empty;
    public VerificationCodePurpose Purpose { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? UsedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsValid => UsedAtUtc == null && AttemptCount < 5 && DateTime.UtcNow < ExpiresAtUtc;
}

public class PatientProfile : BaseEntity, IAuditableEntity
{
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }
    public string? Country { get; set; }
    public string? MedicalNotes { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

