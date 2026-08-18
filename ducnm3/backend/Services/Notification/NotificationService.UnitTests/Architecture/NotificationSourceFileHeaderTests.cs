// File: backend/Services/Notification/NotificationService.UnitTests/Architecture/NotificationSourceFileHeaderTests.cs
// Mục đích: Bảo đảm mọi file C# Notification có comment đúng đường dẫn và mô tả trách nhiệm bằng tiếng Việt.

namespace NotificationService.UnitTests.Architecture;

[TestFixture]
public sealed class NotificationSourceFileHeaderTests
{
    [Test]
    public void AllNotificationCSharpFilesHavePathAndPurposeHeader()
    {
        var repositoryRoot = FindRepositoryRoot();
        var serviceRoot = Path.Combine(repositoryRoot, "backend", "Services", "Notification");
        var invalidFiles = Directory
            .EnumerateFiles(serviceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !HasHeader(path, repositoryRoot))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .ToArray();

        Assert.That(invalidFiles, Is.Empty, string.Join(Environment.NewLine, invalidFiles));
    }

    private static bool HasHeader(string path, string repositoryRoot)
    {
        var lines = File.ReadLines(path).Take(2).ToArray();
        var relativePath = Path.GetRelativePath(repositoryRoot, path).Replace(Path.DirectorySeparatorChar, '/');
        return lines.Length == 2 &&
            lines[0] == $"// File: {relativePath}" &&
            lines[1].StartsWith("// Mục đích: ", StringComparison.Ordinal) &&
            lines[1].Length > "// Mục đích: ".Length;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "backend", "Lms.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Không tìm thấy repository root.");
    }
}
