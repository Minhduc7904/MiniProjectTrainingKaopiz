import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'

export async function createNotificationBatchRequest(payload) {
  const response = await httpClient.post(API_ROUTES.notifications.batches, payload)
  return { ...unwrapEnvelope(response), location: response.headers?.location ?? null }
}

export async function fetchNotificationBatchRequest(batchId) {
  const response = await httpClient.get(API_ROUTES.notifications.batchById(batchId))
  return unwrapEnvelope(response)
}

export async function fetchNotificationBatchFailedItemsRequest(batchId, cursor) {
  const response = await httpClient.get(API_ROUTES.notifications.batchFailedItems(batchId), {
    params: cursor ? { cursor } : undefined,
  })
  return unwrapEnvelope(response)
}
