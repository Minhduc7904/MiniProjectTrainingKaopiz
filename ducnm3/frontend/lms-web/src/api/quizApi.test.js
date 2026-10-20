import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { createQuizRequest, deleteQuizRequest, fetchQuizDetailRequest, fetchQuizzesRequest, updateQuizRequest } from '@/api/quizApi'
import { API_ROUTES } from '@/constants/apiRoutes'

afterEach(() => vi.restoreAllMocks())

const envelope = (data) => ({ data: { data, meta: { traceId: 't' } } })

describe('Quiz authoring requests', () => {
  const courseId = 'course-1'
  const lessonId = 'lesson-1'
  const quizId = 'quiz-1'

  it('uses the lesson-scoped quiz routes', async () => {
    const get = vi.spyOn(httpClient, 'get').mockResolvedValue(envelope([]))

    await fetchQuizzesRequest(courseId, lessonId)
    await fetchQuizDetailRequest(courseId, lessonId, quizId)

    expect(get).toHaveBeenNthCalledWith(1, '/course/api/courses/course-1/lessons/lesson-1/quizzes')
    expect(get).toHaveBeenNthCalledWith(2, API_ROUTES.courses.quizById(courseId, lessonId, quizId))
  })

  it('creates, updates and deletes a quiz', async () => {
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue(envelope({ id: quizId }))
    const put = vi.spyOn(httpClient, 'put').mockResolvedValue(envelope({ id: quizId }))
    const remove = vi.spyOn(httpClient, 'delete').mockResolvedValue({})

    await expect(createQuizRequest(courseId, lessonId, { title: 'Quiz' })).resolves.toMatchObject({ data: { id: quizId } })
    await updateQuizRequest(courseId, lessonId, quizId, { title: 'Quiz 2' })
    await expect(deleteQuizRequest(courseId, lessonId, quizId)).resolves.toBeUndefined()

    expect(post).toHaveBeenCalledWith(API_ROUTES.courses.quizzes(courseId, lessonId), { title: 'Quiz' })
    expect(put).toHaveBeenCalledWith(API_ROUTES.courses.quizById(courseId, lessonId, quizId), { title: 'Quiz 2' })
    expect(remove).toHaveBeenCalledWith(API_ROUTES.courses.quizById(courseId, lessonId, quizId))
  })
})
