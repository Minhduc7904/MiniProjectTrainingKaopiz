import axios from 'axios'
import { ENV } from '@/constants/env'
import { HTTP_LOG_LABELS } from '@/constants/httpLog'
import { TOAST_CONFIG_KEY } from '@/constants/toast'
import { sanitizeSensitiveValue } from '@/utils/sanitizeSensitiveValue'

export const HTTP_STARTED_AT_KEY = 'apiStartedAt'
export const redactHttpLogValue = sanitizeSensitiveValue

function fullUrl(config) {
  try {
    return axios.getUri(config)
  } catch {
    return config?.url ?? ''
  }
}

function snapshotHeaders(headers) {
  if (!headers) {
    return {}
  }

  if (typeof headers.toJSON === 'function') {
    return headers.toJSON()
  }

  return { ...headers }
}

function durationMs(config) {
  const startedAt = config?.[HTTP_STARTED_AT_KEY]
  if (typeof startedAt !== 'number') {
    return null
  }

  return Date.now() - startedAt
}

export function logHttpRequest(config) {
  if (!ENV.httpLog) {
    return
  }

  const method = (config.method || 'get').toUpperCase()
  const url = fullUrl(config)
  const id = config[TOAST_CONFIG_KEY]?.id

  console.group(`[${HTTP_LOG_LABELS.request}] ${method} ${url}`)
  console.log('id', id)
  console.log('baseURL', config.baseURL)
  console.log('url', config.url)
  console.log('params', redactHttpLogValue(config.params ?? null))
  console.log('headers', redactHttpLogValue(snapshotHeaders(config.headers)))
  console.log(
    'data',
    typeof FormData !== 'undefined' && config.data instanceof FormData
      ? [...config.data.keys()]
      : redactHttpLogValue(config.data ?? null),
  )
  console.groupEnd()
}

export function logHttpResponse(response) {
  if (!ENV.httpLog) {
    return
  }

  const config = response.config
  const method = (config.method || 'get').toUpperCase()
  const url = fullUrl(config)
  const id = config[TOAST_CONFIG_KEY]?.id

  console.group(
    `[${HTTP_LOG_LABELS.response}] ${response.status} ${method} ${url}`,
  )
  console.log('id', id)
  console.log('durationMs', durationMs(config))
  console.log('status', response.status)
  console.log('statusText', response.statusText)
  console.log('headers', redactHttpLogValue(snapshotHeaders(response.headers)))
  console.log('data', redactHttpLogValue(response.data))
  console.groupEnd()
}

export function logHttpError(error) {
  if (!ENV.httpLog) {
    return
  }

  const config = error.config ?? {}
  const method = (config.method || 'get').toUpperCase()
  const url = fullUrl(config)
  const id = config[TOAST_CONFIG_KEY]?.id
  const status = error.response?.status ?? null

  console.group(
    `[${HTTP_LOG_LABELS.error}] ${status ?? error.code ?? 'FAILED'} ${method} ${url}`,
  )
  console.log('id', id)
  console.log('durationMs', durationMs(config))
  console.log('code', error.code ?? null)
  console.log('message', error.message)
  console.log('status', status)
  console.log('headers', redactHttpLogValue(snapshotHeaders(error.response?.headers)))
  console.log('data', redactHttpLogValue(error.response?.data ?? null))
  console.error({ code: error.code ?? null, message: error.message, status })
  console.groupEnd()
}
