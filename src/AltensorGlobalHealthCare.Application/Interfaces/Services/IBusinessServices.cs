using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.DTOs.Admin;
using AltensorGlobalHealthCare.Application.DTOs.Catalog;
using AltensorGlobalHealthCare.Application.DTOs.Content;
using AltensorGlobalHealthCare.Application.DTOs.Inquiries;
using AltensorGlobalHealthCare.Application.DTOs.Offers;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.Interfaces.Services;

public interface IPublicPortalService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(string? lang, bool headerOnly = false, bool homeOnly = false, CancellationToken cancellationToken = default);
    Task<CategoryDetailDto?> GetCategoryBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default);
    Task<CategoryDetailDto?> GetSubCategoryBySlugAsync(string categorySlug, string subSlug, string? lang, CancellationToken cancellationToken = default);
    Task<MedicalServiceDetailDto?> GetServiceBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalServiceSummaryDto>> GetMostSearchedServicesAsync(string? lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TreatmentPackageDto>> GetPackagesAsync(string? lang, CancellationToken cancellationToken = default);
    Task<TreatmentPackageDto?> GetPackageBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClinicSummaryDto>> GetClinicsAsync(string? lang, CancellationToken cancellationToken = default);
    Task<ClinicDetailDto?> GetClinicBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DoctorDto>> GetDoctorsAsync(string? lang, CancellationToken cancellationToken = default);
    Task<DoctorDto?> GetDoctorByIdAsync(Guid id, string? lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewDto>> GetReviewsAsync(Guid? clinicId, Guid? doctorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalServiceSummaryDto>> SearchAsync(string query, string? lang, CancellationToken cancellationToken = default);
    Task<CmsPageDto?> GetCmsPageAsync(string slug, string? lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CmsPageMenuItemDto>> GetCmsPagesMenuAsync(string? parentSlug, string? lang, CancellationToken cancellationToken = default);
    Task<AboutUsDto> GetAboutUsAsync(string? lang, CancellationToken cancellationToken = default);
    Task<HowItWorksDto> GetHowItWorksAsync(string? lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BlogPostSummaryDto>> GetBlogPostsAsync(string? lang, CancellationToken cancellationToken = default);
    Task<BlogPostDetailDto?> GetBlogPostBySlugAsync(string slug, string? lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FaqItemDto>> GetFaqItemsAsync(string? lang, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PartnerDto>> GetPartnersAsync(CancellationToken cancellationToken = default);
    Task<string> SubmitContactMessageAsync(ContactMessageRequestDto request, CancellationToken cancellationToken = default);
    Task<string> SubscribeNewsletterAsync(NewsletterRequestDto request, CancellationToken cancellationToken = default);
    Task<string> SubmitReviewAsync(CreateReviewRequestDto request, CancellationToken cancellationToken = default);
}

public interface IPatientService
{
    Task<InquirySummaryDto> CreateInquiryAsync(Guid userId, CreateInquiryRequestDto request, IList<FileUploadDto>? documents, CancellationToken cancellationToken = default);
    Task<PagedResult<InquirySummaryDto>> GetInquiriesAsync(Guid userId, InquiryStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<InquiryDetailDto> GetInquiryByIdAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default);
    Task<InquiryDocumentDto> UploadDocumentAsync(Guid userId, Guid inquiryId, FileUploadDto file, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(Guid userId, Guid inquiryId, Guid documentId, CancellationToken cancellationToken = default);
    Task<string> UpdateInquiryProcedureAsync(Guid userId, Guid inquiryId, Guid medicalServiceId, CancellationToken cancellationToken = default);
    Task<string> CancelInquiryAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default);
    Task<string> RestoreInquiryAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OfferDto>> GetOffersForInquiryAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default);
    Task<string> AcceptOfferAsync(Guid userId, Guid offerId, CancellationToken cancellationToken = default);
    Task<string> DeclineOfferAsync(Guid userId, Guid offerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InquiryMessageDto>> GetInquiryMessagesAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default);
    Task<InquiryMessageDto> SendMessageAsync(Guid userId, Guid inquiryId, SendMessageRequestDto request, CancellationToken cancellationToken = default);
}

public interface ICoordinatorService
{
    Task<PagedResult<InquirySummaryDto>> GetInquiriesAsync(InquiryStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<InquiryDetailDto> GetInquiryByIdAsync(Guid inquiryId, CancellationToken cancellationToken = default);
    Task<string> AssignDoctorAsync(Guid inquiryId, Guid doctorId, Guid coordinatorId, CancellationToken cancellationToken = default);
    Task<string> UpdateStatusAsync(Guid inquiryId, InquiryStatus status, CancellationToken cancellationToken = default);
    Task<OfferDto> CreateOfferAsync(CreateOfferRequestDto request, IList<FileUploadDto>? attachments, CancellationToken cancellationToken = default);
    Task<string> SendOfferAsync(Guid offerId, CancellationToken cancellationToken = default);
    Task<string> WithdrawOfferAsync(Guid offerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InquiryMessageDto>> GetMessagesAsync(Guid inquiryId, CancellationToken cancellationToken = default);
    Task<InquiryMessageDto> SendMessageAsync(Guid coordinatorId, Guid inquiryId, SendMessageRequestDto request, CancellationToken cancellationToken = default);
    Task<DashboardSummaryDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}

public interface IDoctorService
{
    Task<PagedResult<InquirySummaryDto>> GetAssignedInquiriesAsync(Guid doctorUserId, InquiryStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<InquiryDetailDto> GetInquiryByIdAsync(Guid doctorUserId, Guid inquiryId, CancellationToken cancellationToken = default);
    Task<OfferDto> CreateDraftOfferAsync(Guid doctorUserId, CreateOfferRequestDto request, IList<FileUploadDto>? attachments, CancellationToken cancellationToken = default);
    Task<OfferDto> UpdateOfferAsync(Guid doctorUserId, Guid offerId, UpdateOfferRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InquiryMessageDto>> GetMessagesAsync(Guid doctorUserId, Guid inquiryId, CancellationToken cancellationToken = default);
    Task<InquiryMessageDto> SendMessageAsync(Guid doctorUserId, Guid inquiryId, SendMessageRequestDto request, CancellationToken cancellationToken = default);
}

public interface IAdminService
{
    Task<DashboardSummaryDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);

    // Clinics
    Task<IReadOnlyList<ClinicSummaryDto>> GetAllClinicsAsync(CancellationToken cancellationToken = default);
    Task<ClinicDetailDto> CreateClinicAsync(CreateClinicRequestDto request, CancellationToken cancellationToken = default);
    Task<ClinicDetailDto> UpdateClinicAsync(Guid id, UpdateClinicRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteClinicAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> UpdateClinicLogoAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default);
    Task<string> UpdateClinicCoverAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default);

    // Doctors
    Task<DoctorDto> CreateDoctorAsync(CreateDoctorRequestDto request, CancellationToken cancellationToken = default);
    Task<DoctorDto> UpdateDoctorAsync(Guid id, UpdateDoctorRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteDoctorAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> UpdateDoctorPhotoAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default);

    // Categories & Services
    Task<IReadOnlyList<AdminCategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminMedicalServiceSummaryDto>> GetAllServicesAsync(Guid? categoryId = null, string? search = null, CancellationToken cancellationToken = default);
    Task<AdminMedicalServiceDetailDto> GetServiceByIdForAdminAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MedicalServiceDetailDto> CreateServiceAsync(CreateMedicalServiceRequestDto request, CancellationToken cancellationToken = default);
    Task<MedicalServiceDetailDto> UpdateServiceAsync(Guid id, UpdateMedicalServiceRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteServiceAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> UpdateServiceHeroImageAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default);
    Task<string> UpdateServicePromoBannerAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default);
    Task SetClinicPriceAsync(SetClinicPriceRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteClinicPriceAsync(Guid serviceId, Guid clinicId, CancellationToken cancellationToken = default);
    Task SetServiceDoctorsAsync(Guid serviceId, SetServiceDoctorsRequestDto request, CancellationToken cancellationToken = default);

    // Packages
    Task<TreatmentPackageDto> CreatePackageAsync(CreateTreatmentPackageRequestDto request, CancellationToken cancellationToken = default);
    Task<TreatmentPackageDto> UpdatePackageAsync(Guid id, UpdateTreatmentPackageRequestDto request, CancellationToken cancellationToken = default);
    Task DeletePackageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> UpdatePackageCoverAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default);

    // Content & About Us
    Task<IReadOnlyList<AdminCmsPageDto>> GetAllCmsPagesAsync(CancellationToken cancellationToken = default);
    Task<AdminCmsPageDto> GetCmsPageBySlugForAdminAsync(string slug, CancellationToken cancellationToken = default);
    Task<AdminCmsPageDto> UpsertCmsPageAsync(UpsertCmsPageRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteCmsPageAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminAboutPillarDto>> GetAllAboutPillarsAsync(CancellationToken cancellationToken = default);
    Task<AdminAboutPillarDto> CreateAboutPillarAsync(UpsertAboutPillarRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminAboutPillarDto> UpdateAboutPillarAsync(Guid id, UpsertAboutPillarRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAboutPillarAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminTeamMemberDto>> GetAllTeamMembersAsync(CancellationToken cancellationToken = default);
    Task<AdminTeamMemberDto> CreateTeamMemberAsync(UpsertTeamMemberRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminTeamMemberDto> UpdateTeamMemberAsync(Guid id, UpsertTeamMemberRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteTeamMemberAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> UpdateTeamMemberPhotoAsync(Guid id, FileUploadDto file, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminHowItWorksStepDto>> GetAllHowItWorksStepsAsync(CancellationToken cancellationToken = default);
    Task<AdminHowItWorksStepDto> CreateHowItWorksStepAsync(UpsertHowItWorksStepRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminHowItWorksStepDto> UpdateHowItWorksStepAsync(Guid id, UpsertHowItWorksStepRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteHowItWorksStepAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContactMessageDto>> GetContactMessagesAsync(bool? isHandled = null, CancellationToken cancellationToken = default);
    Task<ContactMessageDto> HandleContactMessageAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BlogPostDetailDto> CreateBlogPostAsync(BlogPostDetailDto request, CancellationToken cancellationToken = default);
    Task DeleteBlogPostAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminFaqItemDto>> GetAllFaqsAsync(Guid? medicalServiceId = null, CancellationToken cancellationToken = default);
    Task<AdminFaqItemDto> CreateFaqAsync(UpsertFaqRequestDto request, CancellationToken cancellationToken = default);
    Task<AdminFaqItemDto> UpdateFaqAsync(Guid id, UpsertFaqRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteFaqAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PartnerDto> CreatePartnerAsync(PartnerDto request, CancellationToken cancellationToken = default);
    Task DeletePartnerAsync(Guid id, CancellationToken cancellationToken = default);
}

