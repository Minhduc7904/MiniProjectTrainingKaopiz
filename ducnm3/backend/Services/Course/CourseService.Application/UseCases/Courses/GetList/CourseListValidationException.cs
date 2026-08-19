// File: backend/Services/Course/CourseService.Application/UseCases/Courses/GetList/CourseListValidationException.cs
// Mục đích: Trả validation error chuẩn cho query list Course không hợp lệ.

using BuildingBlocks.Contracts.Api;

namespace CourseService.Application.UseCases.Courses.GetList;

public sealed class CourseListValidationException(IReadOnlyList<ApiErrorDetail> details)
    : ApiException(ApiErrorCodes.ValidationFailed, ApiErrorMessages.ValidationFailed, 400, details);
