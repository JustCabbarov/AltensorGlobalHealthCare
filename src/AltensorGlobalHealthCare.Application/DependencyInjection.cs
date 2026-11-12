using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Application.Services;
using AltensorGlobalHealthCare.Application.Validators;

namespace AltensorGlobalHealthCare.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPublicPortalService, PublicPortalService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<ICoordinatorService, CoordinatorService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
