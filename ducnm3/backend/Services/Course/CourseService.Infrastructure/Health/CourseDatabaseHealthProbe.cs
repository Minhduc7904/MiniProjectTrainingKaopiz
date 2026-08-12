using BuildingBlocks.Contracts.Health;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace CourseService.Infrastructure.Health;

public sealed partial class CourseDatabaseHealthProbe(
    string connectionString,
    ILogger<CourseDatabaseHealthProbe> logger) : IDatabaseHealthProbe
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
        [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Course database health probe failed.")]
        public static partial void DatabaseUnavailable(ILogger logger, Exception exception);
    }
}
