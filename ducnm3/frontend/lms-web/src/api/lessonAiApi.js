import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'
import { SILENT_TOAST_CONFIG } from '@/constants/toast'

// Polling chạy nền nên không hiện toast; thao tác do người dùng bấm vẫn dùng toast toàn cục.
function pollConfig(options = {}) {
  return { signal: options.signal, apiToast: SILENT_TOAST_CONFIG }
}

export async function fetchLessonAiDocumentsRequest(courseId, lessonId, options = {}) {
  return unwrapEnvelope(await httpClient.get(API_ROUTES.courses.lessonAiDocuments(courseId, lessonId), options.silent ? pollConfig(options) : { signal: options.signal }))
}

export async function syncLessonAiDocumentsRequest(courseId, lessonId, { retryFailed = false, signal } = {}) {
  return unwrapEnvelope(await httpClient.post(API_ROUTES.courses.lessonAiDocumentsSync(courseId, lessonId), null, {
    params: retryFailed ? { retryFailed: true } : undefined,
    signal,
  }))
}

export async function fetchLessonAiJobsRequest(courseId, lessonId, options = {}) {
  return unwrapEnvelope(await httpClient.get(API_ROUTES.courses.lessonAiJobs(courseId, lessonId), options.silent ? pollConfig(options) : { signal: options.signal }))
}

export async function fetchLessonAiJobRequest(courseId, lessonId, jobId) {
  return unwrapEnvelope(await httpClient.get(API_ROUTES.courses.lessonAiJobById(courseId, lessonId, jobId)))
}

export async function createLessonAiJobRequest(courseId, lessonId, payload) {
  return unwrapEnvelope(await httpClient.post(API_ROUTES.courses.lessonAiJobs(courseId, lessonId), payload))
}
