using BuildingBlocks.Contracts.Api;
using StudentService.Application.Common.Errors;
using StudentService.Application.UseCases.Students.GetList;

namespace StudentService.UnitTests;

public sealed class GetStudentsQueryTests
{
    private static readonly string[] ExpectedInvalidFields =
    [
        "status",
        "sortBy",
        "sortDirection",
        "page",
        "pageSize",
    ];

    [Test]
    public void DefaultValuesReturnOffsetDefaults()
    {
        var query = GetStudentsQuery.Create(null, null, null, null, null);

        Assert.Multiple(() =>
        {
            Assert.That(query.Status, Is.Null);
            Assert.That(query.SortBy, Is.EqualTo(StudentSortField.CreatedAt));
            Assert.That(query.Descending, Is.True);
            Assert.That(query.Page, Is.EqualTo(1));
            Assert.That(query.PageSize, Is.EqualTo(20));
        });
    }

    [Test]
    public void ValidValuesNormalizeAllowlistedQuery()
    {
        var query = GetStudentsQuery.Create(
            " active ",
            "displayName",
            "ASC",
            2,
            100);

        Assert.Multiple(() =>
        {
            Assert.That(query.Status, Is.EqualTo("ACTIVE"));
            Assert.That(query.SortBy, Is.EqualTo(StudentSortField.DisplayName));
            Assert.That(query.Descending, Is.False);
            Assert.That(query.Page, Is.EqualTo(2));
            Assert.That(query.PageSize, Is.EqualTo(100));
        });
    }

    [Test]
    public void InvalidValuesReturnAllValidationDetails()
    {
        var exception = Assert.Throws<StudentApplicationException>(() =>
            GetStudentsQuery.Create("unknown", "id", "sideways", 0, 101));

        Assert.Multiple(() =>
        {
            Assert.That(
                exception!.ErrorCode,
                Is.EqualTo(ApiErrorCodes.ValidationFailed));
            Assert.That(
                exception.Details.Select(detail => detail.Field),
                Is.EquivalentTo(ExpectedInvalidFields));
        });
    }

    [Test]
    public void PageOffsetExceedsProviderLimitReturnsValidationError()
    {
        var exception = Assert.Throws<StudentApplicationException>(() =>
            GetStudentsQuery.Create(null, null, null, int.MaxValue, 100));

        Assert.That(
            exception!.Details,
            Has.Some.Matches<ApiErrorDetail>(detail => detail.Field == "page"));
    }
}
