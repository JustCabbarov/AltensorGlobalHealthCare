using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.DTOs.Content;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid? ClinicId { get; set; }
    public string? ClinicName { get; set; }
    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorCountry { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public ReviewSource Source { get; set; }
    public bool IsVerifiedPatient { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateReviewRequestDto
{
    public Guid? ClinicId { get; set; }
    public Guid? DoctorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorCountry { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class CmsPageDto
{
    public string Slug { get; set; } = string.Empty;
    public string? ParentSlug { get; set; }
    public string PageType { get; set; } = "Standard";
    public string Title { get; set; } = string.Empty;
    public string? Heading { get; set; }
    public string? Subtitle { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? MissionTitle { get; set; }
    public string? MissionText { get; set; }
    public string? HeroImageUrl { get; set; }
    public bool ShowInMenu { get; set; }
    public int DisplayOrder { get; set; }
}

public class CmsPageMenuItemDto
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PageType { get; set; } = "Standard";
    public int DisplayOrder { get; set; }
}

public class AboutPillarDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconKey { get; set; }
    public int DisplayOrder { get; set; }
}

public class TeamMemberDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Quote { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WhatsAppUrl { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsPrimaryCoordinator { get; set; }
    public int DisplayOrder { get; set; }
}

public class HowItWorksStepDto
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CtaLabel { get; set; }
    public string? CtaUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class AboutUsDto
{
    public CmsPageDto? Page { get; set; }
    public IList<CmsPageMenuItemDto> SubPages { get; set; } = new List<CmsPageMenuItemDto>();
    public IList<AboutPillarDto> Pillars { get; set; } = new List<AboutPillarDto>();
    public IList<TeamMemberDto> TeamMembers { get; set; } = new List<TeamMemberDto>();
    public IList<HowItWorksStepDto> HowItWorksSteps { get; set; } = new List<HowItWorksStepDto>();
    public TeamMemberDto? PrimaryCoordinator { get; set; }
}

public class HowItWorksDto
{
    public CmsPageDto? Page { get; set; }
    public IList<HowItWorksStepDto> Steps { get; set; } = new List<HowItWorksStepDto>();
}

public class BlogPostSummaryDto
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime PublishedAtUtc { get; set; }
}

public class BlogPostDetailDto : BlogPostSummaryDto
{
    public string Content { get; set; } = string.Empty;
}

public class FaqItemDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class PartnerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public PartnerType PartnerType { get; set; }
}

public class ContactMessageRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class NewsletterRequestDto
{
    public string Email { get; set; } = string.Empty;
}

