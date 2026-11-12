using FluentValidation;
using AltensorGlobalHealthCare.Application.DTOs.Auth;

namespace AltensorGlobalHealthCare.Application.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad mütləq daxil edilməlidir.")
            .MinimumLength(2).WithMessage("Ad ən azı 2 simvol olmalıdır.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad mütləq daxil edilməlidir.")
            .MinimumLength(2).WithMessage("Soyad ən azı 2 simvol olmalıdır.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Əlaqə nömrəsi mütləq daxil edilməlidir.")
            .MinimumLength(8).WithMessage("Telefon nömrəsi ən azı 8 rəqəm olmalıdır.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-poçt ünvanı mütləq daxil edilməlidir.")
            .EmailAddress().WithMessage("Düzgün bir e-poçt ünvanı daxil edin.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifrə mütləq daxil edilməlidir.")
            .MinimumLength(8).WithMessage("Şifrə ən azı 8 simvol olmalıdır.")
            .Matches(@"[A-Za-z]").WithMessage("Şifrədə ən azı bir hərf olmalıdır.")
            .Matches(@"[0-9]").WithMessage("Şifrədə ən azı bir rəqəm olmalıdır.");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("Qaydalar və şərtlərlə razılaşmalısınız.");
    }
}

public class ConfirmEmailRequestDtoValidator : AbstractValidator<ConfirmEmailRequestDto>
{
    public ConfirmEmailRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-poçt ünvanı mütləq daxil edilməlidir.")
            .EmailAddress().WithMessage("Düzgün bir e-poçt ünvanı daxil edin.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Təsdiq kodu mütləq daxil edilməlidir.")
            .Length(6).WithMessage("Təsdiq kodu 6 rəqəmli olmalıdır.");
    }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-poçt ünvanı daxil edilməlidir.")
            .EmailAddress().WithMessage("Düzgün e-poçt daxil edin.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifrə daxil edilməlidir.");
    }
}

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-poçt ünvanı daxil edilməlidir.")
            .EmailAddress().WithMessage("Düzgün e-poçt daxil edin.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Təsdiq kodu daxil edilməlidir.")
            .Length(6).WithMessage("Təsdiq kodu 6 rəqəmli olmalıdır.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifrə daxil edilməlidir.")
            .MinimumLength(8).WithMessage("Şifrə ən azı 8 simvol olmalıdır.")
            .Matches(@"[A-Za-z]").WithMessage("Şifrədə ən azı bir hərf olmalıdır.")
            .Matches(@"[0-9]").WithMessage("Şifrədə ən azı bir rəqəm olmalıdır.");
    }
}

public class UpdateProfileRequestDtoValidator : AbstractValidator<UpdateProfileRequestDto>
{
    private static readonly string[] AllowedLanguages = ["az", "en", "ru"];

    public UpdateProfileRequestDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MinimumLength(2).WithMessage("Ad ən azı 2 simvol olmalıdır.")
            .MaximumLength(100).WithMessage("Ad ən çox 100 simvol ola bilər.")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad boş ola bilməz.")
            .MinimumLength(2).WithMessage("Soyad ən azı 2 simvol olmalıdır.")
            .MaximumLength(100).WithMessage("Soyad ən çox 100 simvol ola bilər.")
            .When(x => x.LastName != null);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Əlaqə nömrəsi boş ola bilməz.")
            .MinimumLength(8).WithMessage("Telefon nömrəsi ən azı 8 rəqəm olmalıdır.")
            .MaximumLength(30).WithMessage("Telefon nömrəsi ən çox 30 simvol ola bilər.")
            .When(x => x.PhoneNumber != null);

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Ölkə adı ən çox 100 simvol ola bilər.")
            .When(x => x.Country != null);

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Doğum tarixi gələcək tarix ola bilməz.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.MedicalNotes)
            .MaximumLength(2000).WithMessage("Tibbi qeydlər ən çox 2000 simvol ola bilər.")
            .When(x => x.MedicalNotes != null);

        RuleFor(x => x.PreferredLanguage)
            .Must(lang => AllowedLanguages.Contains(lang!.Trim().ToLowerInvariant()))
            .WithMessage("Dil yalnız 'az', 'en' və ya 'ru' ola bilər.")
            .When(x => !string.IsNullOrWhiteSpace(x.PreferredLanguage));
    }
}

