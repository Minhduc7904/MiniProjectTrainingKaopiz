import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'
import { HTTP_HEADERS } from '@/constants/http'
import { POST_MEDIA_FIELDS } from '@/constants/media'
import { QUERY_PARAMS } from '@/constants/queryParams'
import { SILENT_TOAST_CONFIG } from '@/constants/toast'

export async function uploadMediaRequest({
  file,
}) {
  const body = new FormData()
  body.append(POST_MEDIA_FIELDS.file, file)

  const response = await httpClient.post(API_ROUTES.media.upload, body)
  const envelope = unwrapEnvelope(response)
  const headers = response.headers
  const location =
    (typeof headers?.get === 'function'
      ? headers.get(HTTP_HEADERS.location)
      : headers?.[HTTP_HEADERS.location]) ?? null

  return {
    data: envelope.data,
    meta: envelope.meta,
    location,
  }
}

export async function fetchMediaLibraryRequest({ mediaType = '', status = '', cursor = null, pageSize = 24 } = {}) {
  const params = { [QUERY_PARAMS.pageSize]: pageSize }
  if (mediaType) params[QUERY_PARAMS.mediaType] = mediaType
  if (status) params[QUERY_PARAMS.status] = status
  if (cursor) params[QUERY_PARAMS.cursor] = cursor
  const response = await httpClient.get(API_ROUTES.media.library, { params })
  return unwrapEnvelope(response)
}

export async function getMediaThumbnailRequest(thumbnailStatusUrl, { signal } = {}) {
  const response = await httpClient.get(thumbnailStatusUrl, { signal })
  return unwrapEnvelope(response)
}

export async function getMediaContentRequest(contentUrl, { signal } = {}) {
  const response = await httpClient.get(contentUrl, {
    signal,
    responseType: 'blob',
    apiToast: SILENT_TOAST_CONFIG,
  })
  return response.data
}

export async function retryMediaThumbnailRequest(mediaId, { signal } = {}) {
  const response = await httpClient.post(
    API_ROUTES.media.retryThumbnail(mediaId),
    undefined,
    { signal },
  )
  return unwrapEnvelope(response)
}

export async function createMediaUsageRequest(item) {
  return unwrapEnvelope(await httpClient.post(API_ROUTES.media.usages, item))
}

export async function createMediaUsagesBatchRequest(items) {
  return unwrapEnvelope(await httpClient.post(API_ROUTES.media.usageBatch, { items }))
}

export async function removeMediaUsageRequest(usageId) {
  return httpClient.delete(API_ROUTES.media.usageById(usageId))
}

export async function reorderMediaUsagesRequest({ ownerService, ownerType, ownerId, usageIds }) {
  return unwrapEnvelope(await httpClient.post(API_ROUTES.media.usageReorder, {
    ownerService,
    ownerType,
    ownerId,
    usageIds,
  }))
}
