using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.DTOs.Inquiries;
using AltensorGlobalHealthCare.Application.DTOs.Offers;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.WebApi.Controllers;

[ApiController]
[Route("api/doctor")]
[Authorize(Roles = "Doctor")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly ICurrentUserService _currentUserService;

    public DoctorController(IDoctorService doctorService, ICurrentUserService currentUserService)
    {
        _doctorService = doctorService;
        _currentUserService = currentUserService;
    }

    private Guid CurrentUserId => _currentUserService.UserId
        ?? throw new UnauthorizedException("İstifadəçi müəyyən edilmədi.");

    /// <summary>
    /// Həkimə təyin edilmiş pasient müraciətlərinin siyahısı
    /// </summary>
    [HttpGet("inquiries")]
    public async Task<ActionResult<ApiResponse<PagedResult<InquirySummaryDto>>>> GetAssignedInquiries(
        [FromQuery] InquiryStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15,
        CancellationToken cancellationToken = default)
    {
        var result = await _doctorService.GetAssignedInquiriesAsync(CurrentUserId, status, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<InquirySummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Həkimin təyin olunduğu müraciətin tibbi detalları və sənədləri
    /// </summary>
    [HttpGet("inquiries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<InquiryDetailDto>>> GetInquiryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetInquiryByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(ApiResponse<InquiryDetailDto>.Ok(result));
    }

    /// <summary>
    /// Müraciət üçün həkim tərəfindən ilkin qiymət və müalicə təklifi qaralaması hazırlanması
    /// </summary>
    [HttpPost("inquiries/{id:guid}/offers")]
    public async Task<ActionResult<ApiResponse<OfferDto>>> CreateDraftOffer(
        Guid id,
        [FromForm] CreateOfferRequestDto request,
        [FromForm] List<IFormFile>? attachments,
        CancellationToken cancellationToken)
    {
        request.InquiryId = id;
        var fileDtos = attachments?.Select(f => new FileUploadDto(f.OpenReadStream(), f.FileName, f.ContentType, f.Length)).ToList();
        var result = await _doctorService.CreateDraftOfferAsync(CurrentUserId, request, fileDtos, cancellationToken);
        return Ok(ApiResponse<OfferDto>.Ok(result, "Təklif qaralaması yaradıldı."));
    }

    /// <summary>
    /// Qaralama təklifin qiymət və şərtlərinin yenilənməsi
    /// </summary>
    [HttpPut("offers/{offerId:guid}")]
    public async Task<ActionResult<ApiResponse<OfferDto>>> UpdateOffer(
        Guid offerId,
        [FromBody] UpdateOfferRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _doctorService.UpdateOfferAsync(CurrentUserId, offerId, request, cancellationToken);
        return Ok(ApiResponse<OfferDto>.Ok(result, "Təklif yeniləndi."));
    }

    /// <summary>
    /// Həkim və pasient/koordinator arasında yazışmalar
    /// </summary>
    [HttpGet("inquiries/{id:guid}/messages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InquiryMessageDto>>>> GetMessages(Guid id, CancellationToken cancellationToken)
    {
        var result = await _doctorService.GetMessagesAsync(CurrentUserId, id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<InquiryMessageDto>>.Ok(result));
    }

    /// <summary>
    /// Həkim tərəfindən pasientə/koordinatora mesaj göndərilməsi
    /// </summary>
    [HttpPost("inquiries/{id:guid}/messages")]
    public async Task<ActionResult<ApiResponse<InquiryMessageDto>>> SendMessage(
        Guid id,
        [FromBody] SendMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _doctorService.SendMessageAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(ApiResponse<InquiryMessageDto>.Ok(result, "Mesaj göndərildi."));
    }
}

