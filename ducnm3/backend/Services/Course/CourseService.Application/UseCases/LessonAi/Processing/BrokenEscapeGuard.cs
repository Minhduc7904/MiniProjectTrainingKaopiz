// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Processing/BrokenEscapeGuard.cs
// Mục đích: Phát hiện văn bản do model sinh ra chứa ký tự điều khiển, dấu hiệu LaTeX viết sai escape trong JSON (\times, \frac, \beta thành TAB, FF, BS).

using System.Buffers;

namespace CourseService.Application.UseCases.LessonAi.Processing;

internal static class BrokenEscapeGuard
{
    // BEL, BS, TAB, VT, FF: kết quả của \a \b \t \v \f khi dấu gạch chéo LaTeX không được nhân đôi trong JSON.
    private static readonly SearchValues<char> ControlCharacters = SearchValues.Create("\u0007\u0008\u0009\u000B\u000C");

    public const string Message =
        "The text contains control characters (tab, form feed or backspace), usually because a LaTeX backslash was not doubled inside the JSON string. " +
        "Write every LaTeX backslash as \\\\ (for example \\\\frac, \\\\times, \\\\beta, \\\\int) and call the tool again.";

    public static bool HasBrokenEscape(string? text) =>
        text is not null && text.AsSpan().IndexOfAny(ControlCharacters) >= 0;
}
