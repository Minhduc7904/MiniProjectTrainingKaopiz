using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MySqlConnector;

namespace BuildingBlocks.DatabaseMigration;

/// <summary>Thông tin service, kết nối và thư mục migration cần thiết để chạy migration SQL.</summary>
public sealed record SqlMigrationRunnerOptions(
    string ServiceName,
    string ConnectionString,
    string MigrationsDirectory);

/// <summary>
/// Chạy SQL migration theo tên <c>V&lt;version&gt;__&lt;name&gt;.sql</c> một cách tuần tự và an toàn trên MySQL.
/// Dùng tại entry point khởi động/migration tool của từng service; migration đã áp dụng không được sửa nội dung.
/// </summary>
public static partial class SqlMigrationRunner
{
    private const int LockTimeoutSeconds = 60;

    /// <summary>
    /// Áp dụng các migration chưa có trong <c>schema_migrations</c>.
    /// <paramref name="options"/> cung cấp database và nguồn file; <paramref name="log"/> nhận tiến trình; tác vụ hoàn thành khi toàn bộ migration hợp lệ đã commit.
    /// </summary>
    public static async Task ApplyAsync(
        SqlMigrationRunnerOptions options,
        Action<string> log,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ServiceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.MigrationsDirectory);
        ArgumentNullException.ThrowIfNull(log);

        if (!Directory.Exists(options.MigrationsDirectory))
        {
            throw new DirectoryNotFoundException(
                $"SQL migrations directory does not exist: {options.MigrationsDirectory}");
        }

        // Khóa theo database ngăn hai instance cùng ghi schema_migrations và chạy trùng migration.
        await using var connection = new MySqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var lockName = $"schema_migrations:{connection.Database}";
        await AcquireLockAsync(connection, lockName, cancellationToken);

        try
        {
            await EnsureHistoryTableAsync(connection, cancellationToken);

            var migrations = GetMigrations(options.MigrationsDirectory);
            var appliedMigrations = await GetAppliedMigrationsAsync(connection, cancellationToken);

            // Checksum bảo vệ lịch sử: bản đã áp dụng phải giữ nguyên, bản mới được chạy trong transaction riêng.
            foreach (var migration in migrations)
            {
                if (appliedMigrations.TryGetValue(migration.Version, out var appliedChecksum))
                {
                    if (!string.Equals(appliedChecksum, migration.Checksum, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Migration checksum mismatch for {migration.Version} ({migration.Name}). " +
                            "Do not edit an applied migration; create a new versioned migration instead.");
                    }

                    continue;
                }

                log($"[{options.ServiceName}] Applying SQL migration {migration.Version}__{migration.Name}.");

                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                try
                {
                    await using var migrationCommand = new MySqlCommand(migration.Sql, connection, transaction);
                    await migrationCommand.ExecuteNonQueryAsync(cancellationToken);

                    await using var historyCommand = new MySqlCommand(
                        """
                        INSERT INTO schema_migrations (version, name, applied_at, checksum)
                        VALUES (@version, @name, UTC_TIMESTAMP(), @checksum);
                        """,
                        connection,
                        transaction);
                    historyCommand.Parameters.AddWithValue("@version", migration.Version);
                    historyCommand.Parameters.AddWithValue("@name", migration.Name);
                    historyCommand.Parameters.AddWithValue("@checksum", migration.Checksum);
                    await historyCommand.ExecuteNonQueryAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                    log($"[{options.ServiceName}] Applied SQL migration {migration.Version}__{migration.Name}.");
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    log($"[{options.ServiceName}] Failed SQL migration {migration.Version}__{migration.Name}.");
                    throw;
                }
            }
        }
        finally
        {
            await ReleaseLockAsync(connection, lockName, cancellationToken);
        }
    }

    /// <summary>Đọc file migration, sắp theo version và từ chối version trùng trước khi chạm database.</summary>
    private static List<SqlMigration> GetMigrations(string migrationsDirectory)
    {
        var migrations = Directory
            .EnumerateFiles(migrationsDirectory, "V*__*.sql", SearchOption.TopDirectoryOnly)
            .Select(SqlMigration.FromFile)
            .OrderBy(migration => migration.Version, StringComparer.Ordinal)
            .ToList();

        var duplicateVersion = migrations
            .GroupBy(migration => migration.Version, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateVersion is not null)
        {
            throw new InvalidOperationException(
                $"Duplicate SQL migration version detected: {duplicateVersion.Key}.");
        }

        return migrations;
    }

    /// <summary>Tạo bảng lịch sử idempotent, nơi lưu version và checksum của migration đã commit.</summary>
    private static async Task EnsureHistoryTableAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(
            """
            CREATE TABLE IF NOT EXISTS schema_migrations (
                version VARCHAR(128) NOT NULL PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                applied_at DATETIME NOT NULL,
                checksum CHAR(64) NOT NULL
            ) ENGINE=InnoDB;
            """,
            connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>Đọc checksum theo version để quyết định migration nào bỏ qua hoặc báo bị chỉnh sửa trái phép.</summary>
    private static async Task<Dictionary<string, string>> GetAppliedMigrationsAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(
            "SELECT version, checksum FROM schema_migrations;",
            connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var migrations = new Dictionary<string, string>(StringComparer.Ordinal);
        while (await reader.ReadAsync(cancellationToken))
        {
            migrations.Add(reader.GetString(0), reader.GetString(1));
        }

        return migrations;
    }

    /// <summary>Lấy advisory lock MySQL trong thời gian giới hạn; ném lỗi nếu không lấy được lock.</summary>
    private static async Task AcquireLockAsync(
        MySqlConnection connection,
        string lockName,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(
            "SELECT GET_LOCK(@lockName, @timeoutSeconds);",
            connection);
        command.Parameters.AddWithValue("@lockName", lockName);
        command.Parameters.AddWithValue("@timeoutSeconds", LockTimeoutSeconds);

        var result = Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
        if (result != 1)
        {
            throw new InvalidOperationException(
                $"Could not acquire migration lock '{lockName}' within {LockTimeoutSeconds} seconds.");
        }
    }

    /// <summary>Giải phóng advisory lock trong finally, kể cả khi migration thất bại.</summary>
    private static async Task ReleaseLockAsync(
        MySqlConnection connection,
        string lockName,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(
            "SELECT RELEASE_LOCK(@lockName);",
            connection);
        command.Parameters.AddWithValue("@lockName", lockName);
        await command.ExecuteScalarAsync(cancellationToken);
    }

    /// <summary>Biểu diễn file migration đã parse, bao gồm SQL và SHA-256 checksum để so với lịch sử.</summary>
    private sealed record SqlMigration(string Version, string Name, string Checksum, string Sql)
    {
        /// <summary>Parse tên file, đọc UTF-8 SQL và tính checksum; ném lỗi nếu tên không theo convention.</summary>
        public static SqlMigration FromFile(string path)
        {
            var fileName = Path.GetFileName(path);
            var match = MigrationFileNameRegex().Match(fileName);

            if (!match.Success)
            {
                throw new InvalidOperationException(
                    $"Invalid SQL migration filename '{fileName}'. Expected V<version>__<name>.sql.");
            }

            var sql = File.ReadAllText(path, Encoding.UTF8);
            var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)))
                .ToLowerInvariant();

            return new SqlMigration(
                match.Groups["version"].Value,
                match.Groups["name"].Value,
                checksum,
                sql);
        }
    }

    [GeneratedRegex(@"^V(?<version>[0-9]+)__(?<name>[a-z0-9_-]+)\.sql$")]
    private static partial Regex MigrationFileNameRegex();
}
