using BuildingBlocks.Contracts.Api;
using CourseService.Application.Common.Errors;
using CourseService.Application.UseCases.Quizzes;
using CourseService.Domain.Constants;

namespace CourseService.UnitTests.Application.Quizzes;

public sealed class QuizValidationTests
{
    private static QuizQuestionInput Single(string? markdown = "Câu hỏi", decimal? points = 1m, params QuizOptionInput[] options) =>
        new(markdown, QuizQuestionTypes.SingleChoice, points, options.Length == 0 ? [new("A", true), new("B", false)] : options);

    private static QuizInput Input(params QuizQuestionInput[] questions) => new("Quiz hợp lệ", null, null, questions);

    private static string[] Fields(TestDelegate action)
    {
        var exception = Assert.Throws<CourseApplicationException>(action)!;
        Assert.That(exception.ErrorCode, Is.EqualTo(ApiErrorCodes.ValidationFailed));
        return exception.Details?.Select(detail => detail.Field).ToArray() ?? [];
    }

    [Test]
    public void Normalize_ValidInput_TrimsAndDefaultsToDraft()
    {
        var draft = QuizValidation.Normalize(new QuizInput("  Quiz  ", "  ", null, [Single("  Câu 1  ")]), QuizStatuses.Draft, 0);

        Assert.Multiple(() =>
        {
            Assert.That(draft.Title, Is.EqualTo("Quiz"));
            Assert.That(draft.DescriptionMarkdown, Is.Null);
            Assert.That(draft.Status, Is.EqualTo(QuizStatuses.Draft));
            Assert.That(draft.Questions!.Single().QuestionMarkdown, Is.EqualTo("Câu 1"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("a")]
    public void Normalize_BadTitle_Fails(string? title) =>
        Assert.That(Fields(() => QuizValidation.Normalize(new QuizInput(title, null, null, []), QuizStatuses.Draft, 0)), Contains.Item("title"));

    [Test]
    public void Normalize_PublishWithoutQuestions_Fails() =>
        Assert.That(
            Fields(() => QuizValidation.Normalize(new QuizInput("Quiz", null, QuizStatuses.Published, []), QuizStatuses.Draft, 0)),
            Contains.Item("status"));

    [Test]
    public void Normalize_PublishKeepingExistingQuestions_Succeeds()
    {
        var draft = QuizValidation.Normalize(new QuizInput("Quiz", null, QuizStatuses.Published, null), null, existingQuestionCount: 3);

        Assert.Multiple(() =>
        {
            Assert.That(draft.Status, Is.EqualTo(QuizStatuses.Published));
            Assert.That(draft.Questions, Is.Null);
        });
    }

    [Test]
    public void Normalize_UnknownStatus_Fails() =>
        Assert.That(Fields(() => QuizValidation.Normalize(new QuizInput("Quiz", null, "ARCHIVED", []), null, 0)), Contains.Item("status"));

    [Test]
    public void Normalize_EmptyQuestionMarkdown_Fails() =>
        Assert.That(Fields(() => QuizValidation.Normalize(Input(Single("  ")), null, 0)), Contains.Item("questions[0].questionMarkdown"));

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(101)]
    [TestCase(1.234)]
    public void Normalize_BadPoints_Fails(double points) =>
        Assert.That(Fields(() => QuizValidation.Normalize(Input(Single(points: (decimal)points)), null, 0)), Contains.Item("questions[0].points"));

    [Test]
    public void Normalize_SingleChoiceWithTwoCorrect_Fails() =>
        Assert.That(
            Fields(() => QuizValidation.Normalize(Input(Single(options: [new("A", true), new("B", true)])), null, 0)),
            Contains.Item("questions[0].options"));

    [Test]
    public void Normalize_MultipleChoiceWithoutCorrect_Fails()
    {
        var question = new QuizQuestionInput("Q", QuizQuestionTypes.MultipleChoice, 1m, [new("A", false), new("B", false)]);

        Assert.That(Fields(() => QuizValidation.Normalize(Input(question), null, 0)), Contains.Item("questions[0].options"));
    }

    [Test]
    public void Normalize_MultipleChoiceWithSeveralCorrect_Succeeds()
    {
        var question = new QuizQuestionInput("Q", QuizQuestionTypes.MultipleChoice, 2.5m, [new("A", true), new("B", true), new("C", false)]);

        var draft = QuizValidation.Normalize(Input(question), QuizStatuses.Draft, 0);

        Assert.That(draft.Questions!.Single().Options.Count(option => option.IsCorrect), Is.EqualTo(2));
    }

    [Test]
    public void Normalize_TooFewOptions_Fails() =>
        Assert.That(Fields(() => QuizValidation.Normalize(Input(Single(options: [new("A", true)])), null, 0)), Contains.Item("questions[0].options"));

    [Test]
    public void Normalize_EmptyOptionMarkdown_Fails() =>
        Assert.That(
            Fields(() => QuizValidation.Normalize(Input(Single(options: [new("A", true), new(" ", false)])), null, 0)),
            Contains.Item("questions[0].options[1].optionMarkdown"));

    [Test]
    public void Normalize_UnknownQuestionType_Fails()
    {
        var question = new QuizQuestionInput("Q", "ESSAY", 1m, [new("A", true), new("B", false)]);

        Assert.That(Fields(() => QuizValidation.Normalize(Input(question), null, 0)), Contains.Item("questions[0].questionType"));
    }

    [Test]
    public void Normalize_MoreThanMaxQuestions_Fails()
    {
        var questions = Enumerable.Range(0, QuizValidation.MaxQuestions + 1).Select(_ => Single()).ToArray();

        Assert.That(Fields(() => QuizValidation.Normalize(Input(questions), null, 0)), Contains.Item("questions"));
    }

    [Test]
    public void CombineMarkdown_JoinsDescriptionQuestionsAndOptions()
    {
        var draft = QuizValidation.Normalize(new QuizInput("Quiz", "Mô tả", null, [Single("Câu", options: [new("![a](/media/api/media/x/content)", true), new("B", false)])]), QuizStatuses.Draft, 0);

        var combined = QuizValidation.CombineMarkdown(draft.DescriptionMarkdown, draft.Questions);

        Assert.That(combined, Does.Contain("Mô tả").And.Contain("Câu").And.Contain("/media/api/media/x/content"));
    }
}
