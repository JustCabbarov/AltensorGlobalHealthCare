using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.DTOs.Inquiries;
using AltensorGlobalHealthCare.Application.DTOs.Offers;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.WebApi.Controllers;

[ApiController]
[Route("api/patient")]
[Authorize(Roles = "Patient")]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public PatientController(
        IPatientService patientService,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
    {
        _patientService = patientService;
        _currentUserService = currentUserService;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    private Guid CurrentUserId => _currentUserService.UserId
        ?? throw new UnauthorizedException("İstifadəçi müəyyən edilmədi.");

    /// <summary>
    /// Yeni tibbi konsultasiya və müalicə müraciətinin göndərilməsi (ZagrebMed /submitinquiry analoqu)
    /// </summary>
    [HttpPost("inquiries")]
    public async Task<ActionResult<ApiResponse<InquirySummaryDto>>> CreateInquiry(
        [FromForm] CreateInquiryRequestDto request,
        [FromForm] List<IFormFile>? documents,
        CancellationToken cancellationToken)
    {
        var fileDtos = documents?.Select(f => new FileUploadDto(f.OpenReadStream(), f.FileName, f.ContentType, f.Length)).ToList();
        var result = await _patientService.CreateInquiryAsync(CurrentUserId, request, fileDtos, cancellationToken);
        return Ok(ApiResponse<InquirySummaryDto>.Ok(result, "Müraciətiniz uğurla qəbul edildi."));
    }

    /// <summary>
    /// Pasientin bütün müraciətlərinin siyahısı (status və səhifələmə ilə) (ZagrebMed /user/getAllInquiries analoqu)
    /// </summary>
    [HttpGet("inquiries")]
    public async Task<ActionResult<ApiResponse<PagedResult<InquirySummaryDto>>>> GetInquiries(
        [FromQuery] InquiryStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _patientService.GetInquiriesAsync(CurrentUserId, status, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<InquirySummaryDto>>.Ok(result));
    }

    /// <summary>
    /// Konkret müraciətin bütün detalları (sənədlər, mesajlar, təkliflər)
    /// </summary>
    [HttpGet("inquiries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<InquiryDetailDto>>> GetInquiryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _patientService.GetInquiryByIdAsync(CurrentUserId, id, cancellationToken);
        return Ok(ApiResponse<InquiryDetailDto>.Ok(result));
    }

    /// <summary>
    /// Mövcud müraciətə əlavə tibbi sənəd yükləmək (ZagrebMed PUT /inquiry/{id}/{doc} analoqu)
    /// </summary>
    [HttpPost("inquiries/{id:guid}/documents")]
    public async Task<ActionResult<ApiResponse<InquiryDocumentDto>>> UploadDocument(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var fileDto = new FileUploadDto(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        var result = await _patientService.UploadDocumentAsync(CurrentUserId, id, fileDto, cancellationToken);
        return Ok(ApiResponse<InquiryDocumentDto>.Ok(result, "Sənəd uğurla əlavə edildi."));
    }

    /// <summary>
    /// Yüklənmiş sənədi silmək (ZagrebMed DELETE /inquiry/{id}/{doc} analoqu)
    /// </summary>
    [HttpDelete("inquiries/{id:guid}/documents/{documentId:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteDocument(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        await _patientService.DeleteDocumentAsync(CurrentUserId, id, documentId, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Sənəd silindi."));
    }

    /// <summary>
    /// Qorunan tibbi sənədi təhlükəsiz stream kimi yükləmək (Yalnız sahib pasient və ya səlahiyyətli həkim/koordinator)
    /// </summary>
    [HttpGet("inquiries/{id:guid}/documents/{documentId:guid}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(id, cancellationToken);
        if (inquiry == null || inquiry.PatientId != CurrentUserId) throw new ForbiddenException();

        var doc = await _unitOfWork.InquiryDocuments.FindAsync(d => d.Id == documentId && d.InquiryId == id, cancellationToken);
        if (doc == null) throw NotFoundException.For<InquiryDocument>(documentId);

        var fileResult = _fileStorage.GetSecureMedicalDocument(doc.StoragePath);
        if (fileResult == null) return NotFound("Fayl serverdə tapılmadı.");

        return File(fileResult.Value.FileStream, fileResult.Value.ContentType, doc.OriginalFileName);
    }

    /// <summary>
    /// Müraciət edilən tibbi proseduru dəyişdirmək (ZagrebMed /updateInquiryProcedure analoqu)
    /// </summary>
    [HttpPatch("inquiries/{id:guid}/procedure")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateInquiryProcedure(
        Guid id,
        [FromBody] UpdateInquiryProcedureRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _patientService.UpdateInquiryProcedureAsync(CurrentUserId, id, request.MedicalServiceId, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Müraciəti ləğv etmək / silmək (ZagrebMed DELETE /deleteinquiry analoqu)
    /// </summary>
    [HttpDelete("inquiries/{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> CancelInquiry(Guid id, CancellationToken cancellationToken)
    {
        var result = await _patientService.CancelInquiryAsync(CurrentUserId, id, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Silinmiş müraciəti geri qaytarmaq / bərpa etmək
    /// </summary>
    [HttpPost("inquiries/{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<string>>> RestoreInquiry(Guid id, CancellationToken cancellationToken)
    {
        var result = await _patientService.RestoreInquiryAsync(CurrentUserId, id, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Müraciət üzrə klinikalardan gələn bütün rəsmi təkliflər (ZagrebMed /user/getAllOffers analoqu)
    /// </summary>
    [HttpGet("inquiries/{id:guid}/offers")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<OfferDto>>>> GetOffers(Guid id, CancellationToken cancellationToken)
    {
        var result = await _patientService.GetOffersForInquiryAsync(CurrentUserId, id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OfferDto>>.Ok(result));
    }

    /// <summary>
    /// Klinikanın təklifini qəbul etmək (Digər təkliflər avtomatik imtina edilir)
    /// </summary>
    [HttpPost("offers/{offerId:guid}/accept")]
    public async Task<ActionResult<ApiResponse<string>>> AcceptOffer(Guid offerId, CancellationToken cancellationToken)
    {
        var result = await _patientService.AcceptOfferAsync(CurrentUserId, offerId, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Təklifdən imtina etmək
    /// </summary>
    [HttpPost("offers/{offerId:guid}/decline")]
    public async Task<ActionResult<ApiResponse<string>>> DeclineOffer(Guid offerId, CancellationToken cancellationToken)
    {
        var result = await _patientService.DeclineOfferAsync(CurrentUserId, offerId, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Müraciət üzrə koordinator və həkimlə yazışma tarixçəsi (ZagrebMed /getAllMessages analoqu)
    /// </summary>
    [HttpGet("inquiries/{id:guid}/messages")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InquiryMessageDto>>>> GetMessages(Guid id, CancellationToken cancellationToken)
    {
        var result = await _patientService.GetInquiryMessagesAsync(CurrentUserId, id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<InquiryMessageDto>>.Ok(result));
    }

    /// <summary>
    /// Müraciət çatı daxilində koordinatora/həkimə mesaj göndərmək (ZagrebMed /submitMessage analoqu)
    /// </summary>
    [HttpPost("inquiries/{id:guid}/messages")]
    public async Task<ActionResult<ApiResponse<InquiryMessageDto>>> SendMessage(
        Guid id,
        [FromBody] SendMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _patientService.SendMessageAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(ApiResponse<InquiryMessageDto>.Ok(result, "Mesaj çatdırıldı."));
    }
}
