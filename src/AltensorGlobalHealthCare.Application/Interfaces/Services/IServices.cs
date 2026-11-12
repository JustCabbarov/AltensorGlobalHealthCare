using System.Security.Claims;
using AltensorGlobalHealthCare.Application.DTOs.Auth;
using AltensorGlobalHealthCare.Domain.Entities;

namespace AltensorGlobalHealthCare.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

public interface IJwtTokenService
{
    Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> GenerateTokensAsync(
        ApplicationUser user, IList<string> roles);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> ConfirmEmailAsync(ConfirmEmailRequestDto request, CancellationToken cancellationToken = default);
    Task<string> ResendVerificationCodeAsync(ResendCodeRequestDto request, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<string> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task<CurrentUserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request, CancellationToken cancellationToken = default);
}

public interface IFileStorageService
{
    Task<string> SavePublicFileAsync(Stream fileStream, string fileName, string subFolder, CancellationToken cancellationToken = default);
    Task<string> SaveSecureMedicalDocumentAsync(Stream fileStream, string fileName, Guid inquiryId, CancellationToken cancellationToken = default);
    (Stream FileStream, string ContentType, string FileName)? GetSecureMedicalDocument(string relativePath);
    void DeleteFile(string? relativePath);
}

public interface INotificationDispatcher
{
    Task SendVerificationCodeEmailAsync(string email, string name, string code, CancellationToken cancellationToken = default);
    Task SendPasswordResetCodeEmailAsync(string email, string name, string code, CancellationToken cancellationToken = default);
    Task SendInquiryNotificationToCoordinatorAsync(Inquiry inquiry, CancellationToken cancellationToken = default);
    Task SendOfferNotificationToPatientAsync(Offer offer, CancellationToken cancellationToken = default);
    Task SendNewMessageNotificationAsync(Inquiry inquiry, InquiryMessage message, string recipientEmail, CancellationToken cancellationToken = default);
    Task<(bool Success, string BotUsername, string ChatId, string Message)> ConnectAndVerifyTelegramBotAsync(string botToken, string? manualChatId, CancellationToken cancellationToken = default);
}

