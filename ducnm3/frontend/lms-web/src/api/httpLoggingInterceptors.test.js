import axios from 'axios'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@/api/httpLogger', () => ({
  HTTP_STARTED_AT_KEY: 'apiStartedAt',
  logHttpError: vi.fn(),
  logHttpRequest: vi.fn(),
  logHttpResponse: vi.fn(),
}))

import { attachHttpLoggingInterceptors, loggedFetch } from '@/api/httpLoggingInterceptors'
import { logHttpRequest, logHttpResponse } from '@/api/httpLogger'

afterEach(() => {
  vi.clearAllMocks()
})

describe('attachHttpLoggingInterceptors', () => {
  it('logs the request and response for an independent Axios client', async () => {
    const client = axios.create()
    client.defaults.adapter = async (config) => ({
      config,
      data: { data: { ok: true } },
      headers: {},
      status: 200,
      statusText: 'OK',
    })

    attachHttpLoggingInterceptors(client)

    await client.get('/student/api/auth/me')

    expect(logHttpRequest).toHaveBeenCalledTimes(1)
    expect(logHttpRequest.mock.calls[0][0].apiStartedAt).toEqual(expect.any(Number))
    expect(logHttpResponse).toHaveBeenCalledTimes(1)
  })

  it('logs a fetch request that does not use Axios', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response('courseId,title', {
        headers: { 'Content-Type': 'text/csv' },
        status: 200,
      }),
    )

    await loggedFetch('/course/api/courses/export')

    expect(fetchMock).toHaveBeenCalledWith('/course/api/courses/export', undefined)
    expect(logHttpRequest).toHaveBeenCalledTimes(1)
    expect(logHttpResponse).toHaveBeenCalledTimes(1)
    fetchMock.mockRestore()
  })
})
