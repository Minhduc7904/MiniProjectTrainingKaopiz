// @vitest-environment jsdom

import { configureStore } from '@reduxjs/toolkit'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { aiReducer } from '@/features/ai/aiSlice'
import { AiProvidersPage } from '@/pages/ai/AiProvidersPage'

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
})

const providers = [
  { id: 'p1', name: 'OpenAI main', providerType: 'OPENAI', baseUrl: 'https://api.openai.com/v1', apiKeyHint: '****abcd', status: 'ACTIVE', createdAtUtc: '2026-01-01T00:00:00Z', updatedAtUtc: '2026-01-01T00:00:00Z' },
  { id: 'p2', name: 'Gemini lab', providerType: 'GEMINI', baseUrl: 'https://g.example/v1', apiKeyHint: '****wxyz', status: 'ACTIVE', createdAtUtc: '2026-01-01T00:00:00Z', updatedAtUtc: '2026-01-01T00:00:00Z' },
]
const health = (providerId, healthStatus, httpStatus, latencyMs) => ({ providerId, healthStatus, httpStatus, latencyMs, message: 'm' })

function setup(post) {
  vi.spyOn(httpClient, 'get').mockImplementation((route) => Promise.resolve({
    status: 200,
    data: route.endsWith('/options')
      ? { data: { providerTypes: [], bindableServices: [] }, meta: {} }
      : { data: providers, meta: { traceId: 't', pagination: { page: 1, pageSize: 20, totalItems: 2, totalPages: 1 } } },
  }))
  const postSpy = vi.spyOn(httpClient, 'post').mockImplementation(post)
  const store = configureStore({ reducer: { ai: aiReducer } })
  render(<Provider store={store}><MemoryRouter><AiProvidersPage /></MemoryRouter></Provider>)
  return postSpy
}

describe('AiProvidersPage connection status', () => {
  it('tests all providers once on open and shows each result', async () => {
    const post = setup(() => Promise.resolve({ status: 200, data: { data: [health('p1', 'ONLINE', 200, 120), health('p2', 'AUTH_FAILED', 401, 80)], meta: {} } }))

    await screen.findByText('Hoạt động')
    expect(screen.getByText('Key bị từ chối')).not.toBeNull()
    expect(screen.getByText('HTTP 401 · 80 ms')).not.toBeNull()
    expect(post).toHaveBeenCalledTimes(1)
    expect(post.mock.calls[0][0]).toBe('/ai/api/ai/providers/health-checks')
  })

  it('re-tests a single provider from its row', async () => {
    const post = setup((route) => Promise.resolve({
      status: 200,
      data: { data: route.endsWith('/health-check') ? health('p2', 'ONLINE', 200, 50) : [health('p1', 'ONLINE', 200, 120), health('p2', 'UNREACHABLE', null, 6000)], meta: {} },
    }))

    await screen.findByText('Không kết nối được')
    fireEvent.click(screen.getByRole('button', { name: 'Kiểm tra lại Gemini lab' }))

    await waitFor(() => expect(screen.queryByText('Không kết nối được')).toBeNull())
    expect(post).toHaveBeenLastCalledWith('/ai/api/ai/providers/p2/health-check')
  })
})
