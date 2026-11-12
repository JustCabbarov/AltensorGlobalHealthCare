using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AltensorGlobalHealthCare.Application.Interfaces.UnitOfWork;
using AltensorGlobalHealthCare.Domain.Enums;

namespace AltensorGlobalHealthCare.Infrastructure.BackgroundJobs;

public class OfferExpirationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OfferExpirationJob> _logger;

    public OfferExpirationJob(IServiceProvider serviceProvider, ILogger<OfferExpirationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Təkliflərin müddətini yoxlayan fon xidməti işə düşdü.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var expiredOffers = await unitOfWork.Offers.GetListAsync(
                    o => o.Status == OfferStatus.Sent && o.ExpiresAtUtc < DateTime.UtcNow,
                    stoppingToken);

                if (expiredOffers.Count > 0)
                {
                    foreach (var offer in expiredOffers)
                    {
                        offer.Status = OfferStatus.Expired;
                        unitOfWork.Offers.Update(offer);
                    }

                    await unitOfWork.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("{Count} ədəd müddəti bitmiş təklif 'Expired' statusuna keçirildi.", expiredOffers.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OfferExpirationJob icrasında xəta baş verdi.");
            }

            // Check every 30 minutes
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}

