using BuildingBlocks.Contracts.Health;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace NotificationService.Infrastructure.Health;

public sealed partial class NotificationDatabaseHealthProbe(
    string connectionString,
    ILogger<NotificationDatabaseHealthProbe> logger) : IDatabaseHealthProbe
{
    public async Task<DatabaseHealthProbeResult> CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new MySqlCommand("SELECT 1;", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return new DatabaseHealthProbeResult(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            HealthLog.DatabaseUnavailable(logger, exception);
            return new DatabaseHealthProbeResult(false);
        }
    }

    private static partial class HealthLog
    {
        [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Notification database health probe failed.")]
        public static partial void DatabaseUnavailable(ILogger logger, Exception exception);
    }
}
