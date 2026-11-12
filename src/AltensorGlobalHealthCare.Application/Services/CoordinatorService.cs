using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.DTOs.Admin;
using AltensorGlobalHealthCare.Application.DTOs.Inquiries;
using AltensorGlobalHealthCare.Application.DTOs.Offers;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.Services;

public class CoordinatorService : ICoordinatorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly INotificationDispatcher _notifications;

    public CoordinatorService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        INotificationDispatcher notifications)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _notifications = notifications;
    }

    public async Task<PagedResult<InquirySummaryDto>> GetInquiriesAsync(
        InquiryStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 15;

        var term = search?.Trim().ToLowerInvariant();

        var all = await _unitOfWork.Inquiries.GetListAsync(
            i => (!status.HasValue || i.Status == status) &&
                 (string.IsNullOrEmpty(term) ||
                  i.InquiryNumber.ToLower().Contains(term) ||
                  i.Patient.FirstName.ToLower().Contains(term) ||
                  i.Patient.LastName.ToLower().Contains(term) ||
                  i.Symptoms.ToLower().Contains(term)),
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
                StatusText = GetStatusText(i.Status),
                DocumentsCount = i.Documents.Count,
                OffersCount = i.Offers.Count,
                CreatedAtUtc = i.CreatedAtUtc
            }).ToList();

        return new PagedResult<InquirySummaryDto>(paged, total, page, pageSize);
    }

    public async Task<InquiryDetailDto> GetInquiryByIdAsync(Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetWithDetailsAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);

        return MapToDetailDto(inquiry);
    }

    public async Task<string> AssignDoctorAsync(Guid inquiryId, Guid doctorId, Guid coordinatorId, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);

        var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId, cancellationToken);
        if (doctor == null) throw NotFoundException.For<Doctor>(doctorId);

        inquiry.AssignedDoctorId = doctorId;
        inquiry.AssignedCoordinatorId = coordinatorId;
        if (inquiry.Status == InquiryStatus.Submitted || inquiry.Status == InquiryStatus.InReview)
        {
            inquiry.ChangeStatus(InquiryStatus.Processing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return $"Müraciət uğurla Dr. {doctor.FullName} həkimə təyin edildi.";
    }

    public async Task<string> UpdateStatusAsync(Guid inquiryId, InquiryStatus status, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);

        inquiry.ChangeStatus(status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return $"Status '{GetStatusText(status)}' olaraq yeniləndi.";
    }

    public async Task<OfferDto> CreateOfferAsync(CreateOfferRequestDto request, IList<FileUploadDto>? attachments, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetByIdAsync(request.InquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(request.InquiryId);

        var clinic = await _unitOfWork.Clinics.GetByIdAsync(request.ClinicId, cancellationToken);
        if (clinic == null) throw NotFoundException.For<Clinic>(request.ClinicId);

        var offer = new Offer
        {
            InquiryId = request.InquiryId,
            ClinicId = request.ClinicId,
            DoctorId = request.DoctorId,
            Title = request.Title.Trim(),
            Text = request.Text?.Trim(),
            MedicalTreatmentPrice = request.MedicalTreatmentPrice,
            AccommodationPrice = request.AccommodationPrice,
            AirportTransferPrice = request.AirportTransferPrice,
            AirplaneTicketPrice = request.AirplaneTicketPrice,
            Currency = request.Currency,
            ProcedureDate = request.ProcedureDate,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(request.ExpiresInDays > 0 ? request.ExpiresInDays : 14),
            Status = OfferStatus.Draft
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

    public async Task<string> SendOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await _unitOfWork.Offers.GetWithDetailsAsync(offerId, cancellationToken);
        if (offer == null) throw NotFoundException.For<Offer>(offerId);

        if (offer.Status != OfferStatus.Draft)
        {
            throw new BusinessRuleException("Yalnız qaralama statusunda olan təklif göndərilə bilər.", "INVALID_OFFER_STATUS");
        }

        offer.Status = OfferStatus.Sent;
        if (offer.Inquiry.Status == InquiryStatus.Submitted ||
            offer.Inquiry.Status == InquiryStatus.InReview ||
            offer.Inquiry.Status == InquiryStatus.Processing)
        {
            offer.Inquiry.ChangeStatus(InquiryStatus.OfferReceived);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify patient
        await _notifications.SendOfferNotificationToPatientAsync(offer, cancellationToken);

        return "Təklif uğurla təsdiqləndi və pasientə göndərildi.";
    }

    public async Task<string> WithdrawOfferAsync(Guid offerId, CancellationToken cancellationToken = default)
    {
        var offer = await _unitOfWork.Offers.GetByIdAsync(offerId, cancellationToken);
        if (offer == null) throw NotFoundException.For<Offer>(offerId);

        offer.Status = OfferStatus.Withdrawn;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return "Təklif geri çağırıldı.";
    }

    public async Task<IReadOnlyList<InquiryMessageDto>> GetMessagesAsync(Guid inquiryId, CancellationToken cancellationToken = default)
    {
        var messages = await _unitOfWork.InquiryMessages.GetListAsync(
            m => m.InquiryId == inquiryId, cancellationToken);

        return messages.OrderBy(m => m.CreatedAtUtc).Select(m => new InquiryMessageDto
        {
            Id = m.Id,
            SenderUserId = m.SenderUserId,
            SenderName = m.SenderName,
            SenderRole = m.SenderRole,
            Body = m.Body,
            ReadAtUtc = m.ReadAtUtc,
            CreatedAtUtc = m.CreatedAtUtc,
            IsMine = m.SenderRole == "Coordinator"
        }).ToList();
    }

    public async Task<InquiryMessageDto> SendMessageAsync(Guid coordinatorId, Guid inquiryId, SendMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        var inquiry = await _unitOfWork.Inquiries.GetWithDetailsAsync(inquiryId, cancellationToken);
        if (inquiry == null) throw NotFoundException.For<Inquiry>(inquiryId);

        var message = new InquiryMessage
        {
            InquiryId = inquiryId,
            SenderUserId = coordinatorId,
            SenderName = "Ivana (Pasient Koordinatoru)",
            SenderRole = "Coordinator",
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
            SenderUserId = coordinatorId,
            SenderName = message.SenderName,
            SenderRole = message.SenderRole,
            Body = message.Body,
            CreatedAtUtc = message.CreatedAtUtc,
            IsMine = true
        };
    }

    public async Task<DashboardSummaryDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var allInquiries = await _unitOfWork.Inquiries.GetAllAsync(cancellationToken);
        var activeInquiries = allInquiries.Count(i => i.Status != InquiryStatus.Closed && i.Status != InquiryStatus.Cancelled);
        var patientsCount = await _unitOfWork.PatientProfiles.CountAsync(null, cancellationToken);
        var clinicsCount = await _unitOfWork.Clinics.CountAsync(c => c.IsActive, cancellationToken);
        var doctorsCount = await _unitOfWork.Doctors.CountAsync(d => d.IsActive, cancellationToken);
        var offers = await _unitOfWork.Offers.GetAllAsync(cancellationToken);

        var acceptedOffers = offers.Where(o => o.Status == OfferStatus.Accepted).ToList();
        var acceptedVolume = acceptedOffers.Sum(o => o.TotalPrice);

        return new DashboardSummaryDto
        {
            TotalInquiries = allInquiries.Count,
            ActiveInquiries = activeInquiries,
            TotalPatients = patientsCount,
            TotalClinics = clinicsCount,
            TotalDoctors = doctorsCount,
            TotalOffers = offers.Count,
            AcceptedOffers = acceptedOffers.Count,
            TotalAcceptedVolume = acceptedVolume,
            RecentInquiries = allInquiries.OrderByDescending(i => i.CreatedAtUtc).Take(5).Select(i => new InquirySummaryDto
            {
                Id = i.Id,
                InquiryNumber = i.InquiryNumber,
                Symptoms = i.Symptoms,
                ServiceTitle = i.MedicalService?.TitleAz,
                Status = i.Status,
                StatusText = GetStatusText(i.Status),
                CreatedAtUtc = i.CreatedAtUtc
            }).ToList()
        };
    }

    private static InquiryDetailDto MapToDetailDto(Inquiry inquiry)
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
                IsMine = m.SenderRole == "Coordinator"
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
            StatusText = o.Status.ToString(),
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
}

