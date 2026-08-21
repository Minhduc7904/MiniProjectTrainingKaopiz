import {
  HTTP_STARTED_AT_KEY,
  logHttpError,
  logHttpRequest,
  logHttpResponse,
} from '@/api/httpLogger'

const attachedClients = new WeakSet()

function isCancelled(error) {
  return error?.code === 'ERR_CANCELED' || error?.name === 'CanceledError'
}

function toFetchLogConfig(input, init) {
  const request = typeof Request !== 'undefined' && input instanceof Request ? input : null

  return {
    [HTTP_STARTED_AT_KEY]: Date.now(),
    headers: init?.headers ?? request?.headers ?? {},
    method: init?.method ?? request?.method ?? 'get',
    url: request?.url ?? String(input),
  }
}

export function attachHttpLoggingInterceptors(client) {
  if (attachedClients.has(client)) {
    return
  }

  attachedClients.add(client)

  client.interceptors.request.use((config) => {
    config[HTTP_STARTED_AT_KEY] ??= Date.now()
    logHttpRequest(config)
    return config
  })

  client.interceptors.response.use(
    (response) => {
      logHttpResponse(response)
      return response
    },
    (error) => {
      if (!isCancelled(error)) {
        logHttpError(error)
      }
      return Promise.reject(error)
    },
  )
}

export async function loggedFetch(input, init) {
  const config = toFetchLogConfig(input, init)
  logHttpRequest(config)

  try {
    const response = await fetch(input, init)
    logHttpResponse({
      config,
      data: null,
      headers: Object.fromEntries(response.headers.entries()),
      status: response.status,
      statusText: response.statusText,
    })
    return response
  } catch (error) {
    if (!isCancelled(error)) {
      logHttpError({
        code: error?.code,
        config,
        message: error instanceof Error ? error.message : String(error),
      })
    }
    throw error
  }
}
