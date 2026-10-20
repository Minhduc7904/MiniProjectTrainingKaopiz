import { afterEach, describe, expect, it, vi } from 'vitest'
import { studentHttpClient } from '@/api/studentHttpClient'
import {
  fetchQuizAttemptRequest,
  fetchQuizAttemptsRequest,
  fetchStudentQuizRequest,
  fetchStudentQuizzesRequest,
  saveQuizAnswersRequest,
  startQuizAttemptRequest,
  submitQuizAttemptRequest,
} from '@/api/studentQuizApi'
import { API_ROUTES } from '@/constants/apiRoutes'

afterEach(() => vi.restoreAllMocks())

const ids = { courseId: 'course-1', lessonId: 'lesson-1', quizId: 'quiz-1', attemptId: 'attempt-1' }
const envelope = (data, status = 200) => ({ status, data: { data, meta: { traceId: 't' } } })

describe('Student quiz requests', () => {
  it('lists and loads quizzes through the Student enrollment routes', async () => {
    const get = vi.spyOn(studentHttpClient, 'get').mockResolvedValue(envelope([]))

    await fetchStudentQuizzesRequest(ids.courseId, ids.lessonId)
    await fetchStudentQuizRequest(ids.courseId, ids.lessonId, ids.quizId)

    expect(get).toHaveBeenNthCalledWith(1, API_ROUTES.studentLearning.quizzes(ids.courseId, ids.lessonId), { signal: undefined })
    expect(get).toHaveBeenNthCalledWith(2, API_ROUTES.studentLearning.quizById(ids.courseId, ids.lessonId, ids.quizId), { signal: undefined })
    expect(API_ROUTES.studentLearning.quizzes(ids.courseId, ids.lessonId)).toBe('/course/api/student/enrollments/course-1/lessons/lesson-1/quizzes')
  })

  it.each([[201, true], [200, false]])('flags a new attempt only for status %s', async (status, created) => {
    vi.spyOn(studentHttpClient, 'post').mockResolvedValue(envelope({ id: ids.attemptId, answers: [] }, status))

    const result = await startQuizAttemptRequest(ids.courseId, ids.lessonId, ids.quizId)

    expect(result).toMatchObject({ data: { id: ids.attemptId }, created })
  })

  it('saves a full draft replacing previous answers', async () => {
    const put = vi.spyOn(studentHttpClient, 'put').mockResolvedValue(envelope({ id: ids.attemptId }))
    const answers = [{ questionId: 'q1', optionIds: ['o1'] }]

    await saveQuizAnswersRequest(ids.courseId, ids.lessonId, ids.quizId, ids.attemptId, answers)

    expect(put).toHaveBeenCalledWith(API_ROUTES.studentLearning.quizAttemptAnswers(ids.courseId, ids.lessonId, ids.quizId, ids.attemptId), { answers })
  })

  it('submits final answers, or null to grade the saved draft', async () => {
    const post = vi.spyOn(studentHttpClient, 'post').mockResolvedValue(envelope({}))
    const url = API_ROUTES.studentLearning.quizAttemptSubmit(ids.courseId, ids.lessonId, ids.quizId, ids.attemptId)

    await submitQuizAttemptRequest(ids.courseId, ids.lessonId, ids.quizId, ids.attemptId, [{ questionId: 'q1', optionIds: [] }])
    await submitQuizAttemptRequest(ids.courseId, ids.lessonId, ids.quizId, ids.attemptId)

    expect(post).toHaveBeenNthCalledWith(1, url, { answers: [{ questionId: 'q1', optionIds: [] }] })
    expect(post).toHaveBeenNthCalledWith(2, url, { answers: null })
  })

  it('reads attempt history and a single attempt', async () => {
    const get = vi.spyOn(studentHttpClient, 'get').mockResolvedValue(envelope([]))

    await fetchQuizAttemptsRequest(ids.courseId, ids.lessonId, ids.quizId)
    await fetchQuizAttemptRequest(ids.courseId, ids.lessonId, ids.quizId, ids.attemptId)

    expect(get).toHaveBeenNthCalledWith(1, API_ROUTES.studentLearning.quizAttempts(ids.courseId, ids.lessonId, ids.quizId), { signal: undefined })
    expect(get).toHaveBeenNthCalledWith(2, API_ROUTES.studentLearning.quizAttemptById(ids.courseId, ids.lessonId, ids.quizId, ids.attemptId), { signal: undefined })
  })
})
