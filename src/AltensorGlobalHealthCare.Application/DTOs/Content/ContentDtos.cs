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
    public string Title { get; set; } = string.Empty;
    public string? Heading { get; set; }
    public string Content { get; set; } = string.Empty;
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

