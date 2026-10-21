// File: backend/Services/Course/CourseService.Application/UseCases/LessonAi/Processing/DocumentExtractionProcessor.cs
// Mục đích: Trích xuất văn bản của một tài liệu đính kèm: TXT/MD đọc thẳng, DOCX qua parser, PDF/ảnh qua LLM (tác vụ document-extraction).

using System.Text;
using CourseService.Application.Repositories;
using CourseService.Application.Services.Ai;
using CourseService.Application.Services.Media;
using CourseService.Application.UseCases.LessonAi.Documents;

namespace CourseService.Application.UseCases.LessonAi.Processing;

public sealed class DocumentExtractionProcessor(
    IMediaContentReader mediaContentReader,
    IAiCompletionClient aiClient,
    ILessonAiRepository repository)
{
    public const string ExtractionTask = "document-extraction";
    public const long MaxFileBytes = 15L * 1024 * 1024;
    public const int MaxTextLength = 300_000;

    public const string EmptyDocumentCode = "EMPTY_DOCUMENT";
    public const string InvalidDocumentCode = "INVALID_DOCUMENT";
    public const string TruncatedNotice = "\n\n[Nội dung bị cắt do vượt giới hạn của model hoặc của hệ thống]";

    public async Task ProcessAsync(LessonAiDocumentWork work, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(work);
        try
        {
            var text = await ExtractAsync(work, cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
            {
                await repository.FailDocumentAsync(work.Id, EmptyDocumentCode, "No text could be extracted from this document.", cancellationToken);
                return;
            }

            await repository.CompleteDocumentAsync(work.Id, Truncate(text.Trim()), cancellationToken);
        }
        catch (MediaContentException exception)
        {
            await repository.FailDocumentAsync(work.Id, exception.Code, exception.Message, cancellationToken);
        }
        catch (AiCompletionException exception)
        {
            await repository.FailDocumentAsync(work.Id, exception.Code, exception.Message, cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            await repository.FailDocumentAsync(work.Id, InvalidDocumentCode, exception.Message, cancellationToken);
        }
    }

    private async Task<string> ExtractAsync(LessonAiDocumentWork work, CancellationToken cancellationToken)
    {
        var kind = LessonAiDocumentKinds.Classify(work.ContentType, work.FileName);
        if (kind == LessonAiDocumentKind.Unsupported)
        {
            throw new InvalidDataException(LessonAiDocumentKinds.UnsupportedMessage);
        }

        var content = await mediaContentReader.ReadAsync(work.MediaId, MaxFileBytes, cancellationToken);
        switch (kind)
        {
            case LessonAiDocumentKind.Text:
                return Encoding.UTF8.GetString(content).TrimStart('﻿');
            case LessonAiDocumentKind.Docx:
                return DocxTextExtractor.Extract(content);
        }

        var mediaType = LessonAiDocumentKinds.ResolveMediaType(kind, work.ContentType, work.FileName);
        var attachment = new AiContentPart(
            kind == LessonAiDocumentKind.Pdf ? "file" : "image",
            null,
            mediaType,
            Convert.ToBase64String(content),
            work.FileName);
        var completion = await aiClient.CompleteAsync(
            ExtractionTask,
            [
                new AiMessage("system", LessonAiPrompts.ExtractionSystem),
                new AiMessage("user", null, [AiContentPart.FromText("Transcribe the attached document."), attachment]),
            ],
            [],
            work.RequestedBy,
            cancellationToken);
        var text = completion.Text ?? string.Empty;
        return completion.FinishReason == "length" ? text + TruncatedNotice : text;
    }

    private static string Truncate(string text) =>
        text.Length <= MaxTextLength ? text : text[..MaxTextLength] + TruncatedNotice;
}
