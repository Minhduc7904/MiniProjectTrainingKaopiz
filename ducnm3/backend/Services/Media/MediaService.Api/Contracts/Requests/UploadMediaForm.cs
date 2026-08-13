namespace MediaService.Api.Contracts.Requests;

public sealed record UploadMediaForm(
    IFormFile File,
    string MediaType,
    string UploadedByType,
    string UploadedBy);
