namespace Lms.DataSeeder;

public interface ISeedProgress
{
    void PhaseStarted(SeedPhase phase, long totalRows);

    void PhaseAdvanced(
        SeedPhase phase,
        long processedRows,
        long totalRows,
        long insertedRows,
        long skippedRows);

    void PhaseCompleted(
        SeedPhase phase,
        long totalRows,
        long insertedRows,
        long skippedRows,
        TimeSpan elapsed);

    void SchemaActionCompleted(
        SeedSchemaOperation operation,
        string objectType,
        string name,
        bool succeeded,
        string? failure);
}

public enum SeedSchemaOperation
{
    Remove,
    Keep,
    Restore,
}

public enum SeedPhase
{
    Students,
    Courses,
    Lessons,
    Enrollments,
    LessonProgresses,
    Validation,
}

public sealed class NullSeedProgress : ISeedProgress
{
    public static NullSeedProgress Instance { get; } = new();

    private NullSeedProgress()
    {
    }

    public void PhaseStarted(SeedPhase phase, long totalRows)
    {
    }

    public void PhaseAdvanced(
        SeedPhase phase,
        long processedRows,
        long totalRows,
        long insertedRows,
        long skippedRows)
    {
    }

    public void PhaseCompleted(
        SeedPhase phase,
        long totalRows,
        long insertedRows,
        long skippedRows,
        TimeSpan elapsed)
    {
    }

    public void SchemaActionCompleted(
        SeedSchemaOperation operation,
        string objectType,
        string name,
        bool succeeded,
        string? failure)
    {
    }
}

public sealed record SeedRunSummary(
    SeedPlan Plan,
    long InsertedRows,
    long SkippedRows,
    TimeSpan Elapsed);
