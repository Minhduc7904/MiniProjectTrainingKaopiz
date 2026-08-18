// File: backend/Services/Media/MediaService.Application/UseCases/Media/DirectUpload/DirectUploadChecksum.cs
// Mục đích: Chuẩn hóa và kiểm tra SHA-256 do client gửi trước khi tạo upload intent hoặc xác minh file.

using System.Text.RegularExpressions;

using MediaService.Application.Common.Errors;

namespace MediaService.Application.UseCases.Media.DirectUpload;

public static partial class DirectUploadChecksum
{
    public const string MetadataKey = "checksum-sha256";

    public static string Validate(string checksum)
    {
        if (string.IsNullOrWhiteSpace(checksum) || !ChecksumPattern().IsMatch(checksum))
        {
            throw MediaErrors.InvalidMedia(
                "checksumSha256 must contain exactly 64 lowercase hexadecimal characters.");
        }

        return checksum;
    }

    [GeneratedRegex("^[0-9a-f]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex ChecksumPattern();
}
