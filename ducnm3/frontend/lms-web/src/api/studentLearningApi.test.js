import { afterEach, describe, expect, it, vi } from 'vitest'
import { studentHttpClient } from '@/api/studentHttpClient'
import { completeStudentLessonRequest, fetchStudentCourseCatalogRequest, fetchStudentLessonDetailRequest } from '@/api/studentLearningApi'
import { API_ROUTES } from '@/constants/apiRoutes'

afterEach(() => vi.restoreAllMocks())

describe('Student learning requests', () => {
  const courseId = 'course-1'
  const lessonId = 'lesson-1'

  it('loads a lesson through the Student enrollment detail route', async () => {
    const get = vi.spyOn(studentHttpClient, 'get').mockResolvedValue({
      data: { data: { id: lessonId }, meta: { traceId: 'trace-lesson' } },
    })

    await expect(fetchStudentLessonDetailRequest(courseId, lessonId)).resolves.toMatchObject({
      data: { id: lessonId },
    })
    expect(get).toHaveBeenCalledWith(API_ROUTES.studentLearning.lessonDetail(courseId, lessonId))
  })

  it('completes a lesson through the existing Student progress route', async () => {
    const post = vi.spyOn(studentHttpClient, 'post').mockResolvedValue({
      data: { data: { lessonId, completedAtUtc: '2026-08-22T00:00:00Z' }, meta: { traceId: 'trace-complete' } },
    })

    await expect(completeStudentLessonRequest(courseId, lessonId)).resolves.toMatchObject({
      data: { lessonId },
    })
    expect(post).toHaveBeenCalledWith(API_ROUTES.studentLearning.completeLesson(courseId, lessonId))
  })

  it('sends the Course search query to the Student catalog route', async () => {
    const get = vi.spyOn(studentHttpClient, 'get').mockResolvedValue({
      data: { data: [], meta: { traceId: 'trace-catalog', pagination: {} } },
    })

    await fetchStudentCourseCatalogRequest({ search: 'Backend', page: 1, pageSize: 12 })

    expect(get).toHaveBeenCalledWith(API_ROUTES.studentLearning.catalog, {
      params: { search: 'Backend', page: 1, pageSize: 12 },
    })
  })
})
