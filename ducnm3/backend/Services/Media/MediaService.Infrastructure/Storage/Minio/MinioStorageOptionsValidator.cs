using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace MediaService.Infrastructure.Storage.Minio;

public sealed partial class MinioStorageOptionsValidator : IValidateOptions<MinioStorageOptions>
{
    public ValidateOptionsResult Validate(string? name, MinioStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            failures.Add("Storage:Minio:Endpoint is required.");
        }
        else
        {
            var scheme = options.UseSsl ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            if (!Uri.TryCreate($"{scheme}://{options.Endpoint}", UriKind.Absolute, out _))
            {
                failures.Add("Storage:Minio:Endpoint must be a valid host with an optional port.");
            }
        }

        if (string.IsNullOrWhiteSpace(options.AccessKey))
        {
            failures.Add("Storage:Minio:AccessKey is required.");
        }

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            failures.Add("Storage:Minio:SecretKey is required.");
        }

        if (options.HealthTimeoutSeconds is < 1 or > 30)
        {
            failures.Add("Storage:Minio:HealthTimeoutSeconds must be between 1 and 30.");
        }

        var buckets = options.GetBuckets();
        foreach (var bucket in buckets)
        {
            if (!IsValidBucketName(bucket))
            {
                failures.Add(
                    $"MinIO bucket '{bucket}' must be 3-63 characters and contain only lowercase letters, numbers, dots, or hyphens.");
            }
        }

        if (buckets.Distinct(StringComparer.Ordinal).Count() != buckets.Count)
        {
            failures.Add("Each media category must use a distinct MinIO bucket.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsValidBucketName(string bucket) =>
        !string.IsNullOrWhiteSpace(bucket) &&
        BucketNamePattern().IsMatch(bucket) &&
        !bucket.Contains("..", StringComparison.Ordinal) &&
        !bucket.Contains(".-", StringComparison.Ordinal) &&
        !bucket.Contains("-.", StringComparison.Ordinal);

    [GeneratedRegex("^[a-z0-9][a-z0-9.-]{1,61}[a-z0-9]$", RegexOptions.CultureInvariant)]
    private static partial Regex BucketNamePattern();
}
