// File: backend/Services/Notification/NotificationService.UnitTests/Architecture/NotificationArchitectureTests.cs
// Mục đích: Bảo vệ namespace, dependency layer và cấu trúc mỗi route một endpoint của Notification Service.

using NotificationService.Domain.Constants;

namespace NotificationService.UnitTests.Architecture;

[TestFixture]
public sealed class NotificationArchitectureTests
{
    private static readonly string[] LegacyNamespaceFragments =
    [
        "NotificationService.Application.Features",
        "NotificationService.Application.Abstractions;",
        "NotificationService.Application.Content;",
        "NotificationService.Domain.Notifications;",
        "NotificationService.Infrastructure.Persistence;",
        "NotificationService.Infrastructure.Sending;",
        "NotificationService.Api.Contracts.Requests",
        "NotificationService.Api.Contracts.Responses",
        "NotificationService.Api.Endpoints;",
    ];

    [Test]
    public void SourceFilesDoNotUseLegacyNamespaces()
    {
        var notificationRoot = FindNotificationRoot();
        var violatingFiles = Directory
            .EnumerateFiles(notificationRoot, "*.cs", SearchOption.AllDirectories)
            .Where(IsSourceFile)
            .Where(path => !path.EndsWith(nameof(NotificationArchitectureTests) + ".cs", StringComparison.Ordinal))
            .Where(path => LegacyNamespaceFragments.Any(fragment =>
                File.ReadAllText(path).Contains(fragment, StringComparison.Ordinal)))
            .Select(path => Path.GetRelativePath(notificationRoot, path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.That(violatingFiles, Is.Empty, string.Join(Environment.NewLine, violatingFiles));
    }

    [Test]
    public void DomainAssemblyDoesNotReferenceOuterLayersOrFrameworkAdapters()
    {
        var forbidden = new HashSet<string>(StringComparer.Ordinal)
        {
            "NotificationService.Application",
            "NotificationService.Infrastructure",
            "NotificationService.Api",
            "BuildingBlocks.Presentation",
            "BuildingBlocks.Http",
        };
        var references = typeof(NotificationStatuses).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.That(references, Has.None.Matches<string>(name =>
            forbidden.Contains(name) ||
            name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
            name.StartsWith("MassTransit", StringComparison.Ordinal) ||
            name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)));
    }

    [TestCase("Api/Endpoints/NotificationBatches/Create/CreateNotificationBatchEndpoint.cs")]
    [TestCase("Api/Endpoints/NotificationBatches/GetById/GetNotificationBatchByIdEndpoint.cs")]
    [TestCase("Api/Endpoints/NotificationBatches/GetFailedItems/GetNotificationBatchFailedItemsEndpoint.cs")]
    [TestCase("Api/Endpoints/Notifications/Create/CreateNotificationEndpoint.cs")]
    [TestCase("Api/Endpoints/Notifications/GetById/GetNotificationByIdEndpoint.cs")]
    public void EveryRouteHasDedicatedEndpointFile(string relativePath)
    {
        var serviceRoot = FindNotificationRoot();
        Assert.That(File.Exists(Path.Combine(serviceRoot, $"NotificationService.{relativePath}")), Is.True);
    }

    private static bool IsSourceFile(string path) =>
        !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
        !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

    private static string FindNotificationRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "backend", "Services", "Notification");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Không tìm thấy Notification Service root.");
    }
}
