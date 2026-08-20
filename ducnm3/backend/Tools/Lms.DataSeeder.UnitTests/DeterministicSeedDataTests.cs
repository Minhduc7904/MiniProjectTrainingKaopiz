namespace Lms.DataSeeder.UnitTests;

public class DeterministicSeedDataTests
{
    [Test]
    public void SameSeedAndIndexProduceSameRows()
    {
        var first = new DeterministicSeedData(CreateOptions(randomSeed: 123));
        var second = new DeterministicSeedData(CreateOptions(randomSeed: 123));

        Assert.Multiple(() =>
        {
            Assert.That(first.CreateStudent(42), Is.EqualTo(second.CreateStudent(42)));
            Assert.That(first.CreateCourse(42), Is.EqualTo(second.CreateCourse(42)));
            Assert.That(first.CreateLesson(42, 3), Is.EqualTo(second.CreateLesson(42, 3)));
            Assert.That(
                first.GetCourseIndexesForStudent(42),
                Is.EqualTo(second.GetCourseIndexesForStudent(42)));
        });
    }

    [Test]
    public void DifferentRandomSeedsProduceDifferentIdsAndAssignments()
    {
        var first = new DeterministicSeedData(CreateOptions(randomSeed: 123));
        var second = new DeterministicSeedData(CreateOptions(randomSeed: 456));

        Assert.Multiple(() =>
        {
            Assert.That(first.CreateStudent(42).Id, Is.Not.EqualTo(second.CreateStudent(42).Id));
            Assert.That(first.CreateCourse(42).Id, Is.Not.EqualTo(second.CreateCourse(42).Id));
            Assert.That(
                first.GetCourseIndexesForStudent(42),
                Is.Not.EqualTo(second.GetCourseIndexesForStudent(42)));
        });
    }

    [Test]
    public void GeneratedRelationshipsStayWithinConfiguredRangesAndRemainUnique()
    {
        var generator = new DeterministicSeedData(CreateOptions(randomSeed: 123));

        for (var courseIndex = 1; courseIndex <= 20; courseIndex++)
        {
            Assert.That(generator.GetLessonCount(courseIndex), Is.InRange(1, 5));
        }

        for (var studentIndex = 1; studentIndex <= 50; studentIndex++)
        {
            var courseIndexes = generator.GetCourseIndexesForStudent(studentIndex);
            Assert.Multiple(() =>
            {
                Assert.That(courseIndexes, Has.Count.InRange(1, 10));
                Assert.That(courseIndexes, Is.Unique);
                Assert.That(courseIndexes, Has.All.InRange(1, 20));
            });
        }
    }

    [Test]
    public void CalculatePlanMatchesGeneratedRelationshipCounts()
    {
        var generator = new DeterministicSeedData(CreateOptions(randomSeed: 123));
        var plan = generator.CalculatePlan();

        var expectedLessons = Enumerable.Range(1, 20)
            .Sum(generator.GetLessonCount);
        var expectedEnrollments = Enumerable.Range(1, 50)
            .Sum(index => generator.GetCourseIndexesForStudent(index).Count);

        Assert.Multiple(() =>
        {
            Assert.That(plan.Students, Is.EqualTo(50));
            Assert.That(plan.Courses, Is.EqualTo(20));
            Assert.That(plan.Lessons, Is.EqualTo(expectedLessons));
            Assert.That(plan.Enrollments, Is.EqualTo(expectedEnrollments));
        });
    }

    [Test]
    public void CalculatePlanWithMaximumCourseCountUsesLongTotals()
    {
        var generator = new DeterministicSeedData(CreateOptions(randomSeed: 123) with
        {
            CourseCount = SeedOptions.MaximumCourseCount,
            MinLessonsPerCourse = 5,
            MaxLessonsPerCourse = 5,
        });

        var plan = generator.CalculatePlan();

        Assert.Multiple(() =>
        {
            Assert.That(plan.Courses, Is.EqualTo(SeedOptions.MaximumCourseCount));
            Assert.That(plan.Lessons, Is.EqualTo(1_500_000L));
            Assert.That(plan.TotalRows, Is.GreaterThan(SeedOptions.MaximumCourseCount));
        });
    }

    [Test]
    public void CourseApiLargeProfilePlansOneProgressPerCourse()
    {
        var generator = new DeterministicSeedData(CreateOptions(randomSeed: 123) with
        {
            CourseApiLarge = true,
            CourseCount = 1_000,
            MinLessonsPerCourse = 1,
            MaxLessonsPerCourse = 1,
        });

        var plan = generator.CalculatePlan();

        Assert.That(plan.LessonProgresses, Is.EqualTo(1_000));
    }

    private static SeedOptions CreateOptions(int randomSeed) =>
        new(
            "Server=localhost;Database=lms_student_db;User ID=test;Password=test;",
            "Server=localhost;Database=lms_course_db;User ID=test;Password=test;",
            50,
            20,
            1,
            5,
            1,
            10,
            10,
            randomSeed,
            true,
            false,
            false);
}
