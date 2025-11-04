using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HotelBookingPlatform.Application.Core.Abstracts.IBookingManagementService;
using HotelBookingPlatform.Domain.ILogger;

namespace HotelBookingPlatform.Application.Services;

public class BookingCleanupBackgroundService : BackgroundService
{
    private readonly ILogger<BookingCleanupBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BookingCleanupOptions _options;

    public BookingCleanupBackgroundService(
        ILogger<BookingCleanupBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<BookingCleanupOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking Cleanup Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredBookings(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing expired bookings");
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var log = scope.ServiceProvider.GetService<HotelBookingPlatform.Domain.ILogger.ILog>();
                    log?.Log($"BookingCleanup: Error processing expired bookings - {ex.Message}", "error");
                }
                catch
                {
                    // Swallow any exceptions from logging to avoid crash in the background service
                }
            }

            await Task.Delay(_options.CheckIntervalMinutes * 60 * 1000, stoppingToken);
        }
    }

    private async Task ProcessExpiredBookings(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var releasedBookings = await bookingService.ReleaseExpiredBookingsAsync();

        if (releasedBookings.Any())
        {
            _logger.LogInformation("Released {Count} expired bookings", releasedBookings.Count);
            try
            {
                var log = scope.ServiceProvider.GetService<HotelBookingPlatform.Domain.ILogger.ILog>();
                log?.Log($"BookingCleanup: Released {releasedBookings.Count} expired bookings", "info");
            }
            catch
            {
                // ignore logging failures
            }
        }
    }
}

public class BookingCleanupOptions
{
    public const string ConfigSection = "BookingCleanup";
    public int CheckIntervalMinutes { get; set; } = 30; // Default to 30 minutes if not configured
}