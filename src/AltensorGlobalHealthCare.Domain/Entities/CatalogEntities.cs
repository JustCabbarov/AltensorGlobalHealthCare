using AltensorGlobalHealthCare.Domain.Common;

namespace AltensorGlobalHealthCare.Domain.Entities;

public class Clinic : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public string? DescriptionAz { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionRu { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; } = "Zagreb";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? Certificates { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    public virtual ICollection<ClinicService> ClinicServices { get; set; } = new List<ClinicService>();
    public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public class Doctor : BaseEntity, IAuditableEntity, ISoftDelete
{
    public Guid ClinicId { get; set; }
    public virtual Clinic Clinic { get; set; } = null!;

    public Guid? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? TitleAz { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? SpecializationAz { get; set; }
    public string? SpecializationEn { get; set; }
    public string? SpecializationRu { get; set; }

    public string? BioAz { get; set; }
    public string? BioEn { get; set; }
    public string? BioRu { get; set; }

    public int ExperienceYears { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public class ServiceCategory : BaseEntity, IAuditableEntity, ISoftDelete
{
    public Guid? ParentId { get; set; }
    public virtual ServiceCategory? Parent { get; set; }
    public virtual ICollection<ServiceCategory> SubCategories { get; set; } = new List<ServiceCategory>();

    public string Slug { get; set; } = string.Empty;
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? DescriptionAz { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionRu { get; set; }

    public string? IconKey { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<MedicalService> Services { get; set; } = new List<MedicalService>();
}

public class MedicalService : BaseEntity, IAuditableEntity, ISoftDelete
{
    public Guid CategoryId { get; set; }
    public virtual ServiceCategory Category { get; set; } = null!;

    public string Slug { get; set; } = string.Empty;
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? AboutAz { get; set; }
    public string? AboutEn { get; set; }
    public string? AboutRu { get; set; }

    public string? PreparationAz { get; set; }
    public string? PreparationEn { get; set; }
    public string? PreparationRu { get; set; }

    public string? StepsAz { get; set; }
    public string? StepsEn { get; set; }
    public string? StepsRu { get; set; }

    public string? PrecautionsAz { get; set; }
    public string? PrecautionsEn { get; set; }
    public string? PrecautionsRu { get; set; }

    public string? ResultAz { get; set; }
    public string? ResultEn { get; set; }
    public string? ResultRu { get; set; }

    public string? DurationText { get; set; }
    public bool IsMostSearched { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<ClinicService> ClinicServices { get; set; } = new List<ClinicService>();
}

public class ClinicService : BaseEntity, IAuditableEntity
{
    public Guid ClinicId { get; set; }
    public virtual Clinic Clinic { get; set; } = null!;

    public Guid MedicalServiceId { get; set; }
    public virtual MedicalService MedicalService { get; set; } = null!;

    public decimal PriceFrom { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

public class TreatmentPackage : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string Slug { get; set; } = string.Empty;
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? DescriptionAz { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionRu { get; set; }

    public string? IncludesAz { get; set; }
    public string? IncludesEn { get; set; }
    public string? IncludesRu { get; set; }

    public decimal PriceFrom { get; set; }
    public string Currency { get; set; } = "EUR";

    public Guid? MedicalServiceId { get; set; }
    public virtual MedicalService? MedicalService { get; set; }

    public string? CoverUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

