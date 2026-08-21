import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'
import { QUERY_PARAMS } from '@/constants/queryParams'

export async function fetchMediaJobsRequest(query = {}) {
  const params = {}
  for (const key of [QUERY_PARAMS.jobType, QUERY_PARAMS.status, QUERY_PARAMS.correlationId, QUERY_PARAMS.page, QUERY_PARAMS.pageSize]) {
    if (query[key]) params[key] = query[key]
  }
  const response = await httpClient.get(API_ROUTES.media.jobs, { params })
  return unwrapEnvelope(response)
}
