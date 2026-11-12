using AltensorGlobalHealthCare.Domain.Common;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Domain.Entities;

public class Inquiry : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string InquiryNumber { get; set; } = string.Empty;

    public Guid PatientId { get; set; }
    public virtual ApplicationUser Patient { get; set; } = null!;

    public Guid? MedicalServiceId { get; set; }
    public virtual MedicalService? MedicalService { get; set; }

    public Guid? AssignedDoctorId { get; set; }
    public virtual Doctor? AssignedDoctor { get; set; }

    public Guid? AssignedCoordinatorId { get; set; }
    public virtual ApplicationUser? AssignedCoordinator { get; set; }

    public string Symptoms { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreferredTimeframe { get; set; }

    public bool NeedsAccommodation { get; set; }
    public bool NeedsAirportTransfer { get; set; }
    public bool NeedsAirplaneTicket { get; set; }
    public bool IsEmergency { get; set; }

    public InquiryStatus Status { get; set; } = InquiryStatus.Submitted;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public virtual ICollection<InquiryDocument> Documents { get; set; } = new List<InquiryDocument>();
    public virtual ICollection<InquiryMessage> Messages { get; set; } = new List<InquiryMessage>();
    public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>();

    public void ChangeStatus(InquiryStatus newStatus)
    {
        // Domain status transition logic
        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

public class InquiryDocument : BaseEntity, IAuditableEntity, ISoftDelete
{
    public Guid InquiryId { get; set; }
    public virtual Inquiry Inquiry { get; set; } = null!;

    public string OriginalFileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }

    public Guid UploadedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}

public class InquiryMessage : BaseEntity, IAuditableEntity
{
    public Guid InquiryId { get; set; }
    public virtual Inquiry Inquiry { get; set; } = null!;

    public Guid SenderUserId { get; set; }
    public virtual ApplicationUser SenderUser { get; set; } = null!;

    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty; // Patient, Coordinator, Doctor
    public string Body { get; set; } = string.Empty;

    public DateTime? ReadAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

