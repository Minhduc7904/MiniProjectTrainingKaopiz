export function createLessonDeleteConfirmation(lesson) {
  return {
    kind: 'lesson',
    lessonId: lesson.id,
    title: 'Xóa lesson?',
    description: `Lesson “${lesson.title}” và media usage liên quan sẽ bị xóa.`,
  }
}
