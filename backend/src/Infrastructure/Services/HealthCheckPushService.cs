using Crm.Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Crm.Infrastructure.Services;

public class HealthCheckPushService : IHealthCheckPublisher
{
    private readonly IEmailService _emailService;
    private readonly ILogger<HealthCheckPushService> _logger;

    public HealthCheckPushService(IEmailService emailService, ILogger<HealthCheckPushService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        if (report.Status == HealthStatus.Unhealthy)
        {
            _logger.StatusLog("Health Check Failed! Sending Email...");
            var subject = "CRITICAL: System Health Alert";
            var body = $"System is UNHEALTHY.\n\nEntries:\n{string.Join("\n", report.Entries.Select(e => $"{e.Key}: {e.Value.Status} - {e.Value.Description}"))}";
            
            try
            {
                await _emailService.SendEmailAsync("admin@example.com", subject, body);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to send Health Alert Email");
            }
        }
    }
}

public static class LoggerExtensions
{
    // Quick helper (or use Serilog directly)
    public static void StatusLog(this ILogger logger, string message) => logger.LogWarning(message);
}
