// File: backend/Services/Media/MediaService.UnitTests/Architecture/MediaSourceFileHeaderTests.cs
// Mục đích: Kiểm thử mọi file C# của Media Service có header tiếng Việt mô tả đường dẫn và mục đích cụ thể.

using NUnit.Framework;

namespace MediaService.UnitTests.Architecture;

[TestFixture]
public sealed class MediaSourceFileHeaderTests
{
    [Test]
    public void AllMediaCSharpFilesHaveVietnamesePathAndPurposeHeader()
    {
        var repositoryRoot = FindRepositoryRoot();
        var mediaRoot = Path.Combine(repositoryRoot, "backend", "Services", "Media");
        var invalidFiles = Directory
            .EnumerateFiles(mediaRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !HasRequiredHeader(path, repositoryRoot))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.That(invalidFiles, Is.Empty, string.Join(Environment.NewLine, invalidFiles));
    }

    private static bool HasRequiredHeader(string filePath, string repositoryRoot)
    {
        var lines = File.ReadLines(filePath).Take(2).ToArray();
        var relativePath = Path.GetRelativePath(repositoryRoot, filePath)
            .Replace(Path.DirectorySeparatorChar, '/');
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

        throw new DirectoryNotFoundException("Không tìm thấy thư mục gốc của repository.");
    }
}
