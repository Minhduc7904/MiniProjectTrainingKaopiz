using System.Globalization;
using MySqlConnector;

namespace Lms.DataSeeder;

internal sealed class CourseSeedSchemaOptimizer(
    MySqlConnection connection,
    ISeedProgress progress)
{
    private readonly List<ForeignKeyDefinition> removedForeignKeys = [];
    private readonly List<IndexDefinition> removedIndexes = [];
    private readonly List<PrimaryKeyDefinition> removedPrimaryKeys = [];

    public async Task RemoveAsync(CancellationToken cancellationToken)
    {
        SchemaSnapshot snapshot;
        try
        {
            snapshot = await ReadSnapshotAsync(cancellationToken);
            progress.SchemaActionCompleted(
                SeedSchemaOperation.Remove,
                "schema",
                connection.Database,
                true,
                null);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            progress.SchemaActionCompleted(
                SeedSchemaOperation.Remove,
                "schema",
                connection.Database,
                false,
                exception.Message);
            return;
        }

        foreach (var foreignKey in snapshot.ForeignKeys)
        {
            if (await ExecuteAsync(
                    SeedSchemaOperation.Remove,
                    "foreign key",
                    foreignKey.Name,
                    $"ALTER TABLE {Quote(foreignKey.Table)} DROP FOREIGN KEY {Quote(foreignKey.Name)};",
                    cancellationToken))
            {
                removedForeignKeys.Add(foreignKey);
            }
        }

        foreach (var index in snapshot.Indexes)
        {
            if (await ExecuteAsync(
                    SeedSchemaOperation.Remove,
                    index.NonUnique ? "index" : "unique index",
                    index.Name,
                    $"ALTER TABLE {Quote(index.Table)} DROP INDEX {Quote(index.Name)};",
                    cancellationToken))
            {
                removedIndexes.Add(index);
            }
        }

        foreach (var primaryKey in snapshot.PrimaryKeys)
        {
            if (await ExecuteAsync(
                    SeedSchemaOperation.Remove,
                    "primary key",
                    primaryKey.Table,
                    $"ALTER TABLE {Quote(primaryKey.Table)} DROP PRIMARY KEY;",
                    cancellationToken))
            {
                removedPrimaryKeys.Add(primaryKey);
            }
        }
    }

    public async Task<bool> RestoreAsync(CancellationToken cancellationToken)
    {
        var restored = true;
        restored &= await RestorePrimaryKeysAsync(cancellationToken);
        restored &= await RestoreIndexesAsync(cancellationToken);
        restored &= await RestoreForeignKeysAsync(cancellationToken);
        return restored;
    }

    private async Task<bool> RestorePrimaryKeysAsync(CancellationToken cancellationToken)
    {
        var restored = true;
        for (var index = removedPrimaryKeys.Count - 1; index >= 0; index--)
        {
            var primaryKey = removedPrimaryKeys[index];
            if (await ExecuteAsync(
                    SeedSchemaOperation.Restore,
                    "primary key",
                    primaryKey.Table,
                    $"ALTER TABLE {Quote(primaryKey.Table)} ADD PRIMARY KEY ({Columns(primaryKey.Columns)});",
                    cancellationToken))
            {
                removedPrimaryKeys.RemoveAt(index);
            }
            else
            {
                restored = false;
            }
        }

        return restored;
    }

    private async Task<bool> RestoreIndexesAsync(CancellationToken cancellationToken)
    {
        var restored = true;
        for (var index = removedIndexes.Count - 1; index >= 0; index--)
        {
            var definition = removedIndexes[index];
            if (await ExecuteAsync(
                    SeedSchemaOperation.Restore,
                    definition.NonUnique ? "index" : "unique index",
                    definition.Name,
                    BuildAddIndexSql(definition),
                    cancellationToken))
            {
                removedIndexes.RemoveAt(index);
            }
            else
            {
                restored = false;
            }
        }

        return restored;
    }

    private async Task<bool> RestoreForeignKeysAsync(CancellationToken cancellationToken)
    {
        var restored = true;
        for (var index = removedForeignKeys.Count - 1; index >= 0; index--)
        {
            var foreignKey = removedForeignKeys[index];
            var sql =
                $"ALTER TABLE {Quote(foreignKey.Table)} ADD CONSTRAINT {Quote(foreignKey.Name)} " +
                $"FOREIGN KEY ({Columns(foreignKey.Columns)}) " +
                $"REFERENCES {Quote(foreignKey.ReferencedTable)} ({Columns(foreignKey.ReferencedColumns)}) " +
                $"ON DELETE {foreignKey.DeleteRule} ON UPDATE {foreignKey.UpdateRule};";
            if (await ExecuteAsync(
                    SeedSchemaOperation.Restore,
                    "foreign key",
                    foreignKey.Name,
                    sql,
                    cancellationToken))
            {
                removedForeignKeys.RemoveAt(index);
            }
            else
            {
                restored = false;
            }
        }

        return restored;
    }

    private async Task<SchemaSnapshot> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        var foreignKeys = await ReadForeignKeysAsync(cancellationToken);
        var indexes = await ReadIndexesAsync("INDEX_NAME <> 'PRIMARY'", cancellationToken);
        var primaryKeys = await ReadIndexesAsync("INDEX_NAME = 'PRIMARY'", cancellationToken);

        return new SchemaSnapshot(
            foreignKeys,
            indexes.Select(item => new IndexDefinition(
                item.Table,
                item.Name,
                item.NonUnique,
                item.Type,
                item.Columns)).ToArray(),
            primaryKeys.Select(item => new PrimaryKeyDefinition(item.Table, item.Columns)).ToArray());
    }

    private async Task<IReadOnlyList<ForeignKeyDefinition>> ReadForeignKeysAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT rc.table_name, rc.constraint_name, rc.referenced_table_name,
                   rc.update_rule, rc.delete_rule, kcu.column_name,
                   kcu.referenced_column_name, kcu.ordinal_position
            FROM information_schema.referential_constraints rc
            INNER JOIN information_schema.key_column_usage kcu
                ON kcu.constraint_schema = rc.constraint_schema
                AND kcu.table_name = rc.table_name
                AND kcu.constraint_name = rc.constraint_name
            WHERE rc.constraint_schema = DATABASE()
              AND rc.table_name IN ('courses', 'lessons', 'enrollments', 'lesson_progresses')
            ORDER BY rc.table_name, rc.constraint_name, kcu.ordinal_position;
            """;

        var rows = new List<ForeignKeyRow>();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new ForeignKeyRow(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6)));
        }

        return rows
            .GroupBy(row => new { row.Table, row.Name, row.ReferencedTable, row.UpdateRule, row.DeleteRule })
            .Select(group => new ForeignKeyDefinition(
                group.Key.Table,
                group.Key.Name,
                group.Key.ReferencedTable,
                group.Key.UpdateRule,
                group.Key.DeleteRule,
                group.Select(row => row.Column).ToArray(),
                group.Select(row => row.ReferencedColumn).ToArray()))
            .ToArray();
    }

    private async Task<IReadOnlyList<IndexRowGroup>> ReadIndexesAsync(
        string primaryKeyPredicate,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT table_name, index_name, non_unique, index_type, column_name,
                   collation, sub_part, seq_in_index
            FROM information_schema.statistics
            WHERE table_schema = DATABASE()
              AND table_name IN ('courses', 'lessons', 'enrollments', 'lesson_progresses')
              AND {primaryKeyPredicate}
            ORDER BY table_name, index_name, seq_in_index;
            """;

        var rows = new List<IndexRow>();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (reader.IsDBNull(4))
            {
                throw new SeedValidationException(
                    $"Cannot optimize functional index '{reader.GetString(1)}' on '{reader.GetString(0)}'.");
            }

            rows.Add(new IndexRow(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt64(2) != 0,
                reader.GetString(3),
                new IndexColumn(
                    reader.GetString(4),
                    reader.IsDBNull(5) ? null : reader.GetString(5),
                    reader.IsDBNull(6) ? null : reader.GetInt64(6))));
        }

        return rows
            .GroupBy(row => new { row.Table, row.Name, row.NonUnique, row.Type })
            .Select(group => new IndexRowGroup(
                group.Key.Table,
                group.Key.Name,
                group.Key.NonUnique,
                group.Key.Type,
                group.Select(row => row.Column).ToArray()))
            .ToArray();
    }

    private async Task<bool> ExecuteAsync(
        SeedSchemaOperation operation,
        string objectType,
        string name,
        string sql,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var command = new MySqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
            progress.SchemaActionCompleted(operation, objectType, name, true, null);
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            progress.SchemaActionCompleted(operation, objectType, name, false, exception.Message);
            return false;
        }
    }

    private static string BuildAddIndexSql(IndexDefinition index)
    {
        var kind = index.Type.ToUpperInvariant() switch
        {
            "FULLTEXT" => "FULLTEXT INDEX",
            "SPATIAL" => "SPATIAL INDEX",
            _ when index.NonUnique => "INDEX",
            _ => "UNIQUE INDEX",
        };
        var usingClause = index.Type.Equals("BTREE", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : $" USING {index.Type}";
        return $"ALTER TABLE {Quote(index.Table)} ADD {kind} {Quote(index.Name)}{usingClause} ({Columns(index.Columns)});";
    }

    private static string Columns(IEnumerable<string> columns) =>
        string.Join(", ", columns.Select(Quote));

    private static string Columns(IEnumerable<IndexColumn> columns) =>
        string.Join(", ", columns.Select(column =>
        {
            var prefix = column.SubPart is null
                ? string.Empty
                : $"({column.SubPart.Value.ToString(CultureInfo.InvariantCulture)})";
            var direction = string.Equals(column.Collation, "D", StringComparison.Ordinal)
                ? " DESC"
                : string.Empty;
            return $"{Quote(column.Name)}{prefix}{direction}";
        }));

    private static string Quote(string identifier) =>
        $"`{identifier.Replace("`", "``", StringComparison.Ordinal)}`";

    private sealed record SchemaSnapshot(
        IReadOnlyList<ForeignKeyDefinition> ForeignKeys,
        IReadOnlyList<IndexDefinition> Indexes,
        IReadOnlyList<PrimaryKeyDefinition> PrimaryKeys);

    private sealed record ForeignKeyDefinition(
        string Table,
        string Name,
        string ReferencedTable,
        string UpdateRule,
        string DeleteRule,
        IReadOnlyList<string> Columns,
        IReadOnlyList<string> ReferencedColumns);

    private sealed record IndexDefinition(
        string Table,
        string Name,
        bool NonUnique,
        string Type,
        IReadOnlyList<IndexColumn> Columns);

    private sealed record PrimaryKeyDefinition(string Table, IReadOnlyList<IndexColumn> Columns);

    private sealed record ForeignKeyRow(
        string Table,
        string Name,
        string ReferencedTable,
        string UpdateRule,
        string DeleteRule,
        string Column,
        string ReferencedColumn);

    private sealed record IndexRow(
        string Table,
        string Name,
        bool NonUnique,
        string Type,
        IndexColumn Column);

    private sealed record IndexRowGroup(
        string Table,
        string Name,
        bool NonUnique,
        string Type,
        IReadOnlyList<IndexColumn> Columns);

    private sealed record IndexColumn(string Name, string? Collation, long? SubPart);
}
