import { describe, expect, it } from 'vitest'
import {
  buildCourseCreatePayload,
  isCourseCreateFormValid,
} from '@/pages/courses/courseCreatePayload'

describe('course create form', () => {
  it('sends only the supported create fields and keeps the description markdown', () => {
    expect(buildCourseCreatePayload({
      name: '  Backend Fundamentals  ',
      descriptionMarkdown: '![cover](/media/api/media/media-1/content)',
      status: 'PUBLISHED',
    })).toEqual({
      name: 'Backend Fundamentals',
      descriptionMarkdown: '![cover](/media/api/media/media-1/content)',
    })
  })

  it('requires a course name with at least three non-whitespace characters', () => {
    expect(isCourseCreateFormValid({ name: '  AB  ' })).toBe(false)
    expect(isCourseCreateFormValid({ name: '  API  ' })).toBe(true)
  })
})
