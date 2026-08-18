import { httpClient } from '@/api/httpClient'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { API_ROUTES } from '@/constants/apiRoutes'
import { API_ERROR_CODES } from '@/constants/apiErrorCodes'
import { normalizeProgress } from '@/features/media/directUploadSlice'
import { ENV } from '@/constants/env'

export async function createUploadIntentRequest(payload, { signal } = {}) {
  const response = await httpClient.post(
    API_ROUTES.media.uploadIntents,
    payload,
    { signal },
  )
  return unwrapEnvelope(response)
}

export async function completeDirectUploadRequest(mediaId, actor, { signal } = {}) {
  const response = await httpClient.post(
    API_ROUTES.media.uploadComplete(mediaId),
    actor,
    { signal },
  )
  return unwrapEnvelope(response)
}

export function buildSignedUploadForm(formFields, file) {
  const body = new FormData()
  Object.entries(formFields ?? {}).forEach(([key, value]) => body.append(key, value))
  body.append('file', file)
  return body
}

export function uploadToSignedUrl({
  uploadUrl,
  formFields,
  file,
  onProgress = () => {},
  timeoutMs = ENV.directUploadTimeoutMs,
  createXhr = () => new XMLHttpRequest(),
}) {
  const xhr = createXhr()
  let settled = false
  let rejectRequest = () => {}
  const cleanup = () => {
    xhr.onload = null
    xhr.onerror = null
    xhr.onabort = null
    xhr.ontimeout = null
    xhr.upload.onprogress = null
  }
  const promise = new Promise((resolve, reject) => {
    const resolveOnce = () => {
      if (settled) return
      settled = true
      cleanup()
      resolve()
    }
    const rejectOnce = (error) => {
      if (settled) return
      settled = true
      cleanup()
      reject(error)
    }
    rejectRequest = rejectOnce
    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable) onProgress(normalizeProgress(event.loaded, event.total))
    }
    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        resolveOnce()
        return
      }
      rejectOnce({
        code: API_ERROR_CODES.directUploadFailed,
        message: 'MinIO từ chối direct upload. Hãy tạo lượt upload mới.',
        httpStatus: xhr.status || null,
        traceId: null,
      })
    }
    xhr.onerror = () => rejectOnce({
      code: API_ERROR_CODES.directUploadFailed,
      message: 'Không thể kết nối tới storage để upload.',
      httpStatus: null,
      traceId: null,
    })
    xhr.onabort = () => rejectOnce({
      code: API_ERROR_CODES.uploadCanceled,
      message: 'Direct upload đã bị hủy.',
      httpStatus: null,
      traceId: null,
    })
    xhr.ontimeout = () => rejectOnce({
      code: API_ERROR_CODES.requestTimeout,
      message: 'Direct upload đã hết thời gian chờ.',
      httpStatus: null,
      traceId: null,
    })
    try {
      xhr.open('POST', uploadUrl)
      xhr.timeout = timeoutMs
      xhr.send(buildSignedUploadForm(formFields, file))
    } catch {
      rejectOnce({
        code: API_ERROR_CODES.directUploadFailed,
        message: 'Không thể bắt đầu direct upload tới storage.',
        httpStatus: null,
        traceId: null,
      })
    }
  })
  promise.abort = () => {
    if (settled) return
    xhr.abort()
    rejectRequest({
      code: API_ERROR_CODES.uploadCanceled,
      message: 'Direct upload đã bị hủy.',
      httpStatus: null,
      traceId: null,
    })
  }
  return promise
}
