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

export async function fetchNotificationBatchSnapshotStatusRequest(batchId) {
  const response = await httpClient.get(API_ROUTES.notifications.batchSnapshotStatus(batchId))
  return unwrapEnvelope(response)
}

export async function fetchNotificationBatchDeliveryStatusRequest(batchId) {
  const response = await httpClient.get(API_ROUTES.notifications.batchDeliveryStatus(batchId))
  return unwrapEnvelope(response)
}

export async function fetchNotificationMediaUsageJobStatusRequest(jobId) {
  const response = await httpClient.get(API_ROUTES.media.notificationMediaUsageJobStatus(jobId))
  return unwrapEnvelope(response)
}

export async function fetchNotificationBatchesRequest(query) {
  const response = await httpClient.get(API_ROUTES.notifications.batches, {
    params: query,
  })
  return unwrapEnvelope(response)
}

export async function retryNotificationBatchFailuresRequest(batchId) {
  const response = await httpClient.post(API_ROUTES.notifications.retryBatchFailures(batchId))
  return { ...unwrapEnvelope(response), location: response.headers?.location ?? null }
}

export async function fetchNotificationBatchFailedItemsRequest(batchId, cursor) {
  const response = await httpClient.get(API_ROUTES.notifications.batchFailedItems(batchId), {
    params: cursor ? { cursor } : undefined,
  })
  return unwrapEnvelope(response)
}
