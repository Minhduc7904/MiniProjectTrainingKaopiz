export function canLoadSelectedLessonDetail(course, courseId, selectedLessonId) {
  return Boolean(
    selectedLessonId
    && course?.id === courseId
    && course.lessons?.some((lesson) => lesson.id === selectedLessonId),
  )
}
