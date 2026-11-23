using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.DTOs.Admin;
using AltensorGlobalHealthCare.Application.DTOs.Catalog;
using AltensorGlobalHealthCare.Application.DTOs.Content;
using AltensorGlobalHealthCare.Application.Interfaces.Services;

namespace AltensorGlobalHealthCare.WebApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly INotificationDispatcher _notifications;

    public AdminController(IAdminService adminService, INotificationDispatcher notifications)
    {
        _adminService = adminService;
        _notifications = notifications;
    }

    /// <summary>
    /// Ümumi sistem və biznes statistikası
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetDashboardStatsAsync(cancellationToken);
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(result));
    }

    // --- Clinics ---
    [HttpGet("clinics")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ClinicSummaryDto>>>> GetClinics(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllClinicsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ClinicSummaryDto>>.Ok(result));
    }

    [HttpPost("clinics")]
    public async Task<ActionResult<ApiResponse<ClinicDetailDto>>> CreateClinic([FromBody] CreateClinicRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateClinicAsync(request, cancellationToken);
        return Ok(ApiResponse<ClinicDetailDto>.Ok(result, "Klinika yaradıldı."));
    }

    [HttpPut("clinics/{id:guid}")]
    public async Task<ActionResult<ApiResponse<ClinicDetailDto>>> UpdateClinic(Guid id, [FromBody] UpdateClinicRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateClinicAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ClinicDetailDto>.Ok(result, "Klinika yeniləndi."));
    }

    [HttpDelete("clinics/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteClinic(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteClinicAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Klinika silindi."));
    }

    [HttpPost("clinics/{id:guid}/logo")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateClinicLogo(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var fileDto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        var url = await _adminService.UpdateClinicLogoAsync(id, fileDto, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Loqo yeniləndi."));
    }

    [HttpPost("clinics/{id:guid}/cover")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateClinicCover(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var fileDto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        var url = await _adminService.UpdateClinicCoverAsync(id, fileDto, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Qapaq şəkli yeniləndi."));
    }

    // --- Doctors ---
    [HttpPost("doctors")]
    public async Task<ActionResult<ApiResponse<DoctorDto>>> CreateDoctor([FromBody] CreateDoctorRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateDoctorAsync(request, cancellationToken);
        return Ok(ApiResponse<DoctorDto>.Ok(result, "Həkim əlavə edildi."));
    }

    [HttpPut("doctors/{id:guid}")]
    public async Task<ActionResult<ApiResponse<DoctorDto>>> UpdateDoctor(Guid id, [FromBody] UpdateDoctorRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateDoctorAsync(id, request, cancellationToken);
        return Ok(ApiResponse<DoctorDto>.Ok(result, "Həkim məlumatları yeniləndi."));
    }

    [HttpDelete("doctors/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteDoctor(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteDoctorAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Həkim silindi."));
    }

    [HttpPost("doctors/{id:guid}/photo")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateDoctorPhoto(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var fileDto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        var url = await _adminService.UpdateDoctorPhotoAsync(id, fileDto, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Həkim şəkli yeniləndi."));
    }

    // --- Categories & Services ---
    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminCategoryDto>>>> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllCategoriesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminCategoryDto>>.Ok(result));
    }

    [HttpPost("categories")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateCategoryAsync(request, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result, "Kateqoriya yaradıldı."));
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateCategoryAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result, "Kateqoriya yeniləndi."));
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteCategoryAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Kateqoriya silindi."));
    }

    [HttpGet("services")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminMedicalServiceSummaryDto>>>> GetServices(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllServicesAsync(categoryId, search, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminMedicalServiceSummaryDto>>.Ok(result));
    }

    [HttpGet("services/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminMedicalServiceDetailDto>>> GetServiceById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminService.GetServiceByIdForAdminAsync(id, cancellationToken);
        return Ok(ApiResponse<AdminMedicalServiceDetailDto>.Ok(result));
    }

    [HttpPost("services")]
    public async Task<ActionResult<ApiResponse<MedicalServiceDetailDto>>> CreateService([FromBody] CreateMedicalServiceRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateServiceAsync(request, cancellationToken);
        return Ok(ApiResponse<MedicalServiceDetailDto>.Ok(result, "Tibbi prosedur əlavə edildi."));
    }

    [HttpPut("services/{id:guid}")]
    public async Task<ActionResult<ApiResponse<MedicalServiceDetailDto>>> UpdateService(Guid id, [FromBody] UpdateMedicalServiceRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateServiceAsync(id, request, cancellationToken);
        return Ok(ApiResponse<MedicalServiceDetailDto>.Ok(result, "Tibbi prosedur yeniləndi."));
    }

    [HttpDelete("services/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteService(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteServiceAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Tibbi prosedur silindi."));
    }

    [HttpPost("services/{id:guid}/hero-image")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateServiceHeroImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var fileDto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        var url = await _adminService.UpdateServiceHeroImageAsync(id, fileDto, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Prosedur əsas şəkli yeniləndi."));
    }

    [HttpPost("services/{id:guid}/promo-banner")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateServicePromoBanner(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var fileDto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        var url = await _adminService.UpdateServicePromoBannerAsync(id, fileDto, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Prosedur promo banneri yeniləndi."));
    }

    [HttpPost("services/price")]
    public async Task<ActionResult<ApiResponse<string>>> SetClinicPrice([FromBody] SetClinicPriceRequestDto request, CancellationToken cancellationToken)
    {
        await _adminService.SetClinicPriceAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Klinika üçün prosedur qiyməti təyin edildi."));
    }

    [HttpDelete("services/{serviceId:guid}/clinics/{clinicId:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteClinicPrice(Guid serviceId, Guid clinicId, CancellationToken cancellationToken)
    {
        await _adminService.DeleteClinicPriceAsync(serviceId, clinicId, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Klinika qiyməti silindi."));
    }

    [HttpPut("services/{serviceId:guid}/doctors")]
    public async Task<ActionResult<ApiResponse<string>>> SetServiceDoctors(
        Guid serviceId,
        [FromBody] SetServiceDoctorsRequestDto request,
        CancellationToken cancellationToken)
    {
        await _adminService.SetServiceDoctorsAsync(serviceId, request, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Prosedura bağlı həkimlər yeniləndi."));
    }

    // --- Packages ---
    [HttpPost("packages")]
    public async Task<ActionResult<ApiResponse<TreatmentPackageDto>>> CreatePackage([FromBody] CreateTreatmentPackageRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreatePackageAsync(request, cancellationToken);
        return Ok(ApiResponse<TreatmentPackageDto>.Ok(result, "Paket əlavə edildi."));
    }

    [HttpPut("packages/{id:guid}")]
    public async Task<ActionResult<ApiResponse<TreatmentPackageDto>>> UpdatePackage(Guid id, [FromBody] UpdateTreatmentPackageRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdatePackageAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TreatmentPackageDto>.Ok(result, "Paket yeniləndi."));
    }

    [HttpDelete("packages/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeletePackage(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeletePackageAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Paket silindi."));
    }

    [HttpPost("packages/{id:guid}/cover")]
    public async Task<ActionResult<ApiResponse<string>>> UpdatePackageCover(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var fileDto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        var url = await _adminService.UpdatePackageCoverAsync(id, fileDto, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Paket qapağı yeniləndi."));
    }

    // --- Content & About Us ---
    [HttpGet("pages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminCmsPageDto>>>> GetCmsPages(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllCmsPagesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminCmsPageDto>>.Ok(result));
    }

    [HttpGet("pages/{slug}")]
    public async Task<ActionResult<ApiResponse<AdminCmsPageDto>>> GetCmsPageBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _adminService.GetCmsPageBySlugForAdminAsync(slug, cancellationToken);
        return Ok(ApiResponse<AdminCmsPageDto>.Ok(result));
    }

    [HttpPost("pages")]
    public async Task<ActionResult<ApiResponse<AdminCmsPageDto>>> UpsertCmsPage([FromBody] UpsertCmsPageRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpsertCmsPageAsync(request, cancellationToken);
        return Ok(ApiResponse<AdminCmsPageDto>.Ok(result, "Səhifə saxlanıldı."));
    }

    [HttpDelete("pages/{slug}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteCmsPage(string slug, CancellationToken cancellationToken)
    {
        await _adminService.DeleteCmsPageAsync(slug, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Səhifə silindi."));
    }

    // --- About Us Pillars ---
    [HttpGet("about/pillars")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminAboutPillarDto>>>> GetAboutPillars(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllAboutPillarsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminAboutPillarDto>>.Ok(result));
    }

    [HttpPost("about/pillars")]
    public async Task<ActionResult<ApiResponse<AdminAboutPillarDto>>> CreateAboutPillar([FromBody] UpsertAboutPillarRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateAboutPillarAsync(request, cancellationToken);
        return Ok(ApiResponse<AdminAboutPillarDto>.Ok(result, "Dəyər əlavə edildi."));
    }

    [HttpPut("about/pillars/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminAboutPillarDto>>> UpdateAboutPillar(Guid id, [FromBody] UpsertAboutPillarRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateAboutPillarAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminAboutPillarDto>.Ok(result, "Dəyər yeniləndi."));
    }

    [HttpDelete("about/pillars/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteAboutPillar(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteAboutPillarAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Dəyər silindi."));
    }

    // --- About Us Team ---
    [HttpGet("about/team")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminTeamMemberDto>>>> GetTeamMembers(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllTeamMembersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminTeamMemberDto>>.Ok(result));
    }

    [HttpPost("about/team")]
    public async Task<ActionResult<ApiResponse<AdminTeamMemberDto>>> CreateTeamMember([FromBody] UpsertTeamMemberRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateTeamMemberAsync(request, cancellationToken);
        return Ok(ApiResponse<AdminTeamMemberDto>.Ok(result, "Komanda üzvü əlavə edildi."));
    }

    [HttpPut("about/team/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminTeamMemberDto>>> UpdateTeamMember(Guid id, [FromBody] UpsertTeamMemberRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateTeamMemberAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminTeamMemberDto>.Ok(result, "Komanda üzvü yeniləndi."));
    }

    [HttpDelete("about/team/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTeamMember(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteTeamMemberAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Komanda üzvü silindi."));
    }

    [HttpPost("about/team/{id:guid}/photo")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTeamMemberPhoto(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        var fileDto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        var url = await _adminService.UpdateTeamMemberPhotoAsync(id, fileDto, cancellationToken);
        return Ok(ApiResponse<string>.Ok(url, "Komanda üzvünün şəkli yeniləndi."));
    }

    // --- About Us How It Works ---
    [HttpGet("about/how-it-works")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminHowItWorksStepDto>>>> GetHowItWorksSteps(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllHowItWorksStepsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminHowItWorksStepDto>>.Ok(result));
    }

    [HttpPost("about/how-it-works")]
    public async Task<ActionResult<ApiResponse<AdminHowItWorksStepDto>>> CreateHowItWorksStep([FromBody] UpsertHowItWorksStepRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateHowItWorksStepAsync(request, cancellationToken);
        return Ok(ApiResponse<AdminHowItWorksStepDto>.Ok(result, "Addım əlavə edildi."));
    }

    [HttpPut("about/how-it-works/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminHowItWorksStepDto>>> UpdateHowItWorksStep(Guid id, [FromBody] UpsertHowItWorksStepRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateHowItWorksStepAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminHowItWorksStepDto>.Ok(result, "Addım yeniləndi."));
    }

    [HttpDelete("about/how-it-works/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteHowItWorksStep(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteHowItWorksStepAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Addım silindi."));
    }

    // --- Contact Messages ---
    [HttpGet("contact-messages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContactMessageDto>>>> GetContactMessages(
        [FromQuery] bool? isHandled,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetContactMessagesAsync(isHandled, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ContactMessageDto>>.Ok(result));
    }

    [HttpPatch("contact-messages/{id:guid}/handle")]
    public async Task<ActionResult<ApiResponse<ContactMessageDto>>> HandleContactMessage(Guid id, CancellationToken cancellationToken)
    {
        var result = await _adminService.HandleContactMessageAsync(id, cancellationToken);
        return Ok(ApiResponse<ContactMessageDto>.Ok(result, "Əlaqə mesajı icra edildi kimi qeyd olundu."));
    }

    [HttpPost("blog")]
    public async Task<ActionResult<ApiResponse<BlogPostDetailDto>>> CreateBlogPost([FromBody] BlogPostDetailDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateBlogPostAsync(request, cancellationToken);
        return Ok(ApiResponse<BlogPostDetailDto>.Ok(result, "Məqalə dərc edildi."));
    }

    [HttpDelete("blog/{slug}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteBlogPost(string slug, CancellationToken cancellationToken)
    {
        await _adminService.DeleteBlogPostAsync(slug, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Məqalə silindi."));
    }

    [HttpGet("faq")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AdminFaqItemDto>>>> GetFaqs(
        [FromQuery] Guid? medicalServiceId,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllFaqsAsync(medicalServiceId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AdminFaqItemDto>>.Ok(result));
    }

    [HttpPost("faq")]
    public async Task<ActionResult<ApiResponse<AdminFaqItemDto>>> CreateFaq([FromBody] UpsertFaqRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreateFaqAsync(request, cancellationToken);
        return Ok(ApiResponse<AdminFaqItemDto>.Ok(result, "Sual əlavə edildi."));
    }

    [HttpPut("faq/{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminFaqItemDto>>> UpdateFaq(Guid id, [FromBody] UpsertFaqRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateFaqAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AdminFaqItemDto>.Ok(result, "Sual yeniləndi."));
    }

    [HttpDelete("faq/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteFaq(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteFaqAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Sual silindi."));
    }

    [HttpPost("partners")]
    public async Task<ActionResult<ApiResponse<PartnerDto>>> CreatePartner([FromBody] PartnerDto request, CancellationToken cancellationToken)
    {
        var result = await _adminService.CreatePartnerAsync(request, cancellationToken);
        return Ok(ApiResponse<PartnerDto>.Ok(result, "Tərəfdaş əlavə edildi."));
    }

    [HttpDelete("partners/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeletePartner(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeletePartnerAsync(id, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Tərəfdaş silindi."));
    }

    // --- Notifications Settings (Telegram) ---
    [HttpPost("notifications/telegram/connect")]
    public async Task<ActionResult<ApiResponse<object>>> ConnectTelegramBot(
        [FromBody] TelegramConnectRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _notifications.ConnectAndVerifyTelegramBotAsync(request.BotToken, request.ChatId, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message, "TELEGRAM_CONNECTION_FAILED"));
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            botUsername = result.BotUsername,
            chatId = result.ChatId
        }, result.Message));
    }
}

public class TelegramConnectRequestDto
{
    public string BotToken { get; set; } = string.Empty;
    public string? ChatId { get; set; }
}

