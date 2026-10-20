import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'

export async function fetchQuizzesRequest(courseId, lessonId) {
  return unwrapEnvelope(await httpClient.get(API_ROUTES.courses.quizzes(courseId, lessonId)))
}

export async function fetchQuizDetailRequest(courseId, lessonId, quizId) {
  return unwrapEnvelope(await httpClient.get(API_ROUTES.courses.quizById(courseId, lessonId, quizId)))
}

export async function createQuizRequest(courseId, lessonId, payload) {
  return unwrapEnvelope(await httpClient.post(API_ROUTES.courses.quizzes(courseId, lessonId), payload))
}

export async function updateQuizRequest(courseId, lessonId, quizId, payload) {
  return unwrapEnvelope(await httpClient.put(API_ROUTES.courses.quizById(courseId, lessonId, quizId), payload))
}

export async function deleteQuizRequest(courseId, lessonId, quizId) {
  await httpClient.delete(API_ROUTES.courses.quizById(courseId, lessonId, quizId))
}
