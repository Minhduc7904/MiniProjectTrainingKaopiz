import { httpClient } from '@/api/httpClient'
import {
  HTTP_STARTED_AT_KEY,
  logHttpError,
  logHttpRequest,
  logHttpResponse,
} from '@/api/httpLogger'
import { toApiError } from '@/api/toApiError'
import { HTTP_HEADERS, HTTP_SUCCESS_CODES } from '@/constants/http'
import {
  TOAST_CONFIG_KEY,
  TOAST_DURATION_MS,
  TOAST_MESSAGES,
} from '@/constants/toast'
import {
  toastDismissed,
  toastRequestFailed,
  toastRequestStarted,
  toastRequestSucceeded,
} from '@/features/toasts/toastsSlice'

function requestPath(config) {
  const url = config?.url ?? ''
  return url.split('?')[0] || '/'
}

function toastMeta(config) {
  return config?.[TOAST_CONFIG_KEY]
}

function shouldSkipToast(config) {
  return toastMeta(config)?.skip === true
}

// Request đọc dữ liệu không hiện toast "Đang gọi API"/"Thành công"; chỉ báo khi lỗi.
function isReadRequest(method) {
  return method === 'GET' || method === 'HEAD'
}

let attached = false

export function attachHttpInterceptors(store) {
  if (attached) {
    return
  }

  attached = true

  httpClient.interceptors.request.use((config) => {
    if (typeof FormData !== 'undefined' && config.data instanceof FormData) {
      if (config.headers && typeof config.headers.delete === 'function') {
        config.headers.delete(HTTP_HEADERS.contentType)
      } else if (config.headers) {
        delete config.headers[HTTP_HEADERS.contentType]
        delete config.headers['content-type']
      }
    }

    if (shouldSkipToast(config)) return config

    const id = crypto.randomUUID()
    config.headers[HTTP_HEADERS.correlationId] = id
    config[TOAST_CONFIG_KEY] = {
      id,
      method: (config.method || 'get').toUpperCase(),
      path: requestPath(config),
    }
    config[HTTP_STARTED_AT_KEY] = Date.now()

    logHttpRequest(config)

    if (isReadRequest(config[TOAST_CONFIG_KEY].method)) return config

    store.dispatch(
      toastRequestStarted({
        id,
        method: config[TOAST_CONFIG_KEY].method,
        path: config[TOAST_CONFIG_KEY].path,
        message: TOAST_MESSAGES.pending,
        durationMs: config.timeout || TOAST_DURATION_MS.pending,
      }),
    )

    return config
  })

  httpClient.interceptors.response.use(
    (response) => {
      logHttpResponse(response)
      const meta = toastMeta(response.config)
      if (!meta || meta.skip || isReadRequest(meta.method)) {
        return response
      }

      store.dispatch(
        toastRequestSucceeded({
          id: meta.id,
          httpStatus: response.status,
          code: HTTP_SUCCESS_CODES[response.status] ?? 'OK',
          message: TOAST_MESSAGES.success,
          traceId: response.data?.meta?.traceId ?? meta.id,
        }),
      )

      return response
    },
    (error) => {
      if (error?.code === 'ERR_CANCELED' || error?.name === 'CanceledError') {
        // Request bị hủy sẽ không còn response nào đóng toast, nên gỡ toast "Đang gọi API" ở đây.
        const canceledId = toastMeta(error.config)?.id
        if (canceledId) store.dispatch(toastDismissed(canceledId))
        return Promise.reject(error)
      }
      if (shouldSkipToast(error.config)) return Promise.reject(error)
      logHttpError(error)
      const meta = toastMeta(error.config)
      const apiError = toApiError(error)

      store.dispatch(
        toastRequestFailed({
          id: meta?.id ?? crypto.randomUUID(),
          method: meta?.method,
          path: meta?.path,
          httpStatus: apiError.httpStatus,
          code: apiError.code,
          message: apiError.message,
          traceId: apiError.traceId,
        }),
      )

      return Promise.reject(error)
    },
  )
}
