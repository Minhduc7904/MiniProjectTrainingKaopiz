using System.IO.Compression;
using System.Text;
using CourseService.Application.Services.Ai;
using CourseService.Application.Services.Media;
using CourseService.Application.UseCases.LessonAi.Documents;
using CourseService.Application.UseCases.LessonAi.Processing;
using CourseService.Domain.Constants;
using CourseService.UnitTests.Support;

namespace CourseService.UnitTests.Application.LessonAi;

public sealed class DocumentExtractionTests
{
    private static readonly Guid LessonId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Actor = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [TestCase("application/pdf", "a.bin", LessonAiDocumentKind.Pdf)]
    [TestCase("application/octet-stream", "Bai-Giang.PDF", LessonAiDocumentKind.Pdf)]
    [TestCase("image/png", "a.png", LessonAiDocumentKind.Image)]
    [TestCase("application/octet-stream", "scan.JPG", LessonAiDocumentKind.Image)]
    [TestCase("text/plain; charset=utf-8", "a.txt", LessonAiDocumentKind.Text)]
    [TestCase("application/octet-stream", "notes.md", LessonAiDocumentKind.Text)]
    [TestCase("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "a.docx", LessonAiDocumentKind.Docx)]
    [TestCase("video/mp4", "a.mp4", LessonAiDocumentKind.Unsupported)]
    [TestCase("audio/mpeg", "a.mp3", LessonAiDocumentKind.Unsupported)]
    [TestCase("application/msword", "legacy.doc", LessonAiDocumentKind.Unsupported)]
    public void ClassifyMapsContentTypeAndExtensionToDocumentKind(string contentType, string fileName, LessonAiDocumentKind expected)
    {
        Assert.That(LessonAiDocumentKinds.Classify(contentType, fileName), Is.EqualTo(expected));
    }

    [Test]
    public void DocxExtractorKeepsHeadingsListsAndTables()
    {
        var docx = BuildDocx("""
            <w:p><w:pPr><w:pStyle w:val="Heading1"/></w:pPr><w:r><w:t>Giới thiệu</w:t></w:r></w:p>
            <w:p><w:r><w:t>Đoạn </w:t></w:r><w:r><w:t>văn bản.</w:t></w:r></w:p>
            <w:p><w:pPr><w:numPr><w:ilvl w:val="0"/><w:numId w:val="1"/></w:numPr></w:pPr><w:r><w:t>Mục một</w:t></w:r></w:p>
            <w:tbl>
              <w:tr><w:tc><w:p><w:r><w:t>Tên</w:t></w:r></w:p></w:tc><w:tc><w:p><w:r><w:t>Tuổi</w:t></w:r></w:p></w:tc></w:tr>
              <w:tr><w:tc><w:p><w:r><w:t>An</w:t></w:r></w:p></w:tc><w:tc><w:p><w:r><w:t>20</w:t></w:r></w:p></w:tc></w:tr>
            </w:tbl>
            """);

        var markdown = DocxTextExtractor.Extract(docx);

        Assert.Multiple(() =>
        {
            Assert.That(markdown, Does.Contain("# Giới thiệu"));
            Assert.That(markdown, Does.Contain("Đoạn văn bản."));
            Assert.That(markdown, Does.Contain("- Mục một"));
            Assert.That(markdown, Does.Contain("| Tên | Tuổi |"));
            Assert.That(markdown, Does.Contain("| An | 20 |"));
        });
    }

    [Test]
    public void DocxExtractorRejectsFilesThatAreNotDocx()
    {
        Assert.Throws<InvalidDataException>(() => DocxTextExtractor.Extract(Encoding.UTF8.GetBytes("plain text, not a zip")));
    }

    [Test]
    public async Task TextFileIsReadDirectlyWithoutCallingTheModel()
    {
        var (store, media, ai, processor, work) = Arrange("notes.md", "text/markdown", Encoding.UTF8.GetBytes("﻿# Ghi chú\nNội dung"));

        await processor.ProcessAsync(work, CancellationToken.None);

        var document = store.Documents.Single();
        Assert.Multiple(() =>
        {
            Assert.That(document.Status, Is.EqualTo(LessonAiDocumentStatuses.Extracted));
            Assert.That(store.Texts[document.Id], Is.EqualTo("# Ghi chú\nNội dung"));
            Assert.That(ai.Requests, Is.Empty);
            Assert.That(media.Files, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task DocxFileIsParsedLocallyWithoutCallingTheModel()
    {
        var docx = BuildDocx("<w:p><w:r><w:t>Xin chào</w:t></w:r></w:p>");
        var (store, _, ai, processor, work) = Arrange("a.docx", "application/octet-stream", docx);

        await processor.ProcessAsync(work, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(store.Documents.Single().Status, Is.EqualTo(LessonAiDocumentStatuses.Extracted));
            Assert.That(store.Texts.Values.Single(), Is.EqualTo("Xin chào"));
            Assert.That(ai.Requests, Is.Empty);
        });
    }

    [Test]
    public async Task PdfIsSentToTheExtractionTaskAsAFilePartWithTheRequestingAdmin()
    {
        var pdf = Encoding.ASCII.GetBytes("%PDF-1.4 fake");
        var (store, _, ai, processor, work) = Arrange("slide.pdf", "application/pdf", pdf, ScriptedAiCompletionClient.Text("# Slide 1\nNội dung"));

        await processor.ProcessAsync(work, CancellationToken.None);

        var request = ai.Requests.Single();
        var attachment = request.Messages[^1].Parts!.Single(part => part.Type != "text");
        Assert.Multiple(() =>
        {
            Assert.That(request.Task, Is.EqualTo("document-extraction"));
            Assert.That(request.Tools, Is.Empty);
            Assert.That(request.ActorId, Is.EqualTo(work.RequestedBy));
            Assert.That(attachment.Type, Is.EqualTo("file"));
            Assert.That(attachment.MediaType, Is.EqualTo("application/pdf"));
            Assert.That(Convert.FromBase64String(attachment.DataBase64!), Is.EqualTo(pdf));
            Assert.That(store.Documents.Single().Status, Is.EqualTo(LessonAiDocumentStatuses.Extracted));
            Assert.That(store.Texts.Values.Single(), Is.EqualTo("# Slide 1\nNội dung"));
        });
    }

    [Test]
    public async Task ImageIsSentAsAnImagePart()
    {
        var (_, _, ai, processor, work) = Arrange("scan.png", "image/png", [1, 2, 3], ScriptedAiCompletionClient.Text("chữ trong ảnh"));

        await processor.ProcessAsync(work, CancellationToken.None);

        Assert.That(ai.Requests.Single().Messages[^1].Parts!.Single(part => part.Type != "text").Type, Is.EqualTo("image"));
    }

    [Test]
    public async Task ModelOutputCutByTokenLimitGetsATruncationNotice()
    {
        var (store, _, _, processor, work) = Arrange("a.pdf", "application/pdf", [1], ScriptedAiCompletionClient.Text("phần đầu", "length"));

        await processor.ProcessAsync(work, CancellationToken.None);

        Assert.That(store.Texts.Values.Single(), Does.StartWith("phần đầu").And.EndsWith(DocumentExtractionProcessor.TruncatedNotice));
    }

    [Test]
    public async Task EmptyModelOutputFailsTheDocument()
    {
        var (store, _, _, processor, work) = Arrange("a.pdf", "application/pdf", [1], ScriptedAiCompletionClient.Text("   "));

        await processor.ProcessAsync(work, CancellationToken.None);

        var document = store.Documents.Single();
        Assert.Multiple(() =>
        {
            Assert.That(document.Status, Is.EqualTo(LessonAiDocumentStatuses.Failed));
            Assert.That(document.ErrorCode, Is.EqualTo(DocumentExtractionProcessor.EmptyDocumentCode));
        });
    }

    [Test]
    public async Task InvalidDocxFailsTheDocumentWithInvalidDocumentCode()
    {
        var (store, _, _, processor, work) = Arrange("a.docx", "application/octet-stream", Encoding.UTF8.GetBytes("not zip"));

        await processor.ProcessAsync(work, CancellationToken.None);

        Assert.That(store.Documents.Single().ErrorCode, Is.EqualTo(DocumentExtractionProcessor.InvalidDocumentCode));
    }

    [Test]
    public async Task MediaFailureIsStoredOnTheDocumentUsingTheMediaErrorCode()
    {
        var (store, media, _, processor, work) = Arrange("a.txt", "text/plain", []);
        media.Failures[work.MediaId] = new MediaContentException(MediaContentException.TooLarge, "too big");

        await processor.ProcessAsync(work, CancellationToken.None);

        var document = store.Documents.Single();
        Assert.Multiple(() =>
        {
            Assert.That(document.Status, Is.EqualTo(LessonAiDocumentStatuses.Failed));
            Assert.That(document.ErrorCode, Is.EqualTo(MediaContentException.TooLarge));
        });
    }

    [Test]
    public async Task AiServiceFailureIsStoredOnTheDocumentUsingTheAiErrorCode()
    {
        var store = new InMemoryLessonAiStore();
        var media = new FakeMediaContentReader();
        var work = Queue(store, media, "a.pdf", "application/pdf", [1]);
        var processor = new DocumentExtractionProcessor(media, new ThrowingAiCompletionClient(new AiCompletionException("AI_BINDING_NOT_FOUND", "No model bound.")), store);

        await processor.ProcessAsync(work, CancellationToken.None);

        var document = store.Documents.Single();
        Assert.Multiple(() =>
        {
            Assert.That(document.Status, Is.EqualTo(LessonAiDocumentStatuses.Failed));
            Assert.That(document.ErrorCode, Is.EqualTo("AI_BINDING_NOT_FOUND"));
        });
    }

    private static (InMemoryLessonAiStore Store, FakeMediaContentReader Media, ScriptedAiCompletionClient Ai, DocumentExtractionProcessor Processor, CourseService.Application.Repositories.LessonAiDocumentWork Work) Arrange(
        string fileName,
        string contentType,
        byte[] content,
        params AiCompletion[] modelReplies)
    {
        var store = new InMemoryLessonAiStore();
        var media = new FakeMediaContentReader();
        var ai = new ScriptedAiCompletionClient(modelReplies.Select(reply => (Func<AiCompletion>)(() => reply)).ToArray());
        var work = Queue(store, media, fileName, contentType, content);
        return (store, media, ai, new DocumentExtractionProcessor(media, ai, store), work);
    }

    private static CourseService.Application.Repositories.LessonAiDocumentWork Queue(InMemoryLessonAiStore store, FakeMediaContentReader media, string fileName, string contentType, byte[] content)
    {
        var mediaId = Guid.NewGuid();
        media.Files[mediaId] = content;
        store.AddMissingDocumentsAsync(LessonId, [new(mediaId, fileName, contentType, LessonAiDocumentStatuses.Pending, null, null)], Actor, CancellationToken.None).GetAwaiter().GetResult();
        return store.ClaimNextDocumentAsync(TimeSpan.FromMinutes(1), 3, CancellationToken.None).GetAwaiter().GetResult()!;
    }

    private static byte[] BuildDocx(string bodyXml)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("word/document.xml");
            using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            writer.Write($"<?xml version=\"1.0\" encoding=\"UTF-8\"?><w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body>{bodyXml}</w:body></w:document>");
        }

        return stream.ToArray();
    }
}
