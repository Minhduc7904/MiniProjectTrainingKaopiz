// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Processing/BrokenEscapeGuard.cs
// Mục đích: Phát hiện văn bản do model sinh ra chứa ký tự điều khiển (trừ xuống dòng), dấu hiệu LaTeX viết sai escape trong JSON (\times, \frac, \beta thành TAB, FF, BS).

using System.Buffers;

namespace CourseService.Application.UseCases.LessonAi.Processing;

internal static class BrokenEscapeGuard
{
    // Mọi ký tự điều khiển C0 trừ LF và CR (xuống dòng hợp lệ). Gồm BEL, BS, TAB, VT, FF do \a \b \t \v \f khi dấu gạch chéo LaTeX
    // không được nhân đôi trong JSON, và các ký tự lạ khác (ví dụ ESC U+001B xuất hiện trong lần đánh giá AI-08).
    private static readonly SearchValues<char> ControlCharacters = SearchValues.Create(
        string.Concat(Enumerable.Range(0, 0x20).Where(code => code is not '\n' and not '\r').Select(code => (char)code)));

    public const string Message =
        "The text contains control characters (tab, form feed, backspace or other non-printable characters), usually because a LaTeX backslash was not doubled inside the JSON string. " +
        "Write every LaTeX backslash as \\\\ (for example \\\\frac, \\\\times, \\\\beta, \\\\int) and call the tool again.";

    public static bool HasBrokenEscape(string? text) =>
        text is not null && text.AsSpan().IndexOfAny(ControlCharacters) >= 0;
}
