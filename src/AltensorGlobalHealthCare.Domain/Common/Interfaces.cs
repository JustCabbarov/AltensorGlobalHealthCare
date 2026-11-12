namespace AltensorGlobalHealthCare.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
}

