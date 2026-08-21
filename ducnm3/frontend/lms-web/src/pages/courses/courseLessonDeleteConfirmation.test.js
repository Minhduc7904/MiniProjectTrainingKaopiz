import { describe, expect, it } from 'vitest'
import { createLessonDeleteConfirmation } from '@/pages/courses/courseLessonDeleteConfirmation'

describe('createLessonDeleteConfirmation', () => {
  it('creates a delete confirmation for the selected lesson', () => {
    expect(createLessonDeleteConfirmation({ id: 'lesson-1', title: 'Markdown căn bản' })).toEqual({
      kind: 'lesson',
      lessonId: 'lesson-1',
      title: 'Xóa lesson?',
      description: 'Lesson “Markdown căn bản” và media usage liên quan sẽ bị xóa.',
    })
  })
})
