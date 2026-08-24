import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'
import { QUERY_PARAMS } from '@/constants/queryParams'

export async function fetchStudentsListRequest(query = {}) {
  const params = {}

  if (query.search) {
    params[QUERY_PARAMS.search] = query.search
  }

  if (query.status) {
    params[QUERY_PARAMS.status] = query.status
  }

  if (query.sortBy) {
    params[QUERY_PARAMS.sortBy] = query.sortBy
  }

  if (query.sortDirection) {
    params[QUERY_PARAMS.sortDirection] = query.sortDirection
  }

  if (query.page) {
    params[QUERY_PARAMS.page] = query.page
  }

  if (query.pageSize) {
    params[QUERY_PARAMS.pageSize] = query.pageSize
  }

  const response = await httpClient.get(API_ROUTES.students.list, { params })
  return unwrapEnvelope(response)
}
