using MySqlConnector;

namespace Lms.DataSeeder;

public sealed record SeedOptions(
    string StudentConnectionString,
    string CourseConnectionString,
    int StudentCount,
    int CourseCount,
    int MinLessonsPerCourse,
    int MaxLessonsPerCourse,
    int MinCoursesPerStudent,
    int MaxCoursesPerStudent,
    int BatchSize,
    int RandomSeed,
    bool Confirmed,
    bool Resume,
    bool DryRun)
{
    public const int DefaultStudentCount = 100_000;
    public const int DefaultCourseCount = 100_000;
    public const int DefaultRandomSeed = 20_260_813;
    public const int DefaultBatchSize = 1_000;

    public void Validate(string environment)
    {
        if (!string.Equals(environment, "Development", StringComparison.Ordinal))
        {
            throw new SeedValidationException(
                "Data seeding is allowed only when ASPNETCORE_ENVIRONMENT=Development.");
        }

        if (!DryRun && !Confirmed)
        {
            throw new SeedValidationException(
                "Refusing to write data without the explicit --confirm flag.");
        }

        ValidateRange(StudentCount, 1, DefaultStudentCount, nameof(StudentCount));
        ValidateRange(CourseCount, 1, DefaultCourseCount, nameof(CourseCount));
        ValidateRange(MinLessonsPerCourse, 1, 5, nameof(MinLessonsPerCourse));
        ValidateRange(MaxLessonsPerCourse, MinLessonsPerCourse, 5, nameof(MaxLessonsPerCourse));
        ValidateRange(MinCoursesPerStudent, 1, 10, nameof(MinCoursesPerStudent));
        ValidateRange(MaxCoursesPerStudent, MinCoursesPerStudent, 10, nameof(MaxCoursesPerStudent));
        ValidateRange(BatchSize, 1, 2_000, nameof(BatchSize));

        if (MaxCoursesPerStudent > CourseCount)
        {
            throw new SeedValidationException(
                "MaxCoursesPerStudent cannot exceed CourseCount.");
        }

        ValidateConnectionString(
            StudentConnectionString,
            "lms_student_db",
            nameof(StudentConnectionString));
        ValidateConnectionString(
            CourseConnectionString,
            "lms_course_db",
            nameof(CourseConnectionString));
    }

    private static void ValidateRange(int value, int minimum, int maximum, string name)
    {
        if (value < minimum || value > maximum)
        {
            throw new SeedValidationException(
                $"{name} must be between {minimum:N0} and {maximum:N0}.");
        }
    }

    private static void ValidateConnectionString(
        string connectionString,
        string expectedDatabase,
        string name)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new SeedValidationException($"{name} is required.");
        }

        MySqlConnectionStringBuilder builder;
        try
        {
            builder = new MySqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException exception)
        {
            throw new SeedValidationException($"{name} is invalid.", exception);
        }

        if (!string.Equals(builder.Database, expectedDatabase, StringComparison.Ordinal))
        {
            throw new SeedValidationException(
                $"{name} must target the Development database '{expectedDatabase}'.");
        }
    }
}

public sealed class SeedValidationException : Exception
{
    public SeedValidationException(string message)
        : base(message)
    {
    }

    public SeedValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
