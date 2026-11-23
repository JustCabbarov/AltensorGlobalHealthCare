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
    public bool ShowInHeaderNav { get; set; } = true;
    public bool ShowOnHomePage { get; set; }
    public string? MetaTitleAz { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaTitleRu { get; set; }
    public string? MetaDescriptionAz { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionRu { get; set; }
}

public class UpdateCategoryRequestDto : CreateCategoryRequestDto
{
    public bool IsActive { get; set; }
}

public class AdminCategoryDto
{
    public Guid Id { get; set; }
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
    public bool IsActive { get; set; }
    public bool ShowInHeaderNav { get; set; }
    public bool ShowOnHomePage { get; set; }
    public string? MetaTitleAz { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaTitleRu { get; set; }
    public string? MetaDescriptionAz { get; set; }
    public string? MetaDescriptionEn { get; set; }
    public string? MetaDescriptionRu { get; set; }
    public int ServicesCount { get; set; }
    public IList<AdminCategoryDto> SubCategories { get; set; } = new List<AdminCategoryDto>();
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
    public string? CandidateAz { get; set; }
    public string? CandidateEn { get; set; }
    public string? CandidateRu { get; set; }
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
    public string? PackageInfoAz { get; set; }
    public string? PackageInfoEn { get; set; }
    public string? PackageInfoRu { get; set; }
    public Guid? RelatedPackageId { get; set; }
    public string? DurationText { get; set; }
    public bool IsMostSearched { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateMedicalServiceRequestDto : CreateMedicalServiceRequestDto
{
    public bool IsActive { get; set; }
}

public class AdminMedicalServiceSummaryDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryNameAz { get; set; } = string.Empty;
    public string? CategoryNameEn { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryNameAz { get; set; }
    public string? ParentCategoryNameEn { get; set; }
    public int ClinicsCount { get; set; }
    public int DoctorsCount { get; set; }
    public decimal? StartingPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; }
    public bool IsMostSearched { get; set; }
    public int DisplayOrder { get; set; }
    public string? HeroImageUrl { get; set; }
}

public class AdminClinicPriceDto
{
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public decimal PriceFrom { get; set; }
    public string Currency { get; set; } = "EUR";
    public bool IsActive { get; set; }
}

public class AdminFaqItemDto
{
    public Guid Id { get; set; }
    public string QuestionAz { get; set; } = string.Empty;
    public string? QuestionEn { get; set; }
    public string? QuestionRu { get; set; }
    public string AnswerAz { get; set; } = string.Empty;
    public string? AnswerEn { get; set; }
    public string? AnswerRu { get; set; }
    public string? Category { get; set; }
    public Guid? MedicalServiceId { get; set; }
    public Guid? ServiceCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertFaqRequestDto
{
    public string? QuestionAz { get; set; }
    public string? QuestionEn { get; set; }
    public string? QuestionRu { get; set; }
    public string? AnswerAz { get; set; }
    public string? AnswerEn { get; set; }
    public string? AnswerRu { get; set; }
    // Optional legacy aliases so single-language clients keep working
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public string? Category { get; set; }
    public Guid? MedicalServiceId { get; set; }
    public Guid? ServiceCategoryId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SetServiceDoctorsRequestDto
{
    public IList<Guid> DoctorIds { get; set; } = new List<Guid>();
}

public class AdminMedicalServiceDetailDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }
    public string? AboutAz { get; set; }
    public string? AboutEn { get; set; }
    public string? AboutRu { get; set; }
    public string? CandidateAz { get; set; }
    public string? CandidateEn { get; set; }
    public string? CandidateRu { get; set; }
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
    public string? PackageInfoAz { get; set; }
    public string? PackageInfoEn { get; set; }
    public string? PackageInfoRu { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? PromoBannerUrl { get; set; }
    public Guid? RelatedPackageId { get; set; }
    public string? DurationText { get; set; }
    public bool IsMostSearched { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public IList<AdminClinicPriceDto> ClinicPrices { get; set; } = new List<AdminClinicPriceDto>();
    public IList<Guid> DoctorIds { get; set; } = new List<Guid>();
    public IList<AdminFaqItemDto> Faqs { get; set; } = new List<AdminFaqItemDto>();
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

public class AdminCmsPageDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? ParentSlug { get; set; }
    public string PageType { get; set; } = "Standard";
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }
    public string? HeadingAz { get; set; }
    public string? HeadingEn { get; set; }
    public string? HeadingRu { get; set; }
    public string? SubtitleAz { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleRu { get; set; }
    public string ContentAz { get; set; } = string.Empty;
    public string? ContentEn { get; set; }
    public string? ContentRu { get; set; }
    public string? MissionTitleAz { get; set; }
    public string? MissionTitleEn { get; set; }
    public string? MissionTitleRu { get; set; }
    public string? MissionTextAz { get; set; }
    public string? MissionTextEn { get; set; }
    public string? MissionTextRu { get; set; }
    public string? HeroImageUrl { get; set; }
    public bool ShowInMenu { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class UpsertCmsPageRequestDto
{
    public string Slug { get; set; } = string.Empty;
    public string? ParentSlug { get; set; }
    public string PageType { get; set; } = "Standard";
    public string? TitleAz { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }
    public string? HeadingAz { get; set; }
    public string? HeadingEn { get; set; }
    public string? HeadingRu { get; set; }
    public string? SubtitleAz { get; set; }
    public string? SubtitleEn { get; set; }
    public string? SubtitleRu { get; set; }
    public string? ContentAz { get; set; }
    public string? ContentEn { get; set; }
    public string? ContentRu { get; set; }
    public string? MissionTitleAz { get; set; }
    public string? MissionTitleEn { get; set; }
    public string? MissionTitleRu { get; set; }
    public string? MissionTextAz { get; set; }
    public string? MissionTextEn { get; set; }
    public string? MissionTextRu { get; set; }
    public string? HeroImageUrl { get; set; }
    public bool ShowInMenu { get; set; } = true;
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;

    // Optional legacy aliases
    public string? Title { get; set; }
    public string? Heading { get; set; }
    public string? Content { get; set; }
}

public class AdminAboutPillarDto
{
    public Guid Id { get; set; }
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }
    public string? DescriptionAz { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionRu { get; set; }
    public string? IconKey { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertAboutPillarRequestDto
{
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }
    public string? DescriptionAz { get; set; }
    public string? DescriptionEn { get; set; }
    public string? DescriptionRu { get; set; }
    public string? IconKey { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AdminTeamMemberDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RoleAz { get; set; } = string.Empty;
    public string? RoleEn { get; set; }
    public string? RoleRu { get; set; }
    public string? BioAz { get; set; }
    public string? BioEn { get; set; }
    public string? BioRu { get; set; }
    public string? QuoteAz { get; set; }
    public string? QuoteEn { get; set; }
    public string? QuoteRu { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WhatsAppUrl { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsPrimaryCoordinator { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertTeamMemberRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string RoleAz { get; set; } = string.Empty;
    public string? RoleEn { get; set; }
    public string? RoleRu { get; set; }
    public string? BioAz { get; set; }
    public string? BioEn { get; set; }
    public string? BioRu { get; set; }
    public string? QuoteAz { get; set; }
    public string? QuoteEn { get; set; }
    public string? QuoteRu { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WhatsAppUrl { get; set; }
    public bool IsPrimaryCoordinator { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AdminHowItWorksStepDto
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }
    public string ContentAz { get; set; } = string.Empty;
    public string? ContentEn { get; set; }
    public string? ContentRu { get; set; }
    public string? CtaLabelAz { get; set; }
    public string? CtaLabelEn { get; set; }
    public string? CtaLabelRu { get; set; }
    public string? CtaUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertHowItWorksStepRequestDto
{
    public int StepNumber { get; set; }
    public string TitleAz { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? TitleRu { get; set; }
    public string ContentAz { get; set; } = string.Empty;
    public string? ContentEn { get; set; }
    public string? ContentRu { get; set; }
    public string? CtaLabelAz { get; set; }
    public string? CtaLabelEn { get; set; }
    public string? CtaLabelRu { get; set; }
    public string? CtaUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ContactMessageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsHandled { get; set; }
    public DateTime? HandledAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

