using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Crm.Infrastructure.Services;

public class SqlDatabaseSizeHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly long _maxSizeBytes;

    private readonly ILogger<SqlDatabaseSizeHealthCheck> _logger;

    public SqlDatabaseSizeHealthCheck(IConfiguration configuration, ILogger<SqlDatabaseSizeHealthCheck> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? configuration.GetConnectionString("PostgresConnection") 
            ?? throw new ArgumentNullException("DefaultConnection");
        _logger = logger;
        var limitGb = configuration.GetValue<int>("HealthChecks:MaxDatabaseSizeGb", 10);
        _maxSizeBytes = limitGb * 1024L * 1024L * 1024L;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT pg_database_size(current_database());";
            
            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result != null && long.TryParse(result.ToString(), out var sizeBytes))
            {
                var status = sizeBytes < _maxSizeBytes ? HealthStatus.Healthy : HealthStatus.Degraded;
                var sizeMb = sizeBytes / 1024 / 1024;
                var description = $"PostgreSQL DB Size: {sizeMb} MB (Threshold: {_maxSizeBytes / 1024 / 1024 / 1024} GB)";
                
                if (status != HealthStatus.Healthy)
                {
                    _logger.LogWarning("Database size threshold warning: {Description}", description);
                }

                return new HealthCheckResult(status, description);
            }
            
            return HealthCheckResult.Healthy("Could not determine DB Size");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking database size");
            return HealthCheckResult.Unhealthy("Error checking database size", ex);
        }
    }
}
