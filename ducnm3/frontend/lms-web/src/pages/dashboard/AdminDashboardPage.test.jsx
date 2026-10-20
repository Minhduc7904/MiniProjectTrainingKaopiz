// @vitest-environment jsdom

import { configureStore } from '@reduxjs/toolkit'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { dashboardReducer } from '@/features/dashboard/dashboardSlice'
import { AdminDashboardPage } from '@/pages/dashboard/AdminDashboardPage'

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
})

function responseFor(route) {
  const body = route.includes('/students/summary')
    ? { totalStudents: 42 }
    : route.includes('/media/summary')
      ? { totalMedia: 18 }
      : route.includes('/courses/summary')
        ? { totalCourses: 12, totalLessons: 37 }
        : { service: 'service', status: 'healthy', database: { status: 'healthy' } }
  return { status: 200, data: { data: body, meta: { traceId: `trace-${route}` } } }
}

const aiProviders = [
  { providerId: 'p1', name: 'OpenAI main', providerType: 'OPENAI', baseUrl: 'https://api.openai.com/v1', providerStatus: 'ACTIVE', healthStatus: 'ONLINE', httpStatus: 200, latencyMs: 123, message: 'ok' },
  { providerId: 'p2', name: 'Gemini lab', providerType: 'GEMINI', baseUrl: 'https://gemini.example/v1', providerStatus: 'ACTIVE', healthStatus: 'AUTH_FAILED', httpStatus: 401, latencyMs: 80, message: 'rejected' },
  { providerId: 'p3', name: 'Mistral off', providerType: 'MISTRAL', baseUrl: 'https://mistral.example/v1', providerStatus: 'INACTIVE', healthStatus: 'DISABLED', httpStatus: null, latencyMs: null, message: 'off' },
]

describe('AdminDashboardPage', () => {
  it('renders metrics, all health cards and no sidebar, then refreshes all requests', async () => {
    const get = vi.spyOn(httpClient, 'get').mockImplementation((route) => Promise.resolve(responseFor(route)))
    const post = vi.spyOn(httpClient, 'post').mockResolvedValue({ status: 200, data: { data: aiProviders, meta: { traceId: 'trace-ai' } } })
    const store = configureStore({ reducer: { dashboard: dashboardReducer } })
    const view = render(<Provider store={store}><MemoryRouter><AdminDashboardPage /></MemoryRouter></Provider>)

    await screen.findByText('42')
    await screen.findByText('18')
    await screen.findByText('37')
    expect(screen.getByText('API Gateway')).not.toBeNull()
    expect(screen.getByText('Scheduler Service')).not.toBeNull()
    expect(screen.getByText('AI Service')).not.toBeNull()
    await screen.findByText('OpenAI main')
    expect(screen.getByText('Hoạt động')).not.toBeNull()
    expect(screen.getByText('Key bị từ chối')).not.toBeNull()
    expect(screen.getByText('Mistral off')).not.toBeNull()
    expect(screen.getByRole('link', { name: /AI Provider, model và gán service/i }).getAttribute('href')).toBe('/admin/ai/providers')
    expect(screen.getByText('Màn hình quản lý đang được phát triển').closest('[aria-disabled]')?.getAttribute('aria-disabled')).toBe('true')
    expect(view.container.querySelector('aside')).toBeNull()
    expect(get).toHaveBeenCalledTimes(10)
    expect(post).toHaveBeenCalledTimes(1)

    fireEvent.click(screen.getByRole('button', { name: /Kiểm tra lại toàn bộ/i }))
    await waitFor(() => expect(get).toHaveBeenCalledTimes(20))
    expect(post).toHaveBeenCalledTimes(2)
  })

  it('shows an error and an empty hint for the AI provider list without breaking the rest', async () => {
    vi.spyOn(httpClient, 'get').mockImplementation((route) => Promise.resolve(responseFor(route)))
    const post = vi.spyOn(httpClient, 'post').mockResolvedValueOnce({ status: 200, data: { data: [], meta: { traceId: 't' } } })
    const store = configureStore({ reducer: { dashboard: dashboardReducer } })
    render(<Provider store={store}><MemoryRouter><AdminDashboardPage /></MemoryRouter></Provider>)

    await screen.findByText(/Chưa có AI provider nào/)
    post.mockRejectedValueOnce({ response: { status: 502, data: { error: { code: 'BAD_GATEWAY', message: 'down' } } } })
    const section = screen.getByRole('heading', { name: 'AI provider' }).closest('section')
    fireEvent.click(section.querySelector('button'))
    await screen.findByRole('alert')
    expect(screen.getByText('42')).not.toBeNull()
  })
})
