namespace AltensorGlobalHealthCare.Application.DTOs.Catalog;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconKey { get; set; }
    public int DisplayOrder { get; set; }
    public IList<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
    public IList<MedicalServiceSummaryDto> Services { get; set; } = new List<MedicalServiceSummaryDto>();
}

public class MedicalServiceSummaryDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? DurationText { get; set; }
    public bool IsMostSearched { get; set; }
    public decimal? StartingPrice { get; set; }
    public string Currency { get; set; } = "EUR";
}

public class MedicalServiceDetailDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? About { get; set; }
    public string? Preparation { get; set; }
    public string? Steps { get; set; }
    public string? Precautions { get; set; }
    public string? Result { get; set; }
    public string? DurationText { get; set; }
    public decimal? StartingPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public IList<ClinicPriceDto> AvailableClinics { get; set; } = new List<ClinicPriceDto>();
}

public class ClinicPriceDto
{
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string ClinicSlug { get; set; } = string.Empty;
    public string? ClinicLogoUrl { get; set; }
    public string? City { get; set; }
    public decimal PriceFrom { get; set; }
    public string Currency { get; set; } = "EUR";
}

public class ClinicSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int DoctorsCount { get; set; }
    public int ServicesCount { get; set; }
}

public class ClinicDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverUrl { get; set; }
    public string? Certificates { get; set; }
    public IList<DoctorDto> Doctors { get; set; } = new List<DoctorDto>();
    public IList<MedicalServiceSummaryDto> OfferedServices { get; set; } = new List<MedicalServiceSummaryDto>();
    public IList<ClinicReviewDto> Reviews { get; set; } = new List<ClinicReviewDto>();
}

public class DoctorDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public int ExperienceYears { get; set; }
    public string? PhotoUrl { get; set; }
}

public class TreatmentPackageDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Includes { get; set; }
    public decimal PriceFrom { get; set; }
    public string Currency { get; set; } = "EUR";
    public string? CoverUrl { get; set; }
    public Guid? MedicalServiceId { get; set; }
    public string? ServiceName { get; set; }
}

public class ClinicReviewDto
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorCountry { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPatient { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

