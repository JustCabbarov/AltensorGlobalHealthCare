using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.DTOs.Inquiries;
using AltensorGlobalHealthCare.Application.DTOs.Offers;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationDispatcher _notifications;

    public DoctorService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        INotificationDispatcher notifications)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _notifications = notifications;
    }

    public async Task<PagedResult<InquirySummaryDto>> GetAssignedInquiriesAsync(
        Guid doctorUserId,
        InquiryStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 15;

        var doctor = await GetDoctorByUserIdAsync(doctorUserId, cancellationToken);

        var all = await _unitOfWork.Inquiries.GetListAsync(
            i => i.AssignedDoctorId == doctor.Id && (!status.HasValue || i.Status == status),
            cancellationToken);

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
                StatusText = i.Status.ToString(),
                DocumentsCount = i.Documents.Count,
                OffersCount = i.Offers.Count,
                CreatedAtUtc = i.CreatedAtUtc
            }).ToList();

        return new PagedResult<InquirySummaryDto>(paged, total, page, pageSize);
    }

    public async Task<InquiryDetailDto> GetInquiryByIdAsync(Guid doctorUserId, Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var doctor = await GetDoctorByUserIdAsync(doctorUserId, cancellationToken);
        var inquiry = await _unitOfWork.Inquiries.GetWithDetailsAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);

        if (inquiry.AssignedDoctorId != doctor.Id)
        {
            throw new ForbiddenException("Bu müraciət sizin adınıza təyin edilməyib.");
        }

        return MapToDetailDto(inquiry, doctorUserId);
    }

    public async Task<OfferDto> CreateDraftOfferAsync(
        Guid doctorUserId,
        CreateOfferRequestDto request,
        IList<FileUploadDto>? attachments,
        CancellationToken cancellationToken = default)
    {
        var doctor = await GetDoctorByUserIdAsync(doctorUserId, cancellationToken);
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(request.InquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(request.InquiryId);

        if (inquiry.AssignedDoctorId != doctor.Id)
        {
            throw new ForbiddenException("Yalnız sizə təyin olunan müraciətlər üçün təklif hazırlaya bilərsiniz.");
        }

        var offer = new Offer
        {
            InquiryId = request.InquiryId,
            ClinicId = doctor.ClinicId,
            DoctorId = doctor.Id,
            Title = request.Title.Trim(),
            Text = request.Text?.Trim(),
            MedicalTreatmentPrice = request.MedicalTreatmentPrice,
            AccommodationPrice = request.AccommodationPrice,
            AirportTransferPrice = request.AirportTransferPrice,
            AirplaneTicketPrice = request.AirplaneTicketPrice,
            Currency = request.Currency,
            ProcedureDate = request.ProcedureDate,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(request.ExpiresInDays > 0 ? request.ExpiresInDays : 14),
            Status = OfferStatus.Draft // Doctor creates draft; Coordinator sends
        };

        if (attachments != null)
        {
            foreach (var file in attachments)
            {
                var storagePath = await _fileStorage.SavePublicFileAsync(file.Stream, file.FileName, "offer-docs", cancellationToken);
                offer.Attachments.Add(new OfferAttachment
                {
                    OriginalFileName = file.FileName,
                    StoragePath = storagePath,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length
                });
            }
        }

        await _unitOfWork.Offers.AddAsync(offer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapOfferToDto(offer);
    }

    public async Task<OfferDto> UpdateOfferAsync(
        Guid doctorUserId,
        Guid offerId,
        UpdateOfferRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var doctor = await GetDoctorByUserIdAsync(doctorUserId, cancellationToken);
        var offer = await _unitOfWork.Offers.GetWithDetailsAsync(offerId, cancellationToken);
        if (offer == null) throw NotFoundException.For<Offer>(offerId);

        if (offer.DoctorId != doctor.Id)
        {
            throw new ForbiddenException("Yalnız öz hazırladığınız təklifi redaktə edə bilərsiniz.");
        }

        if (offer.Status != OfferStatus.Draft)
        {
            throw new BusinessRuleException("Yalnız qaralama statusunda olan təklif redaktə edilə bilər.", "OFFER_ALREADY_SENT");
        }

        offer.Title = request.Title.Trim();
        offer.Text = request.Text?.Trim();
        offer.MedicalTreatmentPrice = request.MedicalTreatmentPrice;
        offer.AccommodationPrice = request.AccommodationPrice;
        offer.AirportTransferPrice = request.AirportTransferPrice;
        offer.AirplaneTicketPrice = request.AirplaneTicketPrice;
        offer.Currency = request.Currency;
        offer.ProcedureDate = request.ProcedureDate;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapOfferToDto(offer);
    }

    public async Task<IReadOnlyList<InquiryMessageDto>> GetMessagesAsync(Guid doctorUserId, Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var doctor = await GetDoctorByUserIdAsync(doctorUserId, cancellationToken);
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);

        if (inquiry.AssignedDoctorId != doctor.Id)
        {
            throw new ForbiddenException();
        }

        var messages = await _unitOfWork.InquiryMessages.GetListAsync(m => m.InquiryId == inquiryId, cancellationToken);
        return messages.OrderBy(m => m.CreatedAtUtc).Select(m => new InquiryMessageDto
        {
            Id = m.Id,
            SenderUserId = m.SenderUserId,
            SenderName = m.SenderName,
            SenderRole = m.SenderRole,
            Body = m.Body,
            ReadAtUtc = m.ReadAtUtc,
            CreatedAtUtc = m.CreatedAtUtc,
            IsMine = m.SenderUserId == doctorUserId
        }).ToList();
    }

    public async Task<InquiryMessageDto> SendMessageAsync(Guid doctorUserId, Guid inquiryId, SendMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        var doctor = await GetDoctorByUserIdAsync(doctorUserId, cancellationToken);
        var inquiry = await _unitOfWork.Inquiries.GetWithDetailsAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);

        if (inquiry.AssignedDoctorId != doctor.Id)
        {
            throw new ForbiddenException();
        }

        var message = new InquiryMessage
        {
            InquiryId = inquiryId,
            SenderUserId = doctorUserId,
            SenderName = $"Dr. {doctor.FullName}",
            SenderRole = "Doctor",
            Body = request.Body.Trim()
        };

        await _unitOfWork.InquiryMessages.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (inquiry.Patient?.Email != null)
        {
            await _notifications.SendNewMessageNotificationAsync(inquiry, message, inquiry.Patient.Email, cancellationToken);
        }

        return new InquiryMessageDto
        {
            Id = message.Id,
            SenderUserId = doctorUserId,
            SenderName = message.SenderName,
            SenderRole = message.SenderRole,
            Body = message.Body,
            CreatedAtUtc = message.CreatedAtUtc,
            IsMine = true
        };
    }

    private async Task<Doctor> GetDoctorByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var doctor = await _unitOfWork.Doctors.FindAsync(d => d.UserId == userId && d.IsActive, cancellationToken);
        if (doctor == null)
        {
            throw new ForbiddenException("Bu istifadəçi üçün həkim profili tapılmadı.");
        }
        return doctor;
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
            Symptoms = inquiry.Symptoms,
            Description = inquiry.Description,
            PreferredTimeframe = inquiry.PreferredTimeframe,
            NeedsAccommodation = inquiry.NeedsAccommodation,
            NeedsAirportTransfer = inquiry.NeedsAirportTransfer,
            NeedsAirplaneTicket = inquiry.NeedsAirplaneTicket,
            IsEmergency = inquiry.IsEmergency,
            Status = inquiry.Status,
            StatusText = inquiry.Status.ToString(),
            CreatedAtUtc = inquiry.CreatedAtUtc,
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
            Offers = inquiry.Offers.Select(MapOfferToDto).ToList()
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
            StatusText = o.Status.ToString(),
            IsExpired = o.IsExpired,
            CreatedAtUtc = o.CreatedAtUtc
        };
    }
}

