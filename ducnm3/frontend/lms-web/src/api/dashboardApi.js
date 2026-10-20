import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'

async function timed(send) {
  const startedAt = performance.now()
  const response = await send()
  const envelope = unwrapEnvelope(response)
  return {
    ...envelope,
    httpStatus: response.status,
    latencyMs: Math.round(performance.now() - startedAt),
    checkedAt: new Date().toISOString(),
  }
}

const timedGet = (route) => timed(() => httpClient.get(route))

export const fetchStudentSummaryRequest = () => timedGet(API_ROUTES.students.summary)
export const fetchMediaSummaryRequest = () => timedGet(API_ROUTES.media.summary)
export const fetchCoursesSummaryRequest = () => timedGet(API_ROUTES.courses.summary)
export const fetchHealthRequest = (route) => timedGet(route)

// Server thử gọi từng AI provider nên là POST; trả danh sách provider kèm trạng thái kết nối.
export const fetchAiProvidersHealthRequest = () => timed(() => httpClient.post(API_ROUTES.ai.providerHealthChecks))
