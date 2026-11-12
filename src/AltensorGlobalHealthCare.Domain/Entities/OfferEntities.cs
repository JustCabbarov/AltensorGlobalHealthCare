using AltensorGlobalHealthCare.Domain.Common;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Domain.Entities;

public class Offer : BaseEntity, IAuditableEntity, ISoftDelete
{
    public Guid InquiryId { get; set; }
    public virtual Inquiry Inquiry { get; set; } = null!;

    public Guid ClinicId { get; set; }
    public virtual Clinic Clinic { get; set; } = null!;

    public Guid? DoctorId { get; set; }
    public virtual Doctor? Doctor { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Text { get; set; }

    public decimal MedicalTreatmentPrice { get; set; }
    public decimal AccommodationPrice { get; set; }
    public decimal AirportTransferPrice { get; set; }
    public decimal AirplaneTicketPrice { get; set; }

    public decimal TotalPrice => MedicalTreatmentPrice + AccommodationPrice + AirportTransferPrice + AirplaneTicketPrice;

    public string Currency { get; set; } = "EUR";
    public DateTime? ProcedureDate { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    public OfferStatus Status { get; set; } = OfferStatus.Draft;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<OfferAttachment> Attachments { get; set; } = new List<OfferAttachment>();

    public bool IsExpired => Status == OfferStatus.Sent && DateTime.UtcNow > ExpiresAtUtc;
}

public class OfferAttachment : BaseEntity, IAuditableEntity
{
    public Guid OfferId { get; set; }
    public virtual Offer Offer { get; set; } = null!;

    public string OriginalFileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

