using CourseService.Application.Repositories;
using CourseService.Application.UseCases.Quizzes;
using CourseService.Domain.Constants;

namespace CourseService.UnitTests.Application.Quizzes;

public sealed class QuizGraderTests
{
    private static readonly Guid SingleQuestionId = Guid.NewGuid();
    private static readonly Guid MultipleQuestionId = Guid.NewGuid();
    private static readonly Guid SingleCorrect = Guid.NewGuid();
    private static readonly Guid SingleWrong = Guid.NewGuid();
    private static readonly Guid[] MultipleCorrect = [Guid.NewGuid(), Guid.NewGuid()];
    private static readonly Guid MultipleWrong = Guid.NewGuid();

    private static QuizRecord Quiz() => new(
        Guid.NewGuid(), Guid.NewGuid(), "Quiz", null, 1, QuizStatuses.Published, DateTime.UtcNow, DateTime.UtcNow, false,
        [
            new QuizQuestionRecord(SingleQuestionId, "Q1", QuizQuestionTypes.SingleChoice, 1.5m, 1,
                [new(SingleCorrect, "A", true, 1), new(SingleWrong, "B", false, 2)]),
            new QuizQuestionRecord(MultipleQuestionId, "Q2", QuizQuestionTypes.MultipleChoice, 2.25m, 2,
                [new(MultipleCorrect[0], "A", true, 1), new(MultipleCorrect[1], "B", true, 2), new(MultipleWrong, "C", false, 3)]),
        ]);

    [Test]
    public void Grade_AllCorrect_ReturnsFullScore()
    {
        var grade = QuizGrader.Grade(Quiz(), [
            new(SingleQuestionId, SingleCorrect),
            new(MultipleQuestionId, MultipleCorrect[0]),
            new(MultipleQuestionId, MultipleCorrect[1]),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(grade.Score, Is.EqualTo(3.75m));
            Assert.That(grade.MaxScore, Is.EqualTo(3.75m));
            Assert.That(grade.Questions.All(question => question.Correct), Is.True);
        });
    }

    [Test]
    public void Grade_SingleChoiceWrong_ScoresZeroForThatQuestion()
    {
        var grade = QuizGrader.Grade(Quiz(), [
            new(SingleQuestionId, SingleWrong),
            new(MultipleQuestionId, MultipleCorrect[0]),
            new(MultipleQuestionId, MultipleCorrect[1]),
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(grade.Score, Is.EqualTo(2.25m));
            Assert.That(grade.Questions[0].Correct, Is.False);
            Assert.That(grade.Questions[0].Answered, Is.True);
        });
    }

    [Test]
    public void Grade_MultipleChoiceMissingOneCorrect_ScoresZero()
    {
        var grade = QuizGrader.Grade(Quiz(), [new(MultipleQuestionId, MultipleCorrect[0])]);

        Assert.That(grade.Questions[1].PointsEarned, Is.Zero);
    }

    [Test]
    public void Grade_MultipleChoiceWithExtraWrongOption_ScoresZero()
    {
        var grade = QuizGrader.Grade(Quiz(), [
            new(MultipleQuestionId, MultipleCorrect[0]),
            new(MultipleQuestionId, MultipleCorrect[1]),
            new(MultipleQuestionId, MultipleWrong),
        ]);

        Assert.That(grade.Questions[1].PointsEarned, Is.Zero);
    }

    [Test]
    public void Grade_NoAnswers_ScoresZeroAndKeepsMaxScore()
    {
        var grade = QuizGrader.Grade(Quiz(), []);

        Assert.Multiple(() =>
        {
            Assert.That(grade.Score, Is.Zero);
            Assert.That(grade.MaxScore, Is.EqualTo(3.75m));
            Assert.That(grade.Questions.All(question => !question.Answered), Is.True);
        });
    }

    [Test]
    public void Grade_DuplicateSelections_AreCountedOnce()
    {
        var grade = QuizGrader.Grade(Quiz(), [new(SingleQuestionId, SingleCorrect), new(SingleQuestionId, SingleCorrect)]);

        Assert.That(grade.Questions[0].Correct, Is.True);
    }

    [Test]
    public void Grade_ReturnsCorrectOptionIdsForReview()
    {
        var grade = QuizGrader.Grade(Quiz(), []);

        Assert.Multiple(() =>
        {
            Assert.That(grade.Questions[0].CorrectOptionIds, Is.EqualTo(new[] { SingleCorrect }));
            Assert.That(grade.Questions[1].CorrectOptionIds, Is.EquivalentTo(MultipleCorrect));
        });
    }
}
