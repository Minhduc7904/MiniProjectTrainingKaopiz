// File: backend/Services/Course/CourseService.Domain/Constants/CourseStatuses.cs
// Mục đích: Khai báo trạng thái vòng đời Course dùng thống nhất ở Domain, Application và Infrastructure.

namespace CourseService.Domain.Constants;

public static class CourseStatuses
{
    public const string Draft = "DRAFT";
    public const string Published = "PUBLISHED";
    public const string Archived = "ARCHIVED";

    public static bool IsSupported(string value) =>
        value is Draft or Published or Archived;
}
