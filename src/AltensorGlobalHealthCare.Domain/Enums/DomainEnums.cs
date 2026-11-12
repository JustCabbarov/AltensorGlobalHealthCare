namespace AltensorGlobalHealthCare.Domain.Enums;

public enum InquiryStatus
{
    Submitted = 1,
    InReview = 2,
    Processing = 3,
    OfferReceived = 4,
    Accepted = 5,
    Declined = 6,
    Closed = 7,
    Cancelled = 8
}

public enum OfferStatus
{
    Draft = 1,
    Sent = 2,
    Accepted = 3,
    Declined = 4,
    Expired = 5,
    Withdrawn = 6
}

public enum VerificationCodePurpose
{
    ConfirmEmail = 1,
    ResetPassword = 2
}

public enum ReviewSource
{
    Internal = 1,
    Google = 2
}

public enum NotificationChannel
{
    Email = 1,
    Telegram = 2
}

public enum PartnerType
{
    Institutional = 1,
    Airline = 2,
    Accommodation = 3,
    Editorial = 4
}

