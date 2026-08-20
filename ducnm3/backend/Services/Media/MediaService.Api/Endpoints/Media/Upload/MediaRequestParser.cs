// File: backend/Services/Media/MediaService.Api/Endpoints/Media/Upload/MediaRequestParser.cs
// Mục đích: Đọc multipart form upload Media, kiểm tra trường bắt buộc và tạo dữ liệu đầu vào hợp lệ cho use case Upload.

using MediaService.Application;

using MediaService.Application.Common.Errors;
using MediaService.Domain.ValueObjects;
using BuildingBlocks.Contracts.Api;

namespace MediaService.Api.Endpoints.Media;

public static class MediaRequestParser
{
    public static ActorReference ReadActor(HttpRequest request)
    {
        var actorType = request.Headers[ApiHeaderNames.ActorType].ToString();
        var actorId = request.Headers[ApiHeaderNames.ActorId].ToString();
        if (string.IsNullOrWhiteSpace(actorType) || string.IsNullOrWhiteSpace(actorId))
        {
            throw MediaErrors.InvalidActorType(
                $"{ApiHeaderNames.ActorType} and {ApiHeaderNames.ActorId} headers are required.");
        }

        return new ActorReference(actorType, ParseGuid(actorId, ApiHeaderNames.ActorId));
    }

    public static async Task<IFormCollection> ReadMultipartFormAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            throw MediaErrors.InvalidMedia(
                "Content-Type must be multipart/form-data.");
        }

        try
        {
            return await request.ReadFormAsync(cancellationToken);
        }
        catch (InvalidDataException exception) when (
            exception.Message.Contains(
                "length limit",
                StringComparison.OrdinalIgnoreCase))
        {
            throw MediaErrors.PayloadTooLarge();
        }
        catch (InvalidDataException)
        {
            throw MediaErrors.InvalidMedia("The multipart form is invalid.");
        }
    }

    public static string GetRequiredFormValue(
        IFormCollection form,
        string name)
    {
        var value = form[name].ToString();
        return string.IsNullOrWhiteSpace(value)
            ? throw MediaErrors.InvalidMedia($"{name} is required.")
            : value;
    }

    public static Guid ParseGuid(string value, string field) =>
        Guid.TryParse(value, out var parsed) && parsed != Guid.Empty
            ? parsed
            : throw MediaErrors.InvalidMedia(
                $"{field} must be a valid UUID.");
}
