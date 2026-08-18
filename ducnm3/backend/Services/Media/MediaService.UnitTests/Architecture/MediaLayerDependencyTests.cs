// File: backend/Services/Media/MediaService.UnitTests/Architecture/MediaLayerDependencyTests.cs
// Mục đích: Cung cấp thành phần phục vụ Media Service.

using MediaService.Domain.Entities;

namespace MediaService.UnitTests.Architecture;

[TestFixture]
public sealed class MediaLayerDependencyTests
{
    private static readonly HashSet<string> DisallowedAssemblyNames = new(StringComparer.Ordinal)
    {
        "MediaService.Application",
        "MediaService.Infrastructure",
        "MediaService.Api",
        "BuildingBlocks.Presentation",
        "BuildingBlocks.Http"
    };

    [Test]
    public void DomainAssemblyDoesNotReferenceInfrastructureOrTransportAssemblies()
    {
        var referencedAssemblyNames = typeof(Media)
            .Assembly
            .GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .Where(name => name is not null)
            .ToArray();

        Assert.That(
            referencedAssemblyNames,
            Has.None.Matches<string>(IsForbiddenReference));
    }

    [TestCase("MediaService.Infrastructure")]
    [TestCase("MediaService.Api")]
    [TestCase("MediaService.Application")]
    [TestCase("BuildingBlocks.Presentation")]
    [TestCase("BuildingBlocks.Http")]
    public void IsForbiddenReferenceReturnsTrueForArchitectureViolatingReference(
        string assemblyName)
    {
        Assert.That(IsForbiddenReference(assemblyName), Is.True);
    }

    private static bool IsForbiddenReference(string assemblyName) =>
        DisallowedAssemblyNames.Contains(assemblyName) ||
        assemblyName.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
        assemblyName.StartsWith("Minio", StringComparison.Ordinal) ||
        assemblyName.StartsWith("MassTransit", StringComparison.Ordinal) ||
        assemblyName.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal);
}
