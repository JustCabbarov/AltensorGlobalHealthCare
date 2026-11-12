using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AltensorGlobalHealthCare.Application.Interfaces.Repositories;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Persistence.Contexts;
using AltensorGlobalHealthCare.Persistence.Repositories;

namespace AltensorGlobalHealthCare.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=altensor_global_healthcare_db;Username=postgres;Password=postgres";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IInquiryRepository, InquiryRepository>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        services.AddScoped<IClinicRepository, ClinicRepository>();
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IMedicalServiceRepository, MedicalServiceRepository>();
        services.AddScoped<IUnitOfWork, AltensorGlobalHealthCare.Persistence.UnitOfWork.UnitOfWork>();

        return services;
    }
}
