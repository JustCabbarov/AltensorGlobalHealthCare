using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.DTOs.Offers;

public class CreateOfferRequestDto
{
    public Guid InquiryId { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? DoctorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Text { get; set; }
    public decimal MedicalTreatmentPrice { get; set; }
    public decimal AccommodationPrice { get; set; }
    public decimal AirportTransferPrice { get; set; }
    public decimal AirplaneTicketPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime? ProcedureDate { get; set; }
    public int ExpiresInDays { get; set; } = 14;
}

public class UpdateOfferRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Text { get; set; }
    public decimal MedicalTreatmentPrice { get; set; }
    public decimal AccommodationPrice { get; set; }
    public decimal AirportTransferPrice { get; set; }
    public decimal AirplaneTicketPrice { get; set; }
    public string Currency { get; set; } = "EUR";
    public DateTime? ProcedureDate { get; set; }
}

public class OfferDto
{
    public Guid Id { get; set; }
    public Guid InquiryId { get; set; }
    public string InquiryNumber { get; set; } = string.Empty;

    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string? ClinicLogoUrl { get; set; }

    public Guid? DoctorId { get; set; }
    public string? DoctorName { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Text { get; set; }

    public decimal MedicalTreatmentPrice { get; set; }
    public decimal AccommodationPrice { get; set; }
    public decimal AirportTransferPrice { get; set; }
    public decimal AirplaneTicketPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public string Currency { get; set; } = "EUR";
    public DateTime? ProcedureDate { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public OfferStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public bool IsExpired { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public IList<OfferAttachmentDto> Attachments { get; set; } = new List<OfferAttachmentDto>();
}

public class OfferAttachmentDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}

