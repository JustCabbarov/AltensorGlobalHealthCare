using AltensorGlobalHealthCare.Application.Common;

namespace AltensorGlobalHealthCare.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public IReadOnlyList<ErrorDetail>? Errors { get; }

    protected AppException(
        string message,
        int statusCode,
        string errorCode,
        IReadOnlyList<ErrorDetail>? errors = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
    }
}

public class BadRequestException : AppException
{
    public BadRequestException(string message, string errorCode = "BAD_REQUEST", IReadOnlyList<ErrorDetail>? errors = null)
        : base(message, 400, errorCode, errors) { }
}

public class ValidationException : AppException
{
    public ValidationException(IReadOnlyList<ErrorDetail> errors, string message = "Daxil edilən məlumatlarda səhvlər var.")
        : base(message, 400, "VALIDATION_ERROR", errors) { }

    public ValidationException(string field, string message)
        : base("Daxil edilən məlumatlarda səhvlər var.", 400, "VALIDATION_ERROR", new[] { new ErrorDetail(field, message) }) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "İcazə verilmədi.", string errorCode = "UNAUTHORIZED")
        : base(message, 401, errorCode) { }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Bu əməliyyatı icra etmək üçün səlahiyyətiniz çatmır.", string errorCode = "FORBIDDEN")
        : base(message, 403, errorCode) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message, string errorCode = "NOT_FOUND")
        : base(message, 404, errorCode) { }

    public static NotFoundException For<T>(object id) =>
        new($"{typeof(T).Name} tapılmadı (ID: {id}).", $"{typeof(T).Name.ToUpperInvariant()}_NOT_FOUND");
}

public class ConflictException : AppException
{
    public ConflictException(string message, string errorCode = "CONFLICT")
        : base(message, 409, errorCode) { }
}

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message, 422, errorCode) { }
}

public class TooManyRequestsException : AppException
{
    public TooManyRequestsException(string message = "Həddindən artıq çox sorğu göndərildi. Zəhmət olmasa bir qədər sonra yenidən cəhd edin.", string errorCode = "TOO_MANY_REQUESTS")
        : base(message, 429, errorCode) { }
}

public class FileValidationException : AppException
{
    public FileValidationException(string message, string errorCode = "INVALID_FILE")
        : base(message, 400, errorCode) { }
}

