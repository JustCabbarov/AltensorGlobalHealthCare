using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.Common.Exceptions;
using AltensorGlobalHealthCare.Application.DTOs.Auth;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Application.Validators;
using AltensorGlobalHealthCare.Domain.Entities;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notificationDispatcher;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        INotificationDispatcher notificationDispatcher)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task<string> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new ConflictException("Bu e-poçt ünvanı ilə artıq qeydiyyatdan keçilib.", "EMAIL_ALREADY_EXISTS");
        }

        var normalizedDob = NormalizeUtc(request.DateOfBirth);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Country = request.Country?.Trim(),
            DateOfBirth = normalizedDob,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => new Common.ErrorDetail(null, e.Description, e.Code)).ToList();
            throw new BadRequestException("İstifadəçi yaradıla bilmədi.", "USER_CREATION_FAILED", errors);
        }

        // Add Patient role
        await _userManager.AddToRoleAsync(user, "Patient");

        // Create PatientProfile
        var profile = new PatientProfile
        {
            UserId = user.Id,
            Country = request.Country?.Trim(),
            DateOfBirth = normalizedDob
        };
        await _unitOfWork.PatientProfiles.AddAsync(profile, cancellationToken);

        // Generate 6-digit confirmation code
        var code = GenerateSixDigitCode();
        var verification = new EmailVerificationCode
        {
            UserId = user.Id,
            CodeHash = HashCode(code),
            Purpose = VerificationCodePurpose.ConfirmEmail,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
        };
        await _unitOfWork.VerificationCodes.AddAsync(verification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email with code
        await _notificationDispatcher.SendVerificationCodeEmailAsync(user.Email, user.FullName, code, cancellationToken);

        return "Qeydiyyat uğurla tamamlandı. Zəhmət olmasa e-poçt ünvanınıza göndərilən 6 rəqəmli təsdiq kodunu daxil edin.";
    }

    public async Task<LoginResponseDto> ConfirmEmailAsync(ConfirmEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new NotFoundException("Bu e-poçt ünvanı ilə istifadəçi tapılmadı.", "USER_NOT_FOUND");
        }

        if (user.EmailConfirmed)
        {
            throw new BadRequestException("Bu hesab artıq təsdiqlənib. Daxil ola bilərsiniz.", "ALREADY_CONFIRMED");
        }

        var codeHash = HashCode(request.Code.Trim());
        var codes = await _unitOfWork.VerificationCodes.GetListAsync(
            c => c.UserId == user.Id && c.Purpose == VerificationCodePurpose.ConfirmEmail && c.UsedAtUtc == null,
            cancellationToken);

        var activeCode = codes.OrderByDescending(c => c.CreatedAtUtc).FirstOrDefault();

        if (activeCode == null || activeCode.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new BusinessRuleException("Təsdiq kodunun vaxtı bitib və ya kod mövcud deyil. Yenidən kod istəyin.", "CODE_EXPIRED");
        }

        if (activeCode.AttemptCount >= 5)
        {
            throw new TooManyRequestsException("Təsdiq cəhdlərinin sayı həddindən artıqdır. Yeni təsdiq kodu tələb edin.");
        }

        if (activeCode.CodeHash != codeHash)
        {
            activeCode.AttemptCount++;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new BadRequestException("Daxil edilən təsdiq kodu yanlışdır.", "INVALID_CODE");
        }

        activeCode.UsedAtUtc = DateTime.UtcNow;
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, refreshToken, expiresAt) = await _jwtTokenService.GenerateTokensAsync(user, roles);

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = await BuildCurrentUserDtoAsync(user, roles, cancellationToken)
        };
    }

    public async Task<string> ResendVerificationCodeAsync(ResendCodeRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.EmailConfirmed)
        {
            return "Əgər e-poçt ünvanı mövcuddursa və təsdiqlənməyibsə, yeni təsdiq kodu göndərildi.";
        }

        var codes = await _unitOfWork.VerificationCodes.GetListAsync(
            c => c.UserId == user.Id && c.Purpose == VerificationCodePurpose.ConfirmEmail,
            cancellationToken);

        var lastCode = codes.OrderByDescending(c => c.CreatedAtUtc).FirstOrDefault();

        if (lastCode != null && lastCode.CreatedAtUtc.AddSeconds(60) > DateTime.UtcNow)
        {
            throw new TooManyRequestsException("Yeni kod tələb etmək üçün 60 saniyə gözləməlisiniz.");
        }

        var code = GenerateSixDigitCode();
        var verification = new EmailVerificationCode
        {
            UserId = user.Id,
            CodeHash = HashCode(code),
            Purpose = VerificationCodePurpose.ConfirmEmail,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
        };
        await _unitOfWork.VerificationCodes.AddAsync(verification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationDispatcher.SendVerificationCodeEmailAsync(user.Email!, user.FullName, code, cancellationToken);

        return "Yeni təsdiq kodu e-poçt ünvanınıza göndərildi.";
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedException("E-poçt ünvanı və ya şifrə yanlışdır.", "INVALID_CREDENTIALS");
        }

        if (!user.EmailConfirmed)
        {
            throw new BusinessRuleException("E-poçt ünvanınız təsdiqlənməyib. Zəhmət olmasa təsdiq kodunu daxil edin.", "EMAIL_NOT_CONFIRMED");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, refreshToken, expiresAt) = await _jwtTokenService.GenerateTokensAsync(user, roles);

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = await BuildCurrentUserDtoAsync(user, roles, cancellationToken)
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new UnauthorizedException("Etibarsız giriş tokeni.", "INVALID_ACCESS_TOKEN");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Token daxilində istifadəçi tapılmadı.", "INVALID_USER_CLAIM");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new UnauthorizedException("İstifadəçi mövcud deyil.", "USER_NOT_FOUND");
        }

        var existingRefreshToken = await _unitOfWork.RefreshTokens.FindAsync(
            r => r.UserId == userId && r.Token == request.RefreshToken && !r.IsRevoked, cancellationToken);

        if (existingRefreshToken == null || !existingRefreshToken.IsActive)
        {
            throw new UnauthorizedException("Yeniləmə tokeni etibarsızdır və ya vaxtı bitib.", "INVALID_REFRESH_TOKEN");
        }

        // Revoke current refresh token
        existingRefreshToken.IsRevoked = true;

        var roles = await _userManager.GetRolesAsync(user);
        var (newAccessToken, newRefreshToken, expiresAt) = await _jwtTokenService.GenerateTokensAsync(user, roles);

        existingRefreshToken.ReplacedByToken = newRefreshToken;

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            User = await BuildCurrentUserDtoAsync(user, roles, cancellationToken)
        };
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _unitOfWork.RefreshTokens.GetListAsync(
            r => r.UserId == userId && !r.IsRevoked, cancellationToken);

        foreach (var t in activeTokens)
        {
            t.IsRevoked = true;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Do not reveal email existence
            return "Əgər daxil edilən e-poçt ünvanı sistemdə mövcuddursa, şifrə bərpa kodu göndərildi.";
        }

        var code = GenerateSixDigitCode();
        var verification = new EmailVerificationCode
        {
            UserId = user.Id,
            CodeHash = HashCode(code),
            Purpose = VerificationCodePurpose.ResetPassword,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
        };
        await _unitOfWork.VerificationCodes.AddAsync(verification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationDispatcher.SendPasswordResetCodeEmailAsync(user.Email!, user.FullName, code, cancellationToken);

        return "Şifrə sıfırlama kodu e-poçt ünvanınıza göndərildi.";
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new NotFoundException("İstifadəçi tapılmadı.", "USER_NOT_FOUND");
        }

        var codeHash = HashCode(request.Code.Trim());
        var codes = await _unitOfWork.VerificationCodes.GetListAsync(
            c => c.UserId == user.Id && c.Purpose == VerificationCodePurpose.ResetPassword && c.UsedAtUtc == null,
            cancellationToken);

        var activeCode = codes.OrderByDescending(c => c.CreatedAtUtc).FirstOrDefault();

        if (activeCode == null || activeCode.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new BusinessRuleException("Şifrə sıfırlama kodunun vaxtı bitib. Zəhmət olmasa yenidən cəhd edin.", "CODE_EXPIRED");
        }

        if (activeCode.AttemptCount >= 5)
        {
            throw new TooManyRequestsException("Cəhdlərin sayı limiti aşdı. Yeni kod istəyin.");
        }

        if (activeCode.CodeHash != codeHash)
        {
            activeCode.AttemptCount++;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new BadRequestException("Daxil edilən kod yanlışdır.", "INVALID_CODE");
        }

        activeCode.UsedAtUtc = DateTime.UtcNow;

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new Common.ErrorDetail(null, e.Description, e.Code)).ToList();
            throw new BadRequestException("Şifrə yenilənə bilmədi.", "PASSWORD_RESET_FAILED", errors);
        }

        // Revoke active sessions
        var tokens = await _unitOfWork.RefreshTokens.GetListAsync(r => r.UserId == user.Id && !r.IsRevoked, cancellationToken);
        foreach (var t in tokens) t.IsRevoked = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return "Şifrəniz uğurla yeniləndi. Yeni şifrə ilə daxil ola bilərsiniz.";
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("İstifadəçi tapılmadı.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return await BuildCurrentUserDtoAsync(user, roles, cancellationToken);
    }

    public async Task<CurrentUserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        var validator = new UpdateProfileRequestDtoValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new ErrorDetail(e.PropertyName, e.ErrorMessage, e.ErrorCode))
                .ToList();
            throw new ValidationException(errors);
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new NotFoundException("İstifadəçi tapılmadı.", "USER_NOT_FOUND");
        }

        if (request.FirstName != null)
            user.FirstName = request.FirstName.Trim();

        if (request.LastName != null)
            user.LastName = request.LastName.Trim();

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber.Trim();

        string? normalizedCountry = null;
        if (request.Country != null)
        {
            normalizedCountry = string.IsNullOrWhiteSpace(request.Country) ? null : request.Country.Trim();
            user.Country = normalizedCountry;
        }

        var normalizedDob = NormalizeUtc(request.DateOfBirth);
        if (request.DateOfBirth.HasValue)
        {
            user.DateOfBirth = normalizedDob;
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredLanguage))
        {
            user.PreferredLanguage = request.PreferredLanguage.Trim().ToLowerInvariant();
        }

        user.UpdatedAtUtc = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => new ErrorDetail(null, e.Description, e.Code)).ToList();
            throw new BadRequestException("Profil yenilənə bilmədi.", "PROFILE_UPDATE_FAILED", errors);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var profile = await _unitOfWork.PatientProfiles.FindAsync(p => p.UserId == user.Id, cancellationToken);

        if (profile == null && (roles.Contains("Patient") || request.MedicalNotes != null))
        {
            profile = new PatientProfile
            {
                UserId = user.Id,
                Country = user.Country,
                DateOfBirth = user.DateOfBirth,
                MedicalNotes = string.IsNullOrWhiteSpace(request.MedicalNotes) ? null : request.MedicalNotes.Trim()
            };
            await _unitOfWork.PatientProfiles.AddAsync(profile, cancellationToken);
        }
        else if (profile != null)
        {
            if (request.Country != null)
                profile.Country = normalizedCountry;

            if (request.DateOfBirth.HasValue)
                profile.DateOfBirth = normalizedDob;

            if (request.MedicalNotes != null)
                profile.MedicalNotes = string.IsNullOrWhiteSpace(request.MedicalNotes) ? null : request.MedicalNotes.Trim();

            profile.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await BuildCurrentUserDtoAsync(user, roles, cancellationToken);
    }

    private async Task<CurrentUserDto> BuildCurrentUserDtoAsync(
        ApplicationUser user,
        IList<string> roles,
        CancellationToken cancellationToken)
    {
        var doctor = await _unitOfWork.Doctors.FindAsync(d => d.UserId == user.Id, cancellationToken);
        var patientProfile = await _unitOfWork.PatientProfiles.FindAsync(p => p.UserId == user.Id, cancellationToken);

        return MapToCurrentUserDto(user, roles, doctor?.ClinicId, patientProfile?.MedicalNotes);
    }

    private static DateTime? NormalizeUtc(DateTime? value)
    {
        if (!value.HasValue) return null;
        return value.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : value.Value.ToUniversalTime();
    }

    private static CurrentUserDto MapToCurrentUserDto(
        ApplicationUser user,
        IList<string> roles,
        Guid? clinicId,
        string? medicalNotes)
    {
        return new CurrentUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Country = user.Country,
            DateOfBirth = user.DateOfBirth,
            MedicalNotes = medicalNotes,
            PreferredLanguage = user.PreferredLanguage,
            Roles = roles,
            ClinicId = clinicId
        };
    }

    private static string GenerateSixDigitCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(4);
        var val = BitConverter.ToUInt32(bytes, 0) % 1000000;
        return val.ToString("D6");
    }

    private static string HashCode(string code)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }
}
