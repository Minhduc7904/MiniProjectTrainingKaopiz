// File: backend/Services/Media/MediaService.UnitTests/TestDoubles/StubStudentLookup.cs
// Mục đích: Cung cấp test double StubStudentLookup để unit test cô lập use case khỏi dependency bên ngoài.

using BuildingBlocks.Contracts.Students;
using MediaService.Application.Services.Students;

namespace MediaService.UnitTests.TestDoubles;

public sealed class StubStudentLookup(
    Func<Guid, StudentQueryResponse?>? resolver = null) : IStudentLookup
{
    public Guid LastStudentId { get; private set; }

    public Task<StudentQueryResponse?> GetByIdAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        LastStudentId = studentId;
        var student = resolver is null
            ? new StudentQueryResponse(
                studentId,
                "student@example.com",
                "Student",
                "ACTIVE")
            : resolver(studentId);
        return Task.FromResult<StudentQueryResponse?>(student);
    }
}
