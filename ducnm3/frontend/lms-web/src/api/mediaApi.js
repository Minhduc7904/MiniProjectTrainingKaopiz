import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'
import { HTTP_HEADERS } from '@/constants/http'
import { POST_MEDIA_FIELDS } from '@/constants/media'

export async function uploadMediaRequest({
  file,
  mediaType,
  uploadedByType,
  uploadedBy,
}) {
  const body = new FormData()
  body.append(POST_MEDIA_FIELDS.file, file)
  body.append(POST_MEDIA_FIELDS.mediaType, mediaType)
  body.append(POST_MEDIA_FIELDS.uploadedByType, uploadedByType)
  body.append(POST_MEDIA_FIELDS.uploadedBy, uploadedBy)

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
