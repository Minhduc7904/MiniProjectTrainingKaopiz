import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'

async function timedGet(route) {
  const startedAt = performance.now()
  const response = await httpClient.get(route)
  const envelope = unwrapEnvelope(response)
  return {
    ...envelope,
    httpStatus: response.status,
    latencyMs: Math.round(performance.now() - startedAt),
    checkedAt: new Date().toISOString(),
  }
}

export const fetchStudentSummaryRequest = () => timedGet(API_ROUTES.students.summary)
export const fetchMediaSummaryRequest = () => timedGet(API_ROUTES.media.summary)
export const fetchCoursesSummaryRequest = () => timedGet(API_ROUTES.courses.summary)
export const fetchHealthRequest = (route) => timedGet(route)
