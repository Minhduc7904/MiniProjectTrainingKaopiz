// @vitest-environment jsdom
import { CanceledError } from 'axios'
import { configureStore } from '@reduxjs/toolkit'
import { describe, expect, it } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { attachHttpInterceptors } from '@/api/httpInterceptors'
import { SILENT_TOAST_CONFIG } from '@/constants/toast'
import { toastsReducer } from '@/features/toasts/toastsSlice'

const store = configureStore({ reducer: { toasts: toastsReducer } })
attachHttpInterceptors(store)

describe('http toast interceptors', () => {
  it('removes the pending toast when the request is canceled', async () => {
    let pendingDuringRequest = 0
    const adapter = (config) => {
      pendingDuringRequest = store.getState().toasts.items.length
      return Promise.reject(new CanceledError('canceled', config))
    }

    await expect(httpClient.post('/cancel-me', {}, { adapter })).rejects.toBeInstanceOf(CanceledError)

    expect(pendingDuringRequest).toBe(1)
    expect(store.getState().toasts.items).toEqual([])
  })

  it('shows no toast for silent background requests', async () => {
    const adapter = (config) => Promise.resolve({ data: {}, status: 200, statusText: 'OK', headers: {}, config })

    await httpClient.get('/silent', { adapter, apiToast: SILENT_TOAST_CONFIG })

    expect(store.getState().toasts.items).toEqual([])
  })

  it('shows no pending or success toast for GET requests', async () => {
    let toastsDuringRequest = -1
    const adapter = (config) => {
      toastsDuringRequest = store.getState().toasts.items.length
      return Promise.resolve({ data: {}, status: 200, statusText: 'OK', headers: {}, config })
    }

    await httpClient.get('/read', { adapter })

    expect(toastsDuringRequest).toBe(0)
    expect(store.getState().toasts.items).toEqual([])
  })

  it('still shows a toast for write requests and for failed GET requests', async () => {
    const ok = (config) => Promise.resolve({ data: {}, status: 200, statusText: 'OK', headers: {}, config })
    await httpClient.post('/write', {}, { adapter: ok })
    expect(store.getState().toasts.items).toHaveLength(1)

    const fail = (config) => Promise.reject(Object.assign(new Error('boom'), { config, isAxiosError: true }))
    await expect(httpClient.get('/read-fails', { adapter: fail })).rejects.toThrow('boom')
    expect(store.getState().toasts.items[0].phase).toBe('error')
  })
})
