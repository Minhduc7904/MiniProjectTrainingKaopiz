using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Lms.DataSeeder;

public sealed class DeterministicSeedData(SeedOptions options)
{
    private const ulong LessonSalt = 0x3D66_A713_5F09_CE2D;
    private const ulong EnrollmentSalt = 0x87A2_FC31_0B64_D95E;

    public StudentSeedRow CreateStudent(int studentIndex) =>
        new(
            CreateId("student", studentIndex),
            string.Create(
                CultureInfo.InvariantCulture,
                $"seed.{options.RandomSeed}.student.{studentIndex:D6}@example.test"),
            string.Create(
                CultureInfo.InvariantCulture,
                $"Seed Student {studentIndex:D6}"),
            "ACTIVE");

    public CourseSeedRow CreateCourse(int courseIndex) =>
        new(
            CreateId("course", courseIndex),
            string.Create(
                CultureInfo.InvariantCulture,
                $"Seed {options.RandomSeed} Course {courseIndex:D6}"),
            "PUBLISHED");

    public int GetLessonCount(int courseIndex) =>
        CreateRandom(LessonSalt, courseIndex).NextInclusive(
            options.MinLessonsPerCourse,
            options.MaxLessonsPerCourse);

    public LessonSeedRow CreateLesson(int courseIndex, int displayOrder) =>
        new(
            CreateId("lesson", courseIndex, displayOrder),
            CreateId("course", courseIndex),
            string.Create(
                CultureInfo.InvariantCulture,
                $"Seed {options.RandomSeed} Lesson {courseIndex:D6}-{displayOrder:D2}"),
            displayOrder);

    public IReadOnlyList<int> GetCourseIndexesForStudent(int studentIndex)
    {
        var random = CreateRandom(EnrollmentSalt, studentIndex);
        var count = random.NextInclusive(
            options.MinCoursesPerStudent,
            options.MaxCoursesPerStudent);
        var selected = new HashSet<int>();

        while (selected.Count < count)
        {
            selected.Add(random.NextInclusive(1, options.CourseCount));
        }

        return selected.Order().ToArray();
    }

    public EnrollmentSeedRow CreateEnrollment(int studentIndex, int courseIndex) =>
        new(
            CreateId("enrollment", studentIndex, courseIndex),
            CreateId("course", courseIndex),
            CreateId("student", studentIndex));

    public SeedPlan CalculatePlan()
    {
        long lessonCount = 0;
        for (var courseIndex = 1; courseIndex <= options.CourseCount; courseIndex++)
        {
            lessonCount += GetLessonCount(courseIndex);
        }

        long enrollmentCount = 0;
        for (var studentIndex = 1; studentIndex <= options.StudentCount; studentIndex++)
        {
            enrollmentCount += GetCourseIndexesForStudent(studentIndex).Count;
        }

        return new SeedPlan(
            options.StudentCount,
            options.CourseCount,
            lessonCount,
            enrollmentCount);
    }

    private Guid CreateId(string entityType, int primaryIndex, int secondaryIndex = 0)
    {
        var value = string.Create(
            CultureInfo.InvariantCulture,
            $"{options.RandomSeed}:{entityType}:{primaryIndex}:{secondaryIndex}");
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        Span<byte> bytes = stackalloc byte[16];
        hash.AsSpan(0, bytes.Length).CopyTo(bytes);
        bytes[7] = (byte)((bytes[7] & 0x0F) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        return new Guid(bytes);
    }

    private DeterministicRandom CreateRandom(ulong salt, int index)
    {
        var seed = salt ^
                   unchecked((ulong)(uint)options.RandomSeed << 32) ^
                   unchecked((ulong)(uint)index);
        return new DeterministicRandom(seed);
    }

    private sealed class DeterministicRandom(ulong state)
    {
        private ulong currentState = state;

        public int NextInclusive(int minimum, int maximum)
        {
            var range = checked((uint)(maximum - minimum + 1));
            return minimum + (int)(NextUInt64() % range);
        }

        private ulong NextUInt64()
        {
            currentState += 0x9E37_79B9_7F4A_7C15;
            var value = currentState;
            value = (value ^ (value >> 30)) * 0xBF58_476D_1CE4_E5B9;
            value = (value ^ (value >> 27)) * 0x94D0_49BB_1331_11EB;
            return value ^ (value >> 31);
        }
    }
}

public sealed record StudentSeedRow(Guid Id, string Email, string DisplayName, string Status);

public sealed record CourseSeedRow(Guid Id, string Name, string Status);

public sealed record LessonSeedRow(Guid Id, Guid CourseId, string Title, int DisplayOrder);

public sealed record EnrollmentSeedRow(Guid Id, Guid CourseId, Guid StudentId);

public sealed record SeedPlan(
    long Students,
    long Courses,
    long Lessons,
    long Enrollments)
{
    public long TotalRows => Students + Courses + Lessons + Enrollments;
}
