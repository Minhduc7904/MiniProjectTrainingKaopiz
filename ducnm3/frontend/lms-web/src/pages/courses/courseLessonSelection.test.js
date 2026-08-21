import { describe, expect, it } from 'vitest'
import { canLoadSelectedLessonDetail } from '@/pages/courses/courseLessonSelection'

describe('canLoadSelectedLessonDetail', () => {
  it('returns false when the selected lesson is absent from the current course', () => {
    expect(canLoadSelectedLessonDetail(
      { id: 'course-1', lessons: [] },
      'course-1',
      'deleted-lesson',
    )).toBe(false)
  })

  it('returns false while detail data belongs to a previous course', () => {
    expect(canLoadSelectedLessonDetail(
      { id: 'course-old', lessons: [{ id: 'lesson-1' }] },
      'course-new',
      'lesson-1',
    )).toBe(false)
  })
})
