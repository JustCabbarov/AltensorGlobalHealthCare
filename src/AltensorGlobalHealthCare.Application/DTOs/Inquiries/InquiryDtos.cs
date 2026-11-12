using AltensorGlobalHealthCare.Application.DTOs.Offers;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.DTOs.Inquiries;

public class CreateInquiryRequestDto
{
    public Guid? MedicalServiceId { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreferredTimeframe { get; set; }
    public bool NeedsAccommodation { get; set; }
    public bool NeedsAirportTransfer { get; set; }
    public bool NeedsAirplaneTicket { get; set; }
    public bool IsEmergency { get; set; }
}

public class InquirySummaryDto
{
    public Guid Id { get; set; }
    public string InquiryNumber { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string? ServiceTitle { get; set; }
    public InquiryStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public int DocumentsCount { get; set; }
    public int OffersCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class InquiryDetailDto
{
    public Guid Id { get; set; }
    public string InquiryNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string? PatientPhone { get; set; }
    public string? PatientCountry { get; set; }

    public Guid? MedicalServiceId { get; set; }
    public string? ServiceTitle { get; set; }
    public string? CategoryTitle { get; set; }

    public Guid? AssignedDoctorId { get; set; }
    public string? AssignedDoctorName { get; set; }
    public string? AssignedClinicName { get; set; }

    public string Symptoms { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreferredTimeframe { get; set; }

    public bool NeedsAccommodation { get; set; }
    public bool NeedsAirportTransfer { get; set; }
    public bool NeedsAirplaneTicket { get; set; }
    public bool IsEmergency { get; set; }

    public InquiryStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public IList<InquiryDocumentDto> Documents { get; set; } = new List<InquiryDocumentDto>();
    public IList<InquiryMessageDto> Messages { get; set; } = new List<InquiryMessageDto>();
    public IList<OfferDto> Offers { get; set; } = new List<OfferDto>();
}

public class InquiryDocumentDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class InquiryMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead => ReadAtUtc != null;
    public DateTime? ReadAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsMine { get; set; }
}

public class SendMessageRequestDto
{
    public string Body { get; set; } = string.Empty;
}

public class AssignDoctorRequestDto
{
    public Guid DoctorId { get; set; }
}

public class UpdateInquiryStatusRequestDto
{
    public InquiryStatus Status { get; set; }
}

public class UpdateInquiryProcedureRequestDto
{
    public Guid MedicalServiceId { get; set; }
}

