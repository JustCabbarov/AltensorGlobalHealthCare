using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.DTOs.Inquiries;
using AltensorGlobalHealthCare.Application.DTOs.Offers;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.Services;

public class PatientService : IPatientService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationDispatcher _notifications;

    public PatientService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        INotificationDispatcher notifications)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _notifications = notifications;
    }

    public async Task<InquirySummaryDto> CreateInquiryAsync(
        Guid userId,
        CreateInquiryRequestDto request,
        IList<FileUploadDto>? documents,
        CancellationToken cancellationToken = default)
    {
        var inquiryNumber = await _unitOfWork.Inquiries.GenerateInquiryNumberAsync(cancellationToken);

        var inquiry = new Inquiry
        {
            InquiryNumber = inquiryNumber,
            PatientId = userId,
            MedicalServiceId = request.MedicalServiceId,
            Symptoms = request.Symptoms.Trim(),
            Description = request.Description?.Trim(),
            PreferredTimeframe = request.PreferredTimeframe?.Trim(),
            NeedsAccommodation = request.NeedsAccommodation,
            NeedsAirportTransfer = request.NeedsAirportTransfer,
            NeedsAirplaneTicket = request.NeedsAirplaneTicket,
            IsEmergency = request.IsEmergency,
            Status = InquiryStatus.Submitted
        };

        await _unitOfWork.Inquiries.AddAsync(inquiry, cancellationToken);

        // Upload documents if provided
        if (documents != null && documents.Count > 0)
        {
            if (documents.Count > 10)
            {
                throw new BadRequestException("Bir dəfəyə ən çox 10 sənəd yükləyə bilərsiniz.", "TOO_MANY_DOCUMENTS");
            }

            foreach (var file in documents)
            {
                if (file.Length > 20 * 1024 * 1024)
                {
                    throw new FileValidationException($"Fayl həcmi 20 MB-dan artıq ola bilməz: {file.FileName}");
                }

                var storagePath = await _fileStorage.SaveSecureMedicalDocumentAsync(file.Stream, file.FileName, inquiry.Id, cancellationToken);
                var doc = new InquiryDocument
                {
                    InquiryId = inquiry.Id,
                    OriginalFileName = file.FileName,
                    StoragePath = storagePath,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length,
                    UploadedByUserId = userId
                };
                inquiry.Documents.Add(doc);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify Coordinator
        var created = await _unitOfWork.Inquiries.GetWithDetailsAsync(inquiry.Id, cancellationToken);
        if (created != null)
        {
            await _notifications.SendInquiryNotificationToCoordinatorAsync(created, cancellationToken);
        }

        return new InquirySummaryDto
        {
            Id = inquiry.Id,
            InquiryNumber = inquiry.InquiryNumber,
            Symptoms = inquiry.Symptoms,
            ServiceTitle = created?.MedicalService?.TitleAz,
            Status = inquiry.Status,
            StatusText = GetStatusText(inquiry.Status),
            DocumentsCount = inquiry.Documents.Count,
            OffersCount = 0,
            CreatedAtUtc = inquiry.CreatedAtUtc
        };
    }

    public async Task<PagedResult<InquirySummaryDto>> GetInquiriesAsync(
        Guid userId,
        InquiryStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var all = await _unitOfWork.Inquiries.GetListAsync(
            i => i.PatientId == userId && (!status.HasValue || i.Status == status),
            cancellationToken,
            i => i.MedicalService!,
            i => i.Documents,
            i => i.Offers);

        var total = all.Count;
        var paged = all
            .OrderByDescending(i => i.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InquirySummaryDto
            {
                Id = i.Id,
                InquiryNumber = i.InquiryNumber,
                Symptoms = i.Symptoms,
                ServiceTitle = i.MedicalService?.TitleAz,
                Status = i.Status,
                StatusText = GetStatusText(i.Status),
                DocumentsCount = i.Documents.Count,
                OffersCount = i.Offers.Count,
                CreatedAtUtc = i.CreatedAtUtc
            }).ToList();

        return new PagedResult<InquirySummaryDto>(paged, total, page, pageSize);
    }

    public async Task<InquiryDetailDto> GetInquiryByIdAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetWithDetailsAsync(inquiryId, cancellationToken);
        if (inquiry == null)
        {
            throw NotFoundException.For<Inquiry>(inquiryId);
        }

        if (inquiry.PatientId != userId)
        {
            throw new ForbiddenException("Bu müraciətə baxmaq hüququnuz yoxdur.");
        }

        return MapToDetailDto(inquiry, userId);
    }

    public async Task<InquiryDocumentDto> UploadDocumentAsync(Guid userId, Guid inquiryId, FileUploadDto file, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);
        if (inquiry.PatientId != userId) throw new ForbiddenException();

        if (file.Length > 20 * 1024 * 1024)
        {
            throw new FileValidationException("Fayl həcmi 20 MB-dan çox ola bilməz.");
        }

        var storagePath = await _fileStorage.SaveSecureMedicalDocumentAsync(file.Stream, file.FileName, inquiryId, cancellationToken);
        var doc = new InquiryDocument
        {
            InquiryId = inquiryId,
            OriginalFileName = file.FileName,
            StoragePath = storagePath,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            UploadedByUserId = userId
        };

        await _unitOfWork.InquiryDocuments.AddAsync(doc, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InquiryDocumentDto
        {
            Id = doc.Id,
            OriginalFileName = doc.OriginalFileName,
            ContentType = doc.ContentType,
            SizeBytes = doc.SizeBytes,
            CreatedAtUtc = doc.CreatedAtUtc
        };
    }

    public async Task DeleteDocumentAsync(Guid userId, Guid inquiryId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);
        if (inquiry.PatientId != userId) throw new ForbiddenException();

        var doc = await _unitOfWork.InquiryDocuments.FindAsync(d => d.Id == documentId && d.InquiryId == inquiryId, cancellationToken);
        if (doc == null) throw NotFoundException.For<InquiryDocument>(documentId);

        _fileStorage.DeleteFile(doc.StoragePath);
        _unitOfWork.InquiryDocuments.Remove(doc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> UpdateInquiryProcedureAsync(Guid userId, Guid inquiryId, Guid medicalServiceId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);
        if (inquiry.PatientId != userId) throw new ForbiddenException();

        var service = await _unitOfWork.MedicalServices.GetByIdAsync(medicalServiceId, cancellationToken);
        if (service == null) throw NotFoundException.For<MedicalService>(medicalServiceId);

        inquiry.MedicalServiceId = medicalServiceId;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return "Müraciət etdiyiniz tibbi prosedur yeniləndi.";
    }

    public async Task<string> CancelInquiryAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);
        if (inquiry.PatientId != userId) throw new ForbiddenException();

        if (inquiry.Status == InquiryStatus.Accepted || inquiry.Status == InquiryStatus.Closed)
        {
            throw new BusinessRuleException("Qəbul edilmiş və ya bağlanmış müraciəti ləğv etmək mümkün deyil.", "CANNOT_CANCEL_PROCESSED_INQUIRY");
        }

        inquiry.ChangeStatus(InquiryStatus.Cancelled);
        _unitOfWork.Inquiries.Remove(inquiry); // Soft-delete
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return "Müraciət ləğv edildi.";
    }

    public async Task<string> RestoreInquiryAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdIncludingDeletedAsync(inquiryId, cancellationToken);

        if (inquiry == null || inquiry.PatientId != userId) throw NotFoundException.For<Inquiry>(inquiryId);

        inquiry.IsDeleted = false;
        inquiry.DeletedAtUtc = null;
        inquiry.ChangeStatus(InquiryStatus.Submitted);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return "Müraciət bərpa edildi.";
    }

    public async Task<IReadOnlyList<OfferDto>> GetOffersForInquiryAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);
        if (inquiry.PatientId != userId) throw new ForbiddenException();

        var offers = await _unitOfWork.Offers.GetByInquiryIdAsync(inquiryId, cancellationToken);
        // Only return offers with status Sent, Accepted, Declined, Expired (not Draft)
        return offers
            .Where(o => o.Status != OfferStatus.Draft)
            .Select(MapOfferToDto)
            .ToList();
    }

    public async Task<string> AcceptOfferAsync(Guid userId, Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await _unitOfWork.Offers.GetWithDetailsAsync(offerId, cancellationToken);
        if (offer == null) throw NotFoundException.For<Offer>(offerId);

        if (offer.Inquiry.PatientId != userId) throw new ForbiddenException();

        if (offer.Status != OfferStatus.Sent)
        {
            throw new BusinessRuleException("Yalnız aktiv təklifləri qəbul etmək mümkündür.", "INVALID_OFFER_STATUS");
        }

        if (offer.IsExpired)
        {
            offer.Status = OfferStatus.Expired;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new BusinessRuleException("Bu təklifin etibarlılıq müddəti bitib.", "OFFER_EXPIRED");
        }

        // Accept this offer
        offer.Status = OfferStatus.Accepted;

        // Auto-decline other pending/sent offers for the same inquiry
        var otherOffers = await _unitOfWork.Offers.GetListAsync(
            o => o.InquiryId == offer.InquiryId && o.Id != offer.Id && o.Status == OfferStatus.Sent,
            cancellationToken);

        foreach (var other in otherOffers)
        {
            other.Status = OfferStatus.Declined;
        }

        // Mark inquiry as Accepted
        offer.Inquiry.ChangeStatus(InquiryStatus.Accepted);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return "Təklif qəbul edildi! Pasient koordinatorumuz tezliklə səfər və müalicə detallarını təşkil etmək üçün sizinlə əlaqə saxlayacaq.";
    }

    public async Task<string> DeclineOfferAsync(Guid userId, Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await _unitOfWork.Offers.GetWithDetailsAsync(offerId, cancellationToken);
        if (offer == null) throw NotFoundException.For<Offer>(offerId);
        if (offer.Inquiry.PatientId != userId) throw new ForbiddenException();

        if (offer.Status != OfferStatus.Sent)
        {
            throw new BusinessRuleException("Yalnız aktiv təkliflərdən imtina etmək mümkündür.", "INVALID_OFFER_STATUS");
        }

        offer.Status = OfferStatus.Declined;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return "Təklifdən imtina edildi.";
    }

    public async Task<IReadOnlyList<InquiryMessageDto>> GetInquiryMessagesAsync(Guid userId, Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);
        if (inquiry.PatientId != userId) throw new ForbiddenException();

        var messages = await _unitOfWork.InquiryMessages.GetListAsync(
            m => m.InquiryId == inquiryId, cancellationToken);

        // Mark messages not sent by patient as read
        foreach (var m in messages.Where(m => m.SenderUserId != userId && m.ReadAtUtc == null))
        {
            m.ReadAtUtc = DateTime.UtcNow;
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return messages.OrderBy(m => m.CreatedAtUtc).Select(m => new InquiryMessageDto
        {
            Id = m.Id,
            SenderUserId = m.SenderUserId,
            SenderName = m.SenderName,
            SenderRole = m.SenderRole,
            Body = m.Body,
            ReadAtUtc = m.ReadAtUtc,
            CreatedAtUtc = m.CreatedAtUtc,
            IsMine = m.SenderUserId == userId
        }).ToList();
    }

    public async Task<InquiryMessageDto> SendMessageAsync(Guid userId, Guid inquiryId, SendMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetWithDetailsAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);
        if (inquiry.PatientId != userId) throw new ForbiddenException();

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            throw new BadRequestException("Mesaj mətni boş ola bilməz.", "EMPTY_MESSAGE");
        }

        var message = new InquiryMessage
        {
            InquiryId = inquiryId,
            SenderUserId = userId,
            SenderName = inquiry.Patient.FullName,
            SenderRole = "Patient",
            Body = request.Body.Trim()
        };

        await _unitOfWork.InquiryMessages.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify Coordinator or Doctor
        var coordinatorEmail = "hospitality@altensorglobalhealthcare.com";
        await _notifications.SendNewMessageNotificationAsync(inquiry, message, coordinatorEmail, cancellationToken);

        return new InquiryMessageDto
        {
            Id = message.Id,
            SenderUserId = userId,
            SenderName = message.SenderName,
            SenderRole = message.SenderRole,
            Body = message.Body,
            CreatedAtUtc = message.CreatedAtUtc,
            IsMine = true
        };
    }

    private static InquiryDetailDto MapToDetailDto(Inquiry inquiry, Guid currentUserId)
    {
        return new InquiryDetailDto
        {
            Id = inquiry.Id,
            InquiryNumber = inquiry.InquiryNumber,
            PatientId = inquiry.PatientId,
            PatientName = inquiry.Patient?.FullName ?? string.Empty,
            PatientEmail = inquiry.Patient?.Email ?? string.Empty,
            PatientPhone = inquiry.Patient?.PhoneNumber,
            PatientCountry = inquiry.Patient?.Country,
            MedicalServiceId = inquiry.MedicalServiceId,
            ServiceTitle = inquiry.MedicalService?.TitleAz,
            CategoryTitle = inquiry.MedicalService?.Category?.TitleAz,
            AssignedDoctorId = inquiry.AssignedDoctorId,
            AssignedDoctorName = inquiry.AssignedDoctor?.FullName,
            AssignedClinicName = inquiry.AssignedDoctor?.Clinic?.Name,
            Symptoms = inquiry.Symptoms,
            Description = inquiry.Description,
            PreferredTimeframe = inquiry.PreferredTimeframe,
            NeedsAccommodation = inquiry.NeedsAccommodation,
            NeedsAirportTransfer = inquiry.NeedsAirportTransfer,
            NeedsAirplaneTicket = inquiry.NeedsAirplaneTicket,
            IsEmergency = inquiry.IsEmergency,
            Status = inquiry.Status,
            StatusText = GetStatusText(inquiry.Status),
            CreatedAtUtc = inquiry.CreatedAtUtc,
            UpdatedAtUtc = inquiry.UpdatedAtUtc,
            Documents = inquiry.Documents.Select(d => new InquiryDocumentDto
            {
                Id = d.Id,
                OriginalFileName = d.OriginalFileName,
                ContentType = d.ContentType,
                SizeBytes = d.SizeBytes,
                CreatedAtUtc = d.CreatedAtUtc
            }).ToList(),
            Messages = inquiry.Messages.OrderBy(m => m.CreatedAtUtc).Select(m => new InquiryMessageDto
            {
                Id = m.Id,
                SenderUserId = m.SenderUserId,
                SenderName = m.SenderName,
                SenderRole = m.SenderRole,
                Body = m.Body,
                ReadAtUtc = m.ReadAtUtc,
                CreatedAtUtc = m.CreatedAtUtc,
                IsMine = m.SenderUserId == currentUserId
            }).ToList(),
            Offers = inquiry.Offers.Where(o => o.Status != OfferStatus.Draft).Select(MapOfferToDto).ToList()
        };
    }

    private static OfferDto MapOfferToDto(Offer o)
    {
        return new OfferDto
        {
            Id = o.Id,
            InquiryId = o.InquiryId,
            InquiryNumber = o.Inquiry?.InquiryNumber ?? string.Empty,
            ClinicId = o.ClinicId,
            ClinicName = o.Clinic?.Name ?? string.Empty,
            ClinicLogoUrl = o.Clinic?.LogoUrl,
            DoctorId = o.DoctorId,
            DoctorName = o.Doctor?.FullName,
            Title = o.Title,
            Text = o.Text,
            MedicalTreatmentPrice = o.MedicalTreatmentPrice,
            AccommodationPrice = o.AccommodationPrice,
            AirportTransferPrice = o.AirportTransferPrice,
            AirplaneTicketPrice = o.AirplaneTicketPrice,
            TotalPrice = o.TotalPrice,
            Currency = o.Currency,
            ProcedureDate = o.ProcedureDate,
            ExpiresAtUtc = o.ExpiresAtUtc,
            Status = o.Status,
            StatusText = GetOfferStatusText(o.Status),
            IsExpired = o.IsExpired,
            CreatedAtUtc = o.CreatedAtUtc,
            Attachments = o.Attachments.Select(a => new OfferAttachmentDto
            {
                Id = a.Id,
                OriginalFileName = a.OriginalFileName,
                ContentType = a.ContentType,
                SizeBytes = a.SizeBytes
            }).ToList()
        };
    }

    private static string GetStatusText(InquiryStatus status) => status switch
    {
        InquiryStatus.Submitted => "Göndərildi",
        InquiryStatus.InReview => "Baxılır",
        InquiryStatus.Processing => "İcradadır",
        InquiryStatus.OfferReceived => "Təklif Alındı",
        InquiryStatus.Accepted => "Qəbul Edildi",
        InquiryStatus.Declined => "İmtina Edildi",
        InquiryStatus.Closed => "Bağlandı",
        InquiryStatus.Cancelled => "Ləğv Edildi",
        _ => status.ToString()
    };

    private static string GetOfferStatusText(OfferStatus status) => status switch
    {
        OfferStatus.Draft => "Qaralama",
        OfferStatus.Sent => "Göndərildi",
        OfferStatus.Accepted => "Qəbul Edildi",
        OfferStatus.Declined => "İmtina Edildi",
        OfferStatus.Expired => "Müddəti Bitdi",
        OfferStatus.Withdrawn => "Geri Çağırıldı",
        _ => status.ToString()
    };
}
