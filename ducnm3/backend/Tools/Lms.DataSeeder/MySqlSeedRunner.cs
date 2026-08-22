using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using MySqlConnector;

namespace Lms.DataSeeder;

public sealed class MySqlSeedRunner(
    SeedOptions options,
    ISeedProgress? progress = null)
{
    private const string SeedLockName = "lms:development-data-seed";
    private const int MaximumBatchAttempts = 3;
    private readonly ISeedProgress progress = progress ?? NullSeedProgress.Instance;
    private readonly DeterministicSeedData data = new(options);

    public async Task<SeedRunSummary> RunAsync(
        string environment,
        CancellationToken cancellationToken = default)
    {
        options.Validate(environment);
        var stopwatch = Stopwatch.StartNew();
        var plan = data.CalculatePlan();

        await using var studentConnection = new MySqlConnection(options.StudentConnectionString);
        await studentConnection.OpenAsync(cancellationToken);

        if (options.StudentsOnly)
        {
            await AcquireLockAsync(studentConnection, cancellationToken);
            try
            {
                var existingStudents = await PreflightStudentsOnlyAsync(
                    studentConnection,
                    plan,
                    cancellationToken);

                if (options.DryRun)
                {
                    return new SeedRunSummary(plan, 0, existingStudents, stopwatch.Elapsed);
                }

                await SeedStudentsAsync(studentConnection, existingStudents, cancellationToken);
                await ValidateStudentsOnlyResultAsync(
                    studentConnection,
                    plan,
                    cancellationToken);

                stopwatch.Stop();
                return new SeedRunSummary(
                    plan,
                    plan.Students - existingStudents,
                    existingStudents,
                    stopwatch.Elapsed);
            }
            finally
            {
                await ReleaseLockAsync(studentConnection);
            }
        }

        await using var courseConnection = new MySqlConnection(options.CourseConnectionString);
        await courseConnection.OpenAsync(cancellationToken);
        await AcquireLockAsync(courseConnection, cancellationToken);

        try
        {
            var existing = await PreflightAsync(
                studentConnection,
                courseConnection,
                plan,
                cancellationToken);

            if (options.DryRun)
            {
                return new SeedRunSummary(plan, 0, existing.TotalRows, stopwatch.Elapsed);
            }

            await SeedStudentsAsync(studentConnection, existing.Students, cancellationToken);
            await SeedCoursesAsync(courseConnection, existing.Courses, cancellationToken);
            await SeedLessonsAsync(courseConnection, plan.Lessons, existing.Lessons, cancellationToken);
            await SeedEnrollmentsAsync(
                courseConnection,
                plan.Enrollments,
                existing.Enrollments,
                cancellationToken);
            await SeedLessonProgressesAsync(
                courseConnection,
                plan.LessonProgresses,
                existing.LessonProgresses,
                cancellationToken);
            await ValidateResultAsync(
                studentConnection,
                courseConnection,
                plan,
                cancellationToken);

            stopwatch.Stop();
            return new SeedRunSummary(
                plan,
                plan.TotalRows - existing.TotalRows,
                existing.TotalRows,
                stopwatch.Elapsed);
        }
        finally
        {
            await ReleaseLockAsync(courseConnection);
        }
    }

    private async Task<SeedExistingCounts> PreflightAsync(
        MySqlConnection studentConnection,
        MySqlConnection courseConnection,
        SeedPlan plan,
        CancellationToken cancellationToken)
    {
        await EnsureSchemaReadyAsync(
            studentConnection,
            ["schema_migrations", "students"],
            cancellationToken);
        await EnsureSchemaReadyAsync(
            courseConnection,
            ["schema_migrations", "courses", "lessons", "enrollments", "lesson_progresses"],
            cancellationToken);

        var existing = new SeedExistingCounts(
            await CountRowsAsync(studentConnection, "students", cancellationToken),
            await CountRowsAsync(courseConnection, "courses", cancellationToken),
            await CountRowsAsync(courseConnection, "lessons", cancellationToken),
            await CountRowsAsync(courseConnection, "enrollments", cancellationToken),
            await CountRowsAsync(courseConnection, "lesson_progresses", cancellationToken));

        if (!options.Resume && existing.TotalRows > 0)
        {
            throw new SeedValidationException(
                "Seed target tables are not empty. Reset the development databases first, " +
                "or rerun the same interrupted dataset with --resume and the same --random-seed.");
        }

        if (existing.Students > plan.Students ||
            existing.Courses > plan.Courses ||
            existing.Lessons > plan.Lessons ||
            existing.Enrollments > plan.Enrollments ||
            existing.LessonProgresses > plan.LessonProgresses)
        {
            throw new SeedValidationException(
                "Existing row counts exceed the requested deterministic dataset. " +
                "Reset the development databases before seeding.");
        }

        return existing;
    }

    private async Task<long> PreflightStudentsOnlyAsync(
        MySqlConnection studentConnection,
        SeedPlan plan,
        CancellationToken cancellationToken)
    {
        await EnsureSchemaReadyAsync(
            studentConnection,
            ["schema_migrations", "students"],
            cancellationToken);

        var existingStudents = await CountSeedStudentsAsync(studentConnection, cancellationToken);
        if (!options.Resume && existingStudents > 0)
        {
            throw new SeedValidationException(
                "Student seed rows for this random seed already exist. Rerun with --resume and the same options.");
        }

        if (existingStudents > plan.Students)
        {
            throw new SeedValidationException(
                "Existing Student seed rows exceed the requested deterministic dataset.");
        }

        return existingStudents;
    }

    private async Task SeedStudentsAsync(
        MySqlConnection connection,
        long existingRows,
        CancellationToken cancellationToken)
    {
        var total = options.StudentCount;
        progress.PhaseStarted(SeedPhase.Students, total);
        var stopwatch = Stopwatch.StartNew();
        long processed = 0;

        for (var start = 1; start <= total; start += options.BatchSize)
        {
            var count = Math.Min(options.BatchSize, total - start + 1);
            var rows = new List<StudentSeedRow>(count);
            for (var offset = 0; offset < count; offset++)
            {
                rows.Add(data.CreateStudent(start + offset));
            }

            await ExecuteStudentBatchAsync(connection, rows, cancellationToken);
            processed += count;
            ReportProgress(SeedPhase.Students, processed, total, existingRows);
        }

        progress.PhaseCompleted(
            SeedPhase.Students,
            total,
            total - existingRows,
            existingRows,
            stopwatch.Elapsed);
    }

    private async Task SeedCoursesAsync(
        MySqlConnection connection,
        long existingRows,
        CancellationToken cancellationToken)
    {
        var total = options.CourseCount;
        progress.PhaseStarted(SeedPhase.Courses, total);
        var stopwatch = Stopwatch.StartNew();
        long processed = 0;

        for (var start = 1; start <= total; start += options.BatchSize)
        {
            var count = Math.Min(options.BatchSize, total - start + 1);
            var rows = new List<CourseSeedRow>(count);
            for (var offset = 0; offset < count; offset++)
            {
                rows.Add(data.CreateCourse(start + offset));
            }

            await ExecuteCourseBatchAsync(connection, rows, cancellationToken);
            processed += count;
            ReportProgress(SeedPhase.Courses, processed, total, existingRows);
        }

        progress.PhaseCompleted(
            SeedPhase.Courses,
            total,
            total - existingRows,
            existingRows,
            stopwatch.Elapsed);
    }

    private async Task SeedLessonsAsync(
        MySqlConnection connection,
        long total,
        long existingRows,
        CancellationToken cancellationToken)
    {
        progress.PhaseStarted(SeedPhase.Lessons, total);
        var stopwatch = Stopwatch.StartNew();
        var rows = new List<LessonSeedRow>(options.BatchSize);
        long processed = 0;

        for (var courseIndex = 1; courseIndex <= options.CourseCount; courseIndex++)
        {
            var lessonCount = data.GetLessonCount(courseIndex);
            for (var displayOrder = 1; displayOrder <= lessonCount; displayOrder++)
            {
                rows.Add(data.CreateLesson(courseIndex, displayOrder));
                if (rows.Count == options.BatchSize)
                {
                    await ExecuteLessonBatchAsync(connection, rows, cancellationToken);
                    processed += rows.Count;
                    rows.Clear();
                    ReportProgress(SeedPhase.Lessons, processed, total, existingRows);
                }
            }
        }

        if (rows.Count > 0)
        {
            await ExecuteLessonBatchAsync(connection, rows, cancellationToken);
            processed += rows.Count;
            ReportProgress(SeedPhase.Lessons, processed, total, existingRows);
        }

        progress.PhaseCompleted(
            SeedPhase.Lessons,
            total,
            total - existingRows,
            existingRows,
            stopwatch.Elapsed);
    }

    private async Task SeedEnrollmentsAsync(
        MySqlConnection connection,
        long total,
        long existingRows,
        CancellationToken cancellationToken)
    {
        progress.PhaseStarted(SeedPhase.Enrollments, total);
        var stopwatch = Stopwatch.StartNew();
        var rows = new List<EnrollmentSeedRow>(options.BatchSize);
        long processed = 0;

        for (var studentIndex = 1; studentIndex <= options.StudentCount; studentIndex++)
        {
            foreach (var courseIndex in data.GetCourseIndexesForStudent(studentIndex))
            {
                rows.Add(data.CreateEnrollment(studentIndex, courseIndex));
                if (rows.Count == options.BatchSize)
                {
                    await ExecuteEnrollmentBatchAsync(connection, rows, cancellationToken);
                    processed += rows.Count;
                    rows.Clear();
                    ReportProgress(SeedPhase.Enrollments, processed, total, existingRows);
                }
            }
        }

        if (rows.Count > 0)
        {
            await ExecuteEnrollmentBatchAsync(connection, rows, cancellationToken);
            processed += rows.Count;
            ReportProgress(SeedPhase.Enrollments, processed, total, existingRows);
        }

        progress.PhaseCompleted(
            SeedPhase.Enrollments,
            total,
            total - existingRows,
            existingRows,
            stopwatch.Elapsed);
    }

    private async Task SeedLessonProgressesAsync(
        MySqlConnection connection,
        long total,
        long existingRows,
        CancellationToken cancellationToken)
    {
        if (!options.CourseApiLarge) return;

        progress.PhaseStarted(SeedPhase.LessonProgresses, total);
        var stopwatch = Stopwatch.StartNew();
        var rows = new List<LessonProgressSeedRow>(options.BatchSize);
        long processed = 0;
        for (var courseIndex = 1; courseIndex <= options.CourseCount; courseIndex++)
        {
            rows.Add(data.CreateLessonProgress(courseIndex, options.StudentCount));
            if (rows.Count == options.BatchSize)
            {
                await ExecuteLessonProgressBatchAsync(connection, rows, cancellationToken);
                processed += rows.Count;
                rows.Clear();
                ReportProgress(SeedPhase.LessonProgresses, processed, total, existingRows);
            }
        }

        if (rows.Count > 0)
        {
            await ExecuteLessonProgressBatchAsync(connection, rows, cancellationToken);
            processed += rows.Count;
            ReportProgress(SeedPhase.LessonProgresses, processed, total, existingRows);
        }

        progress.PhaseCompleted(SeedPhase.LessonProgresses, total, total - existingRows, existingRows, stopwatch.Elapsed);
    }

    private async Task ValidateResultAsync(
        MySqlConnection studentConnection,
        MySqlConnection courseConnection,
        SeedPlan plan,
        CancellationToken cancellationToken)
    {
        progress.PhaseStarted(SeedPhase.Validation, 4);
        var stopwatch = Stopwatch.StartNew();
        var actual = new SeedExistingCounts(
            await CountRowsAsync(studentConnection, "students", cancellationToken),
            await CountRowsAsync(courseConnection, "courses", cancellationToken),
            await CountRowsAsync(courseConnection, "lessons", cancellationToken),
            await CountRowsAsync(courseConnection, "enrollments", cancellationToken),
            await CountRowsAsync(courseConnection, "lesson_progresses", cancellationToken));

        progress.PhaseAdvanced(SeedPhase.Validation, 1, 4, 1, 0);
        if (actual.Students != plan.Students ||
            actual.Courses != plan.Courses ||
            actual.Lessons != plan.Lessons ||
            actual.Enrollments != plan.Enrollments ||
            actual.LessonProgresses != plan.LessonProgresses)
        {
            throw new InvalidOperationException(
                "Seed validation failed because final row counts do not match the deterministic plan. " +
                $"Expected {Format(plan)}, actual {Format(actual)}.");
        }

        if (options.CourseApiLarge)
        {
            progress.PhaseAdvanced(SeedPhase.Validation, 4, 4, 4, 0);
            progress.PhaseCompleted(SeedPhase.Validation, 4, 4, 0, stopwatch.Elapsed);
            return;
        }

        var lessonDistribution = await GetDistributionAsync(
            courseConnection,
            "SELECT COUNT(*) AS group_count FROM lessons GROUP BY course_id",
            cancellationToken);
        progress.PhaseAdvanced(SeedPhase.Validation, 2, 4, 2, 0);
        if (lessonDistribution.Groups != plan.Courses ||
            lessonDistribution.Minimum < options.MinLessonsPerCourse ||
            lessonDistribution.Maximum > options.MaxLessonsPerCourse)
        {
            throw new InvalidOperationException("Lesson-per-course validation failed.");
        }

        var enrollmentDistribution = await GetDistributionAsync(
            courseConnection,
            "SELECT COUNT(*) AS group_count FROM enrollments GROUP BY student_id",
            cancellationToken);
        progress.PhaseAdvanced(SeedPhase.Validation, 3, 4, 3, 0);
        if (enrollmentDistribution.Groups != plan.Students ||
            enrollmentDistribution.Minimum < options.MinCoursesPerStudent ||
            enrollmentDistribution.Maximum > options.MaxCoursesPerStudent)
        {
            throw new InvalidOperationException("Course-per-student validation failed.");
        }

        await ValidateEnrollmentStudentIdsAsync(
            studentConnection,
            courseConnection,
            cancellationToken);
        progress.PhaseAdvanced(SeedPhase.Validation, 4, 4, 4, 0);
        progress.PhaseCompleted(SeedPhase.Validation, 4, 4, 0, stopwatch.Elapsed);
    }

    private async Task ValidateStudentsOnlyResultAsync(
        MySqlConnection studentConnection,
        SeedPlan plan,
        CancellationToken cancellationToken)
    {
        progress.PhaseStarted(SeedPhase.Validation, 1);
        var stopwatch = Stopwatch.StartNew();
        var actualStudents = await CountSeedStudentsAsync(studentConnection, cancellationToken);

        if (actualStudents != plan.Students)
        {
            throw new InvalidOperationException(
                "Student-only seed validation failed because final seed row count does not match the deterministic plan. " +
                $"Expected students={plan.Students}, actual students={actualStudents}.");
        }

        progress.PhaseAdvanced(SeedPhase.Validation, 1, 1, 1, 0);
        progress.PhaseCompleted(SeedPhase.Validation, 1, 1, 0, stopwatch.Elapsed);
    }

    private async Task ValidateEnrollmentStudentIdsAsync(
        MySqlConnection studentConnection,
        MySqlConnection courseConnection,
        CancellationToken cancellationToken)
    {
        var firstStudentId = await ExecuteScalarAsync<string>(
            studentConnection,
            "SELECT id FROM students ORDER BY id LIMIT 1;",
            cancellationToken);
        var matchingEnrollment = await ExecuteScalarAsync<long>(
            courseConnection,
            "SELECT COUNT(*) FROM enrollments WHERE student_id = @studentId;",
            cancellationToken,
            ("@studentId", firstStudentId));

        if (matchingEnrollment < options.MinCoursesPerStudent)
        {
            throw new InvalidOperationException(
                "Cross-database logical student reference validation failed.");
        }
    }

    private void ReportProgress(
        SeedPhase phase,
        long processed,
        long total,
        long existingRows)
    {
        var skipped = Math.Min(processed, existingRows);
        progress.PhaseAdvanced(phase, processed, total, processed - skipped, skipped);
    }

    private static Task ExecuteStudentBatchAsync(
        MySqlConnection connection,
        List<StudentSeedRow> rows,
        CancellationToken cancellationToken) =>
        ExecuteBatchAsync(
            connection,
            "students",
            ["id", "email", "display_name", "status"],
            rows.Count,
            (command, rowIndex) =>
            {
                var row = rows[rowIndex];
                AddParameter(command, rowIndex, 0, row.Id.ToString("D"));
                AddParameter(command, rowIndex, 1, row.Email);
                AddParameter(command, rowIndex, 2, row.DisplayName);
                AddParameter(command, rowIndex, 3, row.Status);
            },
            cancellationToken);

    private static Task ExecuteCourseBatchAsync(
        MySqlConnection connection,
        List<CourseSeedRow> rows,
        CancellationToken cancellationToken) =>
        ExecuteBatchAsync(
            connection,
            "courses",
            ["id", "name", "status"],
            rows.Count,
            (command, rowIndex) =>
            {
                var row = rows[rowIndex];
                AddParameter(command, rowIndex, 0, row.Id.ToString("D"));
                AddParameter(command, rowIndex, 1, row.Name);
                AddParameter(command, rowIndex, 2, row.Status);
            },
            cancellationToken);

    private static Task ExecuteLessonBatchAsync(
        MySqlConnection connection,
        List<LessonSeedRow> rows,
        CancellationToken cancellationToken) =>
        ExecuteBatchAsync(
            connection,
            "lessons",
            ["id", "course_id", "title", "display_order"],
            rows.Count,
            (command, rowIndex) =>
            {
                var row = rows[rowIndex];
                AddParameter(command, rowIndex, 0, row.Id.ToString("D"));
                AddParameter(command, rowIndex, 1, row.CourseId.ToString("D"));
                AddParameter(command, rowIndex, 2, row.Title);
                AddParameter(command, rowIndex, 3, row.DisplayOrder);
            },
            cancellationToken);

    private static Task ExecuteEnrollmentBatchAsync(
        MySqlConnection connection,
        List<EnrollmentSeedRow> rows,
        CancellationToken cancellationToken) =>
        ExecuteBatchAsync(
            connection,
            "enrollments",
            ["id", "course_id", "student_id"],
            rows.Count,
            (command, rowIndex) =>
            {
                var row = rows[rowIndex];
                AddParameter(command, rowIndex, 0, row.Id.ToString("D"));
                AddParameter(command, rowIndex, 1, row.CourseId.ToString("D"));
                AddParameter(command, rowIndex, 2, row.StudentId.ToString("D"));
            },
            cancellationToken);

    private static Task ExecuteLessonProgressBatchAsync(
        MySqlConnection connection,
        List<LessonProgressSeedRow> rows,
        CancellationToken cancellationToken) =>
        ExecuteBatchAsync(
            connection,
            "lesson_progresses",
            ["id", "lesson_id", "student_id", "progress_percent"],
            rows.Count,
            (command, rowIndex) =>
            {
                var row = rows[rowIndex];
                AddParameter(command, rowIndex, 0, row.Id.ToString("D"));
                AddParameter(command, rowIndex, 1, row.LessonId.ToString("D"));
                AddParameter(command, rowIndex, 2, row.StudentId.ToString("D"));
                AddParameter(command, rowIndex, 3, row.ProgressPercent);
            },
            cancellationToken);

    private static async Task ExecuteBatchAsync(
        MySqlConnection connection,
        string table,
        IReadOnlyList<string> columns,
        int rowCount,
        Action<MySqlCommand, int> addParameters,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken);
                }

                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = BuildInsertSql(table, columns, rowCount);

                for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    addParameters(command, rowIndex);
                }

                await command.ExecuteNonQueryAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return;
            }
            catch (MySqlException exception)
                when (exception.IsTransient && attempt < MaximumBatchAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
            }
        }
    }

    private static string BuildInsertSql(
        string table,
        IReadOnlyList<string> columns,
        int rowCount)
    {
        var sql = new StringBuilder();
        sql.Append("INSERT INTO `").Append(table).Append("` (`")
            .AppendJoin("`, `", columns)
            .Append("`) VALUES ");

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            if (rowIndex > 0)
            {
                sql.Append(',');
            }

            sql.Append('(');
            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                if (columnIndex > 0)
                {
                    sql.Append(',');
                }

                sql.Append(ParameterName(rowIndex, columnIndex));
            }

            sql.Append(')');
        }

        sql.Append(" ON DUPLICATE KEY UPDATE `id` = `")
            .Append(table)
            .Append("`.`id`;");
        return sql.ToString();
    }

    private static void AddParameter(
        MySqlCommand command,
        int rowIndex,
        int columnIndex,
        object value) =>
        command.Parameters.AddWithValue(ParameterName(rowIndex, columnIndex), value);

    private static string ParameterName(int rowIndex, int columnIndex) =>
        string.Create(CultureInfo.InvariantCulture, $"@p{rowIndex}_{columnIndex}");

    private static async Task EnsureSchemaReadyAsync(
        MySqlConnection connection,
        IReadOnlyCollection<string> requiredTables,
        CancellationToken cancellationToken)
    {
        var placeholders = requiredTables
            .Select((_, index) => $"@table{index}")
            .ToArray();
        var sql =
            "SELECT COUNT(*) FROM information_schema.tables " +
            "WHERE table_schema = DATABASE() AND table_name IN (" +
            string.Join(',', placeholders) +
            ");";

        await using var command = new MySqlCommand(sql, connection);
        var index = 0;
        foreach (var table in requiredTables)
        {
            command.Parameters.AddWithValue(placeholders[index++], table);
        }

        var count = Convert.ToInt64(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);
        if (count != requiredTables.Count)
        {
            throw new SeedValidationException(
                $"Database '{connection.Database}' is not migrated to the expected schema.");
        }

        var baselineApplied = await ExecuteScalarAsync<long>(
            connection,
            "SELECT COUNT(*) FROM schema_migrations WHERE version = '001';",
            cancellationToken);
        if (baselineApplied != 1)
        {
            throw new SeedValidationException(
                $"Database '{connection.Database}' does not contain migration version 001.");
        }
    }

    private static Task<long> CountRowsAsync(
        MySqlConnection connection,
        string table,
        CancellationToken cancellationToken) =>
        ExecuteScalarAsync<long>(
            connection,
            $"SELECT COUNT(*) FROM `{table}`;",
            cancellationToken);

    private Task<long> CountSeedStudentsAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken) =>
        ExecuteScalarAsync<long>(
            connection,
            "SELECT COUNT(*) FROM students WHERE email LIKE @emailPattern;",
            cancellationToken,
            ("@emailPattern", $"seed.{options.RandomSeed}.student.%@example.test"));

    private static async Task<Distribution> GetDistributionAsync(
        MySqlConnection connection,
        string groupedCountSql,
        CancellationToken cancellationToken)
    {
        var sql =
            "SELECT COUNT(*), MIN(group_count), MAX(group_count) FROM (" +
            groupedCountSql +
            ") AS distribution;";
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new Distribution(
            reader.GetInt64(0),
            reader.GetInt64(1),
            reader.GetInt64(2));
    }

    private static async Task<T> ExecuteScalarAsync<T>(
        MySqlConnection connection,
        string sql,
        CancellationToken cancellationToken,
        params (string Name, object? Value)[] parameters)
    {
        await using var command = new MySqlCommand(sql, connection);
        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        }

        var value = await command.ExecuteScalarAsync(cancellationToken);
        if (value is null or DBNull)
        {
            throw new InvalidOperationException("Expected a non-null scalar database result.");
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)Convert.ToString(value, CultureInfo.InvariantCulture)!;
        }

        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture)!;
    }

    private static async Task AcquireLockAsync(
        MySqlConnection connection,
        CancellationToken cancellationToken)
    {
        var acquired = await ExecuteScalarAsync<long>(
            connection,
            "SELECT GET_LOCK(@lockName, 0);",
            cancellationToken,
            ("@lockName", SeedLockName));
        if (acquired != 1)
        {
            throw new SeedValidationException(
                "Another development data seed process is already running.");
        }
    }

    private static async Task ReleaseLockAsync(MySqlConnection connection)
    {
        if (connection.State != ConnectionState.Open)
        {
            return;
        }

        await using var command = new MySqlCommand(
            "SELECT RELEASE_LOCK(@lockName);",
            connection);
        command.Parameters.AddWithValue("@lockName", SeedLockName);
        await command.ExecuteScalarAsync(CancellationToken.None);
    }

    private static string Format(SeedPlan plan) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"students={plan.Students}, courses={plan.Courses}, " +
            $"lessons={plan.Lessons}, enrollments={plan.Enrollments}, " +
            $"lessonProgresses={plan.LessonProgresses}");

    private static string Format(SeedExistingCounts counts) =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"students={counts.Students}, courses={counts.Courses}, " +
            $"lessons={counts.Lessons}, enrollments={counts.Enrollments}, " +
            $"lessonProgresses={counts.LessonProgresses}");

    private sealed record SeedExistingCounts(
        long Students,
        long Courses,
        long Lessons,
        long Enrollments,
        long LessonProgresses)
    {
        public long TotalRows => Students + Courses + Lessons + Enrollments + LessonProgresses;
    }

    private sealed record Distribution(long Groups, long Minimum, long Maximum);
}
