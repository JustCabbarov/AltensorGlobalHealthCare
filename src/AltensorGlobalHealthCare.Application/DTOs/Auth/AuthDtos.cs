namespace AltensorGlobalHealthCare.Application.DTOs.Auth;

public class RegisterRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool AcceptTerms { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class ConfirmEmailRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ResendCodeRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public CurrentUserDto User { get; set; } = null!;
}

public class RefreshTokenRequestDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MedicalNotes { get; set; }
    public string PreferredLanguage { get; set; } = "az";
    public IList<string> Roles { get; set; } = new List<string>();
    public Guid? ClinicId { get; set; }
}

public class UpdateProfileRequestDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? MedicalNotes { get; set; }
    public string? PreferredLanguage { get; set; }
}

