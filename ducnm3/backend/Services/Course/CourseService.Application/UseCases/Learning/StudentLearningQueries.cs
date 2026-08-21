using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;

namespace CourseService.Application.UseCases.Learning;

public interface IStudentLearningRepository
{
    Task<StudentEnrollmentsResult> GetEnrollmentsAsync(Guid studentId, GetStudentEnrollmentsQuery query, CancellationToken cancellationToken);
    Task<StudentCourseCatalogResult> GetCatalogAsync(Guid studentId, GetStudentCourseCatalogQuery query, CancellationToken cancellationToken);
    Task<bool> IsEnrolledAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken);
    Task<StudentCourseDetailResult?> GetDetailAsync(Guid courseId, CancellationToken cancellationToken);
    Task<StudentCourseProgressResult?> GetProgressAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken);
}

public sealed record GetStudentEnrollmentsQuery
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 12;
    public const int MaximumPageSize = 50;

    private GetStudentEnrollmentsQuery(int page, int pageSize) => (Page, PageSize) = (page, pageSize);

    public int Page { get; }
    public int PageSize { get; }

    public static GetStudentEnrollmentsQuery Create(int? page, int? pageSize)
    {
        var normalizedPage = page ?? DefaultPage;
        var normalizedPageSize = pageSize ?? DefaultPageSize;
        var details = new List<ApiErrorDetail>();
        if (normalizedPage < 1) details.Add(new ApiErrorDetail("page", "Page must be greater than or equal to 1."));
        if (normalizedPageSize is < 1 or > MaximumPageSize)
            details.Add(new ApiErrorDetail("pageSize", $"Page size must be between 1 and {MaximumPageSize}."));
        if (((long)normalizedPage - 1) * normalizedPageSize > int.MaxValue)
            details.Add(new ApiErrorDetail("page", "The requested page is outside the supported range."));
        if (details.Count > 0) throw CourseErrors.ValidationFailed(details);
        return new GetStudentEnrollmentsQuery(normalizedPage, normalizedPageSize);
    }
}

public sealed record StudentEnrollmentListItem(
    Guid EnrollmentId,
    Guid CourseId,
    string CourseName,
    string CourseStatus,
    DateTime CourseCreatedAtUtc,
    DateTime EnrolledAtUtc);

public sealed record StudentEnrollmentsResult(
    IReadOnlyList<StudentEnrollmentListItem> Items,
    long TotalItems,
    int TotalPages);

public sealed record GetStudentCourseCatalogQuery
{
    public const int DefaultPage = GetStudentEnrollmentsQuery.DefaultPage;
    public const int DefaultPageSize = GetStudentEnrollmentsQuery.DefaultPageSize;
    public const int MaximumPageSize = GetStudentEnrollmentsQuery.MaximumPageSize;

    private GetStudentCourseCatalogQuery(int page, int pageSize) => (Page, PageSize) = (page, pageSize);

    public int Page { get; }
    public int PageSize { get; }

    public static GetStudentCourseCatalogQuery Create(int? page, int? pageSize)
    {
        var normalizedPage = page ?? DefaultPage;
        var normalizedPageSize = pageSize ?? DefaultPageSize;
        var details = new List<ApiErrorDetail>();
        if (normalizedPage < 1) details.Add(new ApiErrorDetail("page", "Page must be greater than or equal to 1."));
        if (normalizedPageSize is < 1 or > MaximumPageSize)
            details.Add(new ApiErrorDetail("pageSize", $"Page size must be between 1 and {MaximumPageSize}."));
        if (((long)normalizedPage - 1) * normalizedPageSize > int.MaxValue)
            details.Add(new ApiErrorDetail("page", "The requested page is outside the supported range."));
        if (details.Count > 0) throw CourseErrors.ValidationFailed(details);
        return new GetStudentCourseCatalogQuery(normalizedPage, normalizedPageSize);
    }
}

public sealed record StudentCourseCatalogItem(Guid CourseId, string Name, string Status, DateTime CreatedAtUtc);

public sealed record StudentCourseCatalogResult(IReadOnlyList<StudentCourseCatalogItem> Items, long TotalItems, int TotalPages);

public sealed record StudentLessonPreview(Guid Id, string Title, uint DisplayOrder);

public sealed record StudentCourseDetailResult(
    Guid Id,
    string Name,
    string? DescriptionMarkdown,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<StudentLessonPreview> Lessons);

public sealed record StudentCourseProgressResult(
    Guid CourseId,
    int TotalLessons,
    int CompletedLessons,
    decimal ProgressPercent,
    StudentLessonPreview? NextLesson);

public sealed class GetStudentEnrollmentsHandler(IStudentLearningRepository repository)
{
    public Task<StudentEnrollmentsResult> HandleAsync(Guid studentId, GetStudentEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (studentId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        return repository.GetEnrollmentsAsync(studentId, query, cancellationToken);
    }
}

public sealed class GetStudentCourseCatalogHandler(IStudentLearningRepository repository)
{
    public Task<StudentCourseCatalogResult> HandleAsync(Guid studentId, GetStudentCourseCatalogQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (studentId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        return repository.GetCatalogAsync(studentId, query, cancellationToken);
    }
}

public sealed class GetStudentEnrollmentDetailHandler(IStudentLearningRepository repository)
{
    public async Task<StudentCourseDetailResult> HandleAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || studentId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        if (!await repository.IsEnrolledAsync(courseId, studentId, cancellationToken)) throw StudentNotEnrolled();
        return await repository.GetDetailAsync(courseId, cancellationToken) ?? throw CourseErrors.CourseNotFound();
    }

    private static CourseApplicationException StudentNotEnrolled() =>
        new("STUDENT_NOT_ENROLLED", "Student is not enrolled in this course.", 403);
}

public sealed class GetMyCourseProgressHandler(IStudentLearningRepository repository)
{
    public async Task<StudentCourseProgressResult> HandleAsync(Guid courseId, Guid studentId, CancellationToken cancellationToken)
    {
        if (courseId == Guid.Empty || studentId == Guid.Empty) throw CourseErrors.ValidationFailed([]);
        if (!await repository.IsEnrolledAsync(courseId, studentId, cancellationToken)) throw StudentNotEnrolled();
        return await repository.GetProgressAsync(courseId, studentId, cancellationToken) ?? throw CourseErrors.CourseNotFound();
    }

    private static CourseApplicationException StudentNotEnrolled() =>
        new("STUDENT_NOT_ENROLLED", "Student is not enrolled in this course.", 403);
}
