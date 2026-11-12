using AltensorGlobalHealthCare.Domain.Common;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Domain.Entities;

public class Review : BaseEntity, IAuditableEntity, ISoftDelete
{
    public Guid? ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }

    public Guid? DoctorId { get; set; }
    public virtual Doctor? Doctor { get; set; }

    public Guid? PatientId { get; set; }
    public virtual ApplicationUser? Patient { get; set; }

    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorCountry { get; set; }

    public int Rating { get; set; } // 1 to 5
    public string Comment { get; set; } = string.Empty;

    public ReviewSource Source { get; set; } = ReviewSource.Internal;
    public bool IsVerifiedPatient { get; set; }
    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

public class CmsPage : BaseEntity, IAuditableEntity
{
    public string Slug { get; set; } = string.Empty; // about-us, how-it-works, terms, privacy

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? HeadingAz { get; set; }
    public string? HeadingEn { get; set; }
    public string? HeadingRu { get; set; }

    public string ContentAz { get; set; } = string.Empty;
    public string? ContentEn { get; set; }
    public string? ContentRu { get; set; }

    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

public class BlogPost : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string Slug { get; set; } = string.Empty;

    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }

    public string? SummaryAz { get; set; }
    public string? SummaryEn { get; set; }
    public string? SummaryRu { get; set; }

    public string ContentAz { get; set; } = string.Empty;
    public string? ContentEn { get; set; }
    public string? ContentRu { get; set; }

    public string? CoverUrl { get; set; }
    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

public class FaqItem : BaseEntity
{
    public string QuestionAz { get; set; } = string.Empty;
    public string? QuestionEn { get; set; }
    public string? QuestionRu { get; set; }

    public string AnswerAz { get; set; } = string.Empty;
    public string? AnswerEn { get; set; }
    public string? AnswerRu { get; set; }

    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Partner : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public PartnerType PartnerType { get; set; } = PartnerType.Institutional;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ContactMessage : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;

    public bool IsHandled { get; set; }
    public DateTime? HandledAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

public class NewsletterSubscriber : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public DateTime SubscribedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

