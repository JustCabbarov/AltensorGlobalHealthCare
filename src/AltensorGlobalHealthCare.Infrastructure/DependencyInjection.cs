using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AltensorGlobalHealthCare.Application.Interfaces.Services;
using AltensorGlobalHealthCare.Infrastructure.BackgroundJobs;
using AltensorGlobalHealthCare.Infrastructure.Identity;
using AltensorGlobalHealthCare.Infrastructure.Notifications;
using AltensorGlobalHealthCare.Infrastructure.Storage;

namespace AltensorGlobalHealthCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddHostedService<OfferExpirationJob>();

        return services;
    }
}

