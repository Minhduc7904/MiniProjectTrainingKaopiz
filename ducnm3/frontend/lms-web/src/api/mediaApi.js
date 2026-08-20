import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'
import { HTTP_HEADERS } from '@/constants/http'
import { POST_MEDIA_FIELDS } from '@/constants/media'

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

export async function getMediaThumbnailRequest(thumbnailStatusUrl, { signal } = {}) {
  const response = await httpClient.get(thumbnailStatusUrl, { signal })
  return unwrapEnvelope(response)
}

export async function getMediaContentRequest(contentUrl, { signal } = {}) {
  const response = await httpClient.get(contentUrl, { signal, responseType: 'blob' })
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
