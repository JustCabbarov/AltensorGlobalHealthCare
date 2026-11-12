using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Common.Exceptions;

namespace AltensorGlobalHealthCare.WebApi.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("İstifadəçi sorğunu dayandırdı: {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Response.StatusCode = 499; // Client Closed Request
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Xəta baş verdi, lakin HTTP cavab artıq göndərilməyə başladığı üçün middleware cavabı dəyişdirə bilmədi: {Message}", exception.Message);
            return;
        }

        context.Response.ContentType = "application/json; charset=utf-8";

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        int statusCode;
        string errorCode;
        string message;
        IReadOnlyList<ErrorDetail>? errors = null;

        switch (exception)
        {
            case AppException appEx:
                statusCode = appEx.StatusCode;
                errorCode = appEx.ErrorCode;
                message = appEx.Message;
                errors = appEx.Errors;
                _logger.LogWarning("Tətbiq xətası [{StatusCode} - {ErrorCode}]: {Message} (Path: {Path}, TraceId: {TraceId})",
                    statusCode, errorCode, message, context.Request.Path, traceId);
                break;

            case FluentValidation.ValidationException fvEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorCode = "VALIDATION_ERROR";
                message = "Daxil edilən məlumatlarda səhvlər var.";
                errors = fvEx.Errors
                    .Select(e => new ErrorDetail(e.PropertyName, e.ErrorMessage, e.ErrorCode))
                    .ToList();
                _logger.LogWarning("Validasiya xətası ({Count} səhv) (Path: {Path}, TraceId: {TraceId})",
                    errors.Count, context.Request.Path, traceId);
                break;

            case DbUpdateConcurrencyException:
                statusCode = (int)HttpStatusCode.Conflict;
                errorCode = "CONCURRENCY_CONFLICT";
                message = "Məlumat başqa bir əməliyyat tərəfindən dəyişdirilib. Zəhmət olmasa yeniləyib təkrar cəhd edin.";
                _logger.LogWarning("Konkurans xətası (Path: {Path}, TraceId: {TraceId})", context.Request.Path, traceId);
                break;

            case DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx:
                statusCode = (int)HttpStatusCode.Conflict;
                if (pgEx.SqlState == PostgresErrorCodes.UniqueViolation) // 23505
                {
                    errorCode = "DUPLICATE_RECORD";
                    message = "Bu unikal məlumat (məsələn, e-poçt və ya unikal kod) artıq sistemdə mövcuddur.";
                }
                else if (pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation) // 23503
                {
                    errorCode = "FOREIGN_KEY_CONFLICT";
                    message = "Əlaqəli məlumatlar mövcud olduğuna görə bu əməliyyatı tamamlamaq mümkün olmadı.";
                }
                else
                {
                    errorCode = "DATABASE_ERROR";
                    message = "Verilənlər bazasında əməliyyat xətası baş verdi.";
                }
                _logger.LogError(dbEx, "Postgres xətası [{SqlState}]: {Message} (Path: {Path}, TraceId: {TraceId})",
                    pgEx.SqlState, pgEx.MessageText, context.Request.Path, traceId);
                break;

            case BadHttpRequestException badHttpEx:
                statusCode = badHttpEx.StatusCode;
                errorCode = statusCode == 413 ? "PAYLOAD_TOO_LARGE" : "BAD_HTTP_REQUEST";
                message = statusCode == 413
                    ? "Yüklənən fayl və ya sorğu həcmi icazə verilən həddi aşır."
                    : "Yanlış HTTP sorğusu formalaşdırılıb.";
                _logger.LogWarning("BadHttpRequest [{StatusCode}]: {Message} (TraceId: {TraceId})", statusCode, badHttpEx.Message, traceId);
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                errorCode = "INTERNAL_SERVER_ERROR";
                message = _env.IsDevelopment()
                    ? $"[DEV DEBUG] {exception.GetType().Name}: {exception.Message}"
                    : "Gözlənilməz daxili server xətası baş verdi. Texniki dəstək ilə əlaqə saxlayın.";

                _logger.LogError(exception, "Kritik server xətası: {Message} (Path: {Path}, Method: {Method}, TraceId: {TraceId})",
                    exception.Message, context.Request.Path, context.Request.Method, traceId);
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(message, errorCode, errors, traceId);
        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}

