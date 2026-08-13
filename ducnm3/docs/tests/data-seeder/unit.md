# Data Seeder Unit Tests

## Project

`backend/Tools/Lms.DataSeeder.UnitTests/Lms.DataSeeder.UnitTests.csproj`

## Deterministic generator

Source: `DeterministicSeedDataTests.cs`.

### `SameSeedAndIndexProduceSameRows`

- Setup: two generators with identical options and random seed.
- Input: the same Student, Course, Lesson and enrollment indexes.
- Pass: complete generated rows and course assignments are equal.

### `DifferentRandomSeedsProduceDifferentIdsAndAssignments`

- Setup: two generators with different random seeds.
- Input: identical entity indexes.
- Pass: Student/Course IDs and course assignments differ, proving seed isolation.

### `GeneratedRelationshipsStayWithinConfiguredRangesAndRemainUnique`

- Setup: generate 20 Courses and assignments for 50 Students.
- Input: Lesson range `1-5`, enrollment range `1-10`.
- Pass: every count is inside its range; one Student never receives a duplicate
  Course index; every Course index exists.

### `CalculatePlanMatchesGeneratedRelationshipCounts`

- Setup: calculate the plan and independently enumerate generated relationships.
- Pass: Student/Course fixed totals and Lesson/Enrollment computed totals match.

## Safety option validation

Source: `SeedOptionsTests.cs`.

- `ValidateAcceptsConfirmedDevelopmentConfiguration`: valid Development options
  do not throw.
- `ValidateRejectsEnvironmentOtherThanDevelopment`: Production is rejected.
- `ValidateRejectsWriteWithoutConfirmation`: a write without `--confirm` is
  rejected.
- `ValidateAllowsDryRunWithoutConfirmation`: read-only dry run is allowed.
- `ValidateRejectsUnexpectedDatabaseName`: a connection not targeting
  `lms_course_db` is rejected.
- `ValidateRejectsInvalidCourseAssignmentRange`: zero or an inverted/out-of-range
  assignment range is rejected.

Run:

```bash
dotnet test backend/Tools/Lms.DataSeeder.UnitTests/Lms.DataSeeder.UnitTests.csproj
```
