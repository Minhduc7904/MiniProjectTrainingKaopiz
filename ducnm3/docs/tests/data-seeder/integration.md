# Data Seeder Integration Tests

## Project

`backend/Tools/Lms.DataSeeder.IntegrationTests/Lms.DataSeeder.IntegrationTests.csproj`

Docker Engine is required. The test starts two isolated MySQL `8.4`
Testcontainers, one for `lms_student_db` and one for `lms_course_db`. It applies
the real Student/Course `V001` SQL migrations through `SqlMigrationRunner`.
No developer database or Docker Compose volume is read or changed.

## `RunAsyncSeedsRelationshipsAndResumeIsIdempotent`

Setup:

- Student database has only migrated `students`.
- Course database has only migrated `courses`, `lessons`, `enrollments` and
  `lesson_progresses`.
- Dataset has 30 Students, 20 Courses, `1-5` Lessons/Course and `1-10`
  Courses/Student.

Assertions:

1. First run inserts exactly 30 Students and 20 Courses.
2. Lesson and Enrollment row counts equal the deterministic plan.
3. No duplicate `(course_id, student_id)` group exists.
4. Running the same dataset with `Resume = true` inserts zero new rows and reports
   all planned rows as already present.
5. Row counts remain unchanged after resume.
6. Running again in fresh mode is rejected because target tables are not empty.

The runner's own final validation, exercised by the test, also verifies exact
counts, Lesson/Course range, Course/Student range and one logical Student
reference across the two databases.

Pass condition: the NUnit test completes without assertion, migration, MySQL
constraint or validation failure, and both containers are disposed.

Run:

```bash
dotnet test backend/Tools/Lms.DataSeeder.IntegrationTests/Lms.DataSeeder.IntegrationTests.csproj
```
