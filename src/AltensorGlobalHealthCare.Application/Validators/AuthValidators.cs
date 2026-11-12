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

