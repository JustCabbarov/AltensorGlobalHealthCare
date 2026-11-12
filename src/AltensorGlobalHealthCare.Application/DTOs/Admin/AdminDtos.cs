using AltensorGlobalHealthCare.Application.DTOs.Inquiries;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.DTOs.Admin;

public class DashboardSummaryDto
{
    public int TotalInquiries { get; set; }
    public int ActiveInquiries { get; set; }
    public int TotalPatients { get; set; }
    public int TotalClinics { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalOffers { get; set; }
    public int AcceptedOffers { get; set; }
    public decimal TotalAcceptedVolume { get; set; }
    public IList<InquirySummaryDto> RecentInquiries { get; set; } = new List<InquirySummaryDto>();
}

public class CreateClinicRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? DescriptionAz { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionRu { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Certificates { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateClinicRequestDto : CreateClinicRequestDto
{
    public bool IsActive { get; set; }
}

public class CreateDoctorRequestDto
{
    public Guid ClinicId { get; set; }
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
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateDoctorRequestDto : CreateDoctorRequestDto
{
    public bool IsActive { get; set; }
}

public class CreateCategoryRequestDto
{
    public Guid? ParentId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }
    public string? DescriptionAz { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionRu { get; set; }
    public string? IconKey { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateCategoryRequestDto : CreateCategoryRequestDto
{
    public bool IsActive { get; set; }
}

public class CreateMedicalServiceRequestDto
{
    public Guid CategoryId { get; set; }
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
}

public class UpdateMedicalServiceRequestDto : CreateMedicalServiceRequestDto
{
    public bool IsActive { get; set; }
}

public class SetClinicPriceRequestDto
{
    public Guid ClinicId { get; set; }
    public Guid MedicalServiceId { get; set; }
    public decimal PriceFrom { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; } = true;
}

public class CreateTreatmentPackageRequestDto
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
    public int DisplayOrder { get; set; }
}

public class UpdateTreatmentPackageRequestDto : CreateTreatmentPackageRequestDto
{
    public bool IsActive { get; set; }
}

