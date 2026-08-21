import { httpClient } from '@/api/httpClient'
import { loggedFetch } from '@/api/httpLoggingInterceptors'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'

export async function fetchCoursesListRequest(query = {}) {
  const params = Object.fromEntries(Object.entries(query).filter(([, value]) => value != null && value !== ''))
  return unwrapEnvelope(await httpClient.get(API_ROUTES.courses.list, { params }))
}

export async function fetchCourseDetailsRequest(courseId) {
  return unwrapEnvelope(await httpClient.get(API_ROUTES.courses.detail(courseId)))
}

export async function createCourseRequest(course) {
  return unwrapEnvelope(await httpClient.post(API_ROUTES.courses.list, course))
}

export async function createCourseLessonRequest(courseId, lesson) {
  return unwrapEnvelope(await httpClient.post(API_ROUTES.courses.lessons(courseId), lesson))
}

export async function updateCourseRequest(courseId, payload) {
  return unwrapEnvelope(await httpClient.put(API_ROUTES.courses.byId(courseId), payload))
}

export async function deleteCourseRequest(courseId) {
  await httpClient.delete(API_ROUTES.courses.byId(courseId))
}

export async function fetchCourseLessonDetailRequest(courseId, lessonId) {
  return unwrapEnvelope(await httpClient.get(API_ROUTES.courses.lessonById(courseId, lessonId)))
}

export async function updateCourseLessonRequest(courseId, lessonId, payload) {
  return unwrapEnvelope(await httpClient.put(API_ROUTES.courses.lessonById(courseId, lessonId), payload))
}

export async function deleteCourseLessonRequest(courseId, lessonId) {
  await httpClient.delete(API_ROUTES.courses.lessonById(courseId, lessonId))
}

export async function reorderCourseLessonsRequest(courseId, lessonIds) {
  return unwrapEnvelope(await httpClient.put(API_ROUTES.courses.lessonReorder(courseId), { lessonIds }))
}

export async function exportCoursesRequest(query = {}, onProgress) {
  const params = new URLSearchParams()
  if (query.status) params.set('status', query.status)
  if (query.limit != null && query.limit !== '') params.set('limit', String(query.limit))
  const response = await loggedFetch(`${httpClient.defaults.baseURL}${API_ROUTES.courses.export}${params.size ? `?${params}` : ''}`, { signal: query.signal })
  if (!response.ok) throw await response.json()
  const reader = response.body.getReader()
  const chunks = []
  let bytesLoaded = 0
  let rowsLoaded = 0
  const startedAt = performance.now()
  const totalBytesHeader = response.headers.get('Content-Length')
  const totalBytes = totalBytesHeader ? Number(totalBytesHeader) : null
  while (true) {
    const { done, value } = await reader.read()
    if (done) break
    chunks.push(value)
    bytesLoaded += value.byteLength
    rowsLoaded += value.reduce((count, byte) => count + (byte === 10 ? 1 : 0), 0)
    onProgress?.({ bytesLoaded, totalBytes, rowsLoaded, elapsedMs: performance.now() - startedAt })
  }
  const blob = new Blob(chunks, { type: response.headers.get('Content-Type') ?? 'text/csv;charset=utf-8' })
  return { blob, bytesLoaded, rowsLoaded, totalBytes, filename: 'courses.csv' }
}
