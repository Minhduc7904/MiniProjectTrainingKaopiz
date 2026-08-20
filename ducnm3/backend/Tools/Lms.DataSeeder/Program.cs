using System.Diagnostics;
using System.Globalization;
using MySqlConnector;
using Spectre.Console;

namespace Lms.DataSeeder;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Contains("--help", StringComparer.Ordinal) ||
            args.Contains("-h", StringComparer.Ordinal))
        {
            PrintHelp();
            return 0;
        }

        using var cancellationSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationSource.Cancel();
        };

        try
        {
            var options = ParseOptions(args);
            var environment =
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;
            options.Validate(environment);
            PrintConfiguration(options);

            SeedRunSummary? summary = null;
            await AnsiConsole.Progress()
                .AutoClear(false)
                .HideCompleted(false)
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn())
                .StartAsync(async context =>
                {
                    var progress = new TerminalSeedProgress(context);
                    var runner = new MySqlSeedRunner(options, progress);
                    summary = await runner.RunAsync(environment, cancellationSource.Token);
                });

            PrintSummary(summary!);
            return 0;
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[yellow]Seed cancelled. Rerun with --resume and the same options.[/]");
            return 130;
        }
        catch (SeedValidationException exception)
        {
            AnsiConsole.MarkupLine($"[red]Safety check failed:[/] {Markup.Escape(exception.Message)}");
            return 2;
        }
        catch (MySqlException exception)
        {
            AnsiConsole.MarkupLine(
                $"[red]MySQL seed failed:[/] {Markup.Escape(exception.Message)}");
            return 3;
        }
        catch (Exception exception)
        {
            AnsiConsole.MarkupLine(
                $"[red]Seed failed:[/] {Markup.Escape(exception.Message)}");
            return 1;
        }
    }

    public static SeedOptions ParseOptions(IReadOnlyList<string> args)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var flags = new HashSet<string>(StringComparer.Ordinal);
        var valueOptions = new HashSet<string>(
            [
                "--students",
                "--courses",
                "--min-lessons",
                "--max-lessons",
                "--min-courses-per-student",
                "--max-courses-per-student",
                "--batch-size",
                "--random-seed",
                "--profile",
            ],
            StringComparer.Ordinal);
        var flagOptions = new HashSet<string>(
            ["--confirm", "--resume", "--dry-run"],
            StringComparer.Ordinal);

        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];
            if (flagOptions.Contains(argument))
            {
                flags.Add(argument);
                continue;
            }

            if (!valueOptions.Contains(argument))
            {
                throw new SeedValidationException($"Unknown option '{argument}'. Use --help.");
            }

            if (++index >= args.Count)
            {
                throw new SeedValidationException($"Option '{argument}' requires a value.");
            }

            values[argument] = args[index];
        }

        var profile = values.GetValueOrDefault("--profile");
        if (profile is not null && profile != "course-api-large")
        {
            throw new SeedValidationException("--profile must be course-api-large.");
        }

        var courseApiLarge = profile == "course-api-large";
        return new SeedOptions(
            RequiredEnvironmentVariable(
                "SEED_STUDENT_DB_CONNECTION_STRING",
                "STUDENT_DB_CONNECTION_STRING"),
            RequiredEnvironmentVariable(
                "SEED_COURSE_DB_CONNECTION_STRING",
                "COURSE_DB_CONNECTION_STRING"),
            ParseInteger(values, "--students", SeedOptions.DefaultStudentCount),
            ParseInteger(values, "--courses", courseApiLarge ? 3_000_000 : SeedOptions.DefaultCourseCount),
            ParseInteger(values, "--min-lessons", 1),
            ParseInteger(values, "--max-lessons", courseApiLarge ? 1 : 5),
            ParseInteger(values, "--min-courses-per-student", courseApiLarge ? 10 : 1),
            ParseInteger(values, "--max-courses-per-student", 10),
            ParseInteger(values, "--batch-size", courseApiLarge ? 2_000 : SeedOptions.DefaultBatchSize),
            ParseInteger(values, "--random-seed", SeedOptions.DefaultRandomSeed),
            flags.Contains("--confirm"),
            flags.Contains("--resume"),
            flags.Contains("--dry-run"),
            courseApiLarge);
    }

    private static int ParseInteger(
        Dictionary<string, string> values,
        string option,
        int defaultValue)
    {
        if (!values.TryGetValue(option, out var rawValue))
        {
            return defaultValue;
        }

        if (!int.TryParse(
                rawValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var value))
        {
            throw new SeedValidationException($"Option '{option}' must be an integer.");
        }

        return value;
    }

    private static string RequiredEnvironmentVariable(
        string preferredName,
        string fallbackName)
    {
        var value = Environment.GetEnvironmentVariable(preferredName);
        if (string.IsNullOrWhiteSpace(value))
        {
            value = Environment.GetEnvironmentVariable(fallbackName);
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new SeedValidationException(
                $"{preferredName} or {fallbackName} environment variable is required.");
        }

        return value;
    }

    private static void PrintConfiguration(SeedOptions options)
    {
        var student = new MySqlConnectionStringBuilder(options.StudentConnectionString);
        var course = new MySqlConnectionStringBuilder(options.CourseConnectionString);
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Setting")
            .AddColumn("Value")
            .AddRow("Student database", $"{student.Server}:{student.Port}/{student.Database}")
            .AddRow("Course database", $"{course.Server}:{course.Port}/{course.Database}")
            .AddRow("Students", $"{options.StudentCount:N0}")
            .AddRow("Courses", $"{options.CourseCount:N0}")
            .AddRow(
                "Lessons per course",
                $"{options.MinLessonsPerCourse}-{options.MaxLessonsPerCourse}")
            .AddRow(
                "Courses per student",
                $"{options.MinCoursesPerStudent}-{options.MaxCoursesPerStudent}")
            .AddRow("Batch size", $"{options.BatchSize:N0}")
            .AddRow("Random seed", options.RandomSeed.ToString(CultureInfo.InvariantCulture))
            .AddRow("Mode", options.DryRun ? "dry-run" : options.Resume ? "resume" : "fresh");

        AnsiConsole.Write(new Rule("[green]LMS Development Data Seeder[/]"));
        AnsiConsole.Write(table);
    }

    private static void PrintSummary(SeedRunSummary summary)
    {
        var summaryTable = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Result")
            .AddColumn("Rows")
            .AddRow("Students", $"{summary.Plan.Students:N0}")
            .AddRow("Courses", $"{summary.Plan.Courses:N0}")
            .AddRow("Lessons", $"{summary.Plan.Lessons:N0}")
            .AddRow("Enrollments", $"{summary.Plan.Enrollments:N0}")
            .AddRow("Lesson progresses", $"{summary.Plan.LessonProgresses:N0}")
            .AddRow("Inserted this run", $"{summary.InsertedRows:N0}")
            .AddRow("Already present", $"{summary.SkippedRows:N0}")
            .AddRow("Elapsed", summary.Elapsed.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture));

        AnsiConsole.Write(new Rule("[green]Seed complete and validated[/]"));
        AnsiConsole.Write(summaryTable);
    }

    private static void PrintHelp()
    {
        AnsiConsole.Write(
            new Panel(
                new Text(
                    """
                Usage: dotnet run --project backend/Tools/Lms.DataSeeder -- [options]

                  --confirm                         Required for writes
                  --resume                          Resume the same deterministic dataset
                  --dry-run                         Validate schema and show planned counts only
                  --profile course-api-large        3M courses with lessons and progress
                  --students <1..100000>            Default: 100000
                  --courses <1..300000>             Default: 100000; profile: up to 3000000
                  --min-lessons <1..5>              Default: 1
                  --max-lessons <1..5>              Default: 5
                  --min-courses-per-student <1..10> Default: 1
                  --max-courses-per-student <1..10> Default: 10
                  --batch-size <1..2000>            Default: 1000
                  --random-seed <integer>            Default: 20260813

                Required environment:
                  ASPNETCORE_ENVIRONMENT=Development
                  STUDENT_DB_CONNECTION_STRING (or SEED_STUDENT_DB_CONNECTION_STRING)
                  COURSE_DB_CONNECTION_STRING  (or SEED_COURSE_DB_CONNECTION_STRING)
                """))
                .Header("Lms.DataSeeder")
                .Border(BoxBorder.Rounded));
    }

    private sealed class TerminalSeedProgress(ProgressContext context) : ISeedProgress
    {
        private readonly Dictionary<SeedPhase, ProgressTask> tasks = [];
        private readonly Dictionary<SeedPhase, Stopwatch> timers = [];

        public void PhaseStarted(SeedPhase phase, long totalRows)
        {
            tasks[phase] = context.AddTask(
                $"[blue]{phase}[/]",
                maxValue: totalRows);
            timers[phase] = Stopwatch.StartNew();
        }

        public void PhaseAdvanced(
            SeedPhase phase,
            long processedRows,
            long totalRows,
            long insertedRows,
            long skippedRows)
        {
            var elapsedSeconds = Math.Max(timers[phase].Elapsed.TotalSeconds, 0.001);
            var rowsPerSecond = processedRows / elapsedSeconds;
            var task = tasks[phase];
            task.MaxValue = totalRows;
            task.Value = processedRows;
            task.Description =
                $"[blue]{phase}[/] {processedRows:N0}/{totalRows:N0} " +
                $"[green]+{insertedRows:N0}[/] [grey]skip {skippedRows:N0}[/] " +
                $"[yellow]{rowsPerSecond:N0}/s[/]";
        }

        public void PhaseCompleted(
            SeedPhase phase,
            long totalRows,
            long insertedRows,
            long skippedRows,
            TimeSpan elapsed)
        {
            var task = tasks[phase];
            task.Value = totalRows;
            task.Description =
                $"[green]{phase}[/] {totalRows:N0} rows in {elapsed:hh\\:mm\\:ss}";
            task.StopTask();
            timers[phase].Stop();
        }
    }
}
