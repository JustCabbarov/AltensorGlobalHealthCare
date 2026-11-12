using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AltensorGlobalHealthCare.Application.Common;
using AltensorGlobalHealthCare.Application.DTOs.Auth;
using AltensorGlobalHealthCare.Application.Interfaces.Services;

namespace AltensorGlobalHealthCare.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Yeni pasient qeydiyyatı (6 rəqəmli təsdiq kodu emailə göndərilir)
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result, "Qeydiyyat uğurla tamamlandı."));
    }

    /// <summary>
    /// Emailə göndərilən 6 rəqəmli kodun təsdiqi və ilk giriş
    /// </summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> ConfirmEmail([FromBody] ConfirmEmailRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.ConfirmEmailAsync(request, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Hesabınız təsdiqləndi."));
    }

    /// <summary>
    /// Təsdiq kodunun yenidən göndərilməsi (60 saniyəlik cooldown ilə)
    /// </summary>
    [HttpPost("resend-code")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ResendCode([FromBody] ResendCodeRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResendVerificationCodeAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Sistemə giriş (Bütün rollar: Admin, Coordinator, Doctor, Patient)
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Uğurla daxil oldunuz."));
    }

    /// <summary>
    /// Access token-in müddəti bitdikdə Refresh token vasitəsilə yeni token əldə etmək
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Token yeniləndi."));
    }

    /// <summary>
    /// Sistemdən çıxış (Aktiv sessiya və refresh token ləğv edilir)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> Logout(CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId.HasValue)
        {
            await _authService.LogoutAsync(_currentUserService.UserId.Value, cancellationToken);
        }
        return Ok(ApiResponse<string>.Ok("Uğurla çıxış edildi."));
    }

    /// <summary>
    /// Şifrənin unudulması halında emailə 6 rəqəmli sıfırlama kodunun göndərilməsi
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// 6 rəqəmli kod və yeni şifrə vasitəsilə şifrənin yenilənməsi
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok(result));
    }

    /// <summary>
    /// Cari daxil olmuş istifadəçi və rolları haqqında məlumat
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _authService.GetCurrentUserAsync(User, cancellationToken);
        return Ok(ApiResponse<CurrentUserDto>.Ok(result));
    }
}

