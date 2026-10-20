import { studentHttpClient } from '@/api/studentHttpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'

const routes = API_ROUTES.studentLearning

export async function fetchStudentQuizzesRequest(courseId, lessonId, options = {}) {
  return unwrapEnvelope(await studentHttpClient.get(routes.quizzes(courseId, lessonId), { signal: options.signal }))
}

export async function fetchStudentQuizRequest(courseId, lessonId, quizId, options = {}) {
  return unwrapEnvelope(await studentHttpClient.get(routes.quizById(courseId, lessonId, quizId), { signal: options.signal }))
}

/** 201 khi tạo bài làm mới, 200 khi tiếp tục bài đang làm dở; cả hai trả về đáp án đã lưu. */
export async function startQuizAttemptRequest(courseId, lessonId, quizId, options = {}) {
  const response = await studentHttpClient.post(routes.quizAttempts(courseId, lessonId, quizId), undefined, { signal: options.signal })
  return { ...unwrapEnvelope(response), created: response.status === 201 }
}

export async function saveQuizAnswersRequest(courseId, lessonId, quizId, attemptId, answers) {
  return unwrapEnvelope(await studentHttpClient.put(routes.quizAttemptAnswers(courseId, lessonId, quizId, attemptId), { answers }))
}

/** `answers` null nghĩa là nộp đáp án nháp đã lưu trên server. */
export async function submitQuizAttemptRequest(courseId, lessonId, quizId, attemptId, answers = null) {
  return unwrapEnvelope(await studentHttpClient.post(routes.quizAttemptSubmit(courseId, lessonId, quizId, attemptId), { answers }))
}

export async function fetchQuizAttemptsRequest(courseId, lessonId, quizId, options = {}) {
  return unwrapEnvelope(await studentHttpClient.get(routes.quizAttempts(courseId, lessonId, quizId), { signal: options.signal }))
}

export async function fetchQuizAttemptRequest(courseId, lessonId, quizId, attemptId, options = {}) {
  return unwrapEnvelope(await studentHttpClient.get(routes.quizAttemptById(courseId, lessonId, quizId, attemptId), { signal: options.signal }))
}
