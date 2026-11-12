using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.DTOs.Admin;
using AltensorGlobalHealthCare.Application.DTOs.Inquiries;
using AltensorGlobalHealthCare.Application.DTOs.Offers;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.WebApi.Controllers;

[ApiController]
[Route("api/coordinator")]
[Authorize(Roles = "Admin,Coordinator")]
public class CoordinatorController : ControllerBase
{
    private readonly ICoordinatorService _coordinatorService;
    private readonly ICurrentUserService _currentUserService;

    public CoordinatorController(ICoordinatorService coordinatorService, ICurrentUserService currentUserService)
    {
        _coordinatorService = coordinatorService;
        _currentUserService = currentUserService;
    }

    private Guid CurrentUserId => _currentUserService.UserId
        ?? throw new UnauthorizedException("İstifadəçi müəyyən edilmədi.");

    /// <summary>
    /// Bütün pasient müraciətlərinin siyahısı və filtrlənməsi
    /// </summary>
    [HttpGet("inquiries")]
    public async Task<ActionResult<ApiResponse<PagedResult<InquirySummaryDto>>>> GetInquiries(
        [FromQuery] InquiryStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        CancellationToken cancellationToken = default)
    {
        var result = await _coordinatorService.GetInquiriesAsync(status, search, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<InquirySummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Müraciətin bütün detalları (pasient məlumatı, sənədlər, mesajlar, təkliflər)
    /// </summary>
    [HttpGet("inquiries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<InquiryDetailDto>>> GetInquiryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _coordinatorService.GetInquiryByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<InquiryDetailDto>.Ok(result));
    }

    /// <summary>
    /// Müraciəti klinikaya və ya həkimə təyin etmək
    /// </summary>
    [HttpPost("inquiries/{id:guid}/assign")]
    public async Task<ActionResult<ApiResponse<string>>> AssignDoctor(
        Guid id,
        [FromBody] AssignDoctorRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _coordinatorService.AssignDoctorAsync(id, request.DoctorId, CurrentUserId, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Müraciətin cari statusunu yeniləmək
    /// </summary>
    [HttpPatch("inquiries/{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(
        Guid id,
        [FromBody] UpdateInquiryStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _coordinatorService.UpdateStatusAsync(id, request.Status, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Müraciət üzrə rəsmi qiymət təklifi (Offer) yaratmaq (ZagrebMed /submitoffer analoqu)
    /// </summary>
    [HttpPost("inquiries/{id:guid}/offers")]
    public async Task<ActionResult<ApiResponse<OfferDto>>> CreateOffer(
        Guid id,
        [FromForm] CreateOfferRequestDto request,
        [FromForm] List<IFormFile>? attachments,
        CancellationToken cancellationToken)
    {
        request.InquiryId = id;
        var fileDtos = attachments?.Select(f => new FileUploadDto(f.OpenReadStream(), f.FileName, f.ContentType, f.Length)).ToList();
        var result = await _coordinatorService.CreateOfferAsync(request, fileDtos, cancellationToken);
        return Ok(ApiResponse<OfferDto>.Ok(result, "Təklif qaralaması yaradıldı."));
    }

    /// <summary>
    /// Təklifi təsdiqləyib pasientə göndərmək
    /// </summary>
    [HttpPost("offers/{offerId:guid}/send")]
    public async Task<ActionResult<ApiResponse<string>>> SendOffer(Guid offerId, CancellationToken cancellationToken)
    {
        var result = await _coordinatorService.SendOfferAsync(offerId, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Göndərilmiş təklifi geri çağırmaq / ləğv etmək (ZagrebMed DELETE /deleteoffer analoqu)
    /// </summary>
    [HttpPost("offers/{offerId:guid}/withdraw")]
    public async Task<ActionResult<ApiResponse<string>>> WithdrawOffer(Guid offerId, CancellationToken cancellationToken)
    {
        var result = await _coordinatorService.WithdrawOfferAsync(offerId, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Müraciət daxili yazışmalar
    /// </summary>
    [HttpGet("inquiries/{id:guid}/messages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InquiryMessageDto>>>> GetMessages(Guid id, CancellationToken cancellationToken)
    {
        var result = await _coordinatorService.GetMessagesAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<InquiryMessageDto>>.Ok(result));
    }

    /// <summary>
    /// Pasientə və ya həkimə koordinator adından mesaj göndərmək
    /// </summary>
    [HttpPost("inquiries/{id:guid}/messages")]
    public async Task<ActionResult<ApiResponse<InquiryMessageDto>>> SendMessage(
        Guid id,
        [FromBody] SendMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _coordinatorService.SendMessageAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(ApiResponse<InquiryMessageDto>.Ok(result, "Mesaj göndərildi."));
    }

    /// <summary>
    /// Koordinator idarəetmə paneli üçün statistik göstəricilər
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _coordinatorService.GetDashboardStatsAsync(cancellationToken);
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(result));
    }
}

