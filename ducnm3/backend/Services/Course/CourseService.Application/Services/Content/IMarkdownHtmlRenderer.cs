namespace CourseService.Application.Services.Content;

public interface IMarkdownHtmlRenderer
{
    string? Render(string? markdown);
}
