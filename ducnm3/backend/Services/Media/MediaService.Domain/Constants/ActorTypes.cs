// File: backend/Services/Media/MediaService.Domain/Constants/ActorTypes.cs
// Mục đích: Khai báo các loại actor thực hiện thao tác Media để kiểm tra quyền và audit.

namespace MediaService.Domain.Constants;

public static class ActorTypes
{
    public const string Admin = "ADMIN";
    public const string Student = "STUDENT";
}
