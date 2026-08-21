namespace StudentService.Domain.Constants;

public static class StudentStatuses
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
    public const string Blocked = "BLOCKED";

    public static bool IsSupported(string value) =>
        value is Active or Inactive or Blocked;
}
