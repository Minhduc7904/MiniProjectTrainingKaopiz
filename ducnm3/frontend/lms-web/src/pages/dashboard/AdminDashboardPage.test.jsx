// @vitest-environment jsdom

import { configureStore } from '@reduxjs/toolkit'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { Provider } from 'react-redux'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { httpClient } from '@/api/httpClient'
import { dashboardReducer } from '@/features/dashboard/dashboardSlice'
import { AdminDashboardPage } from '@/pages/dashboard/AdminDashboardPage'

afterEach(() => vi.restoreAllMocks())

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

describe('AdminDashboardPage', () => {
  it('renders metrics, all health cards and no sidebar, then refreshes all requests', async () => {
    const get = vi.spyOn(httpClient, 'get').mockImplementation((route) => Promise.resolve(responseFor(route)))
    const store = configureStore({ reducer: { dashboard: dashboardReducer } })
    const view = render(<Provider store={store}><MemoryRouter><AdminDashboardPage /></MemoryRouter></Provider>)

    await screen.findByText('42')
    await screen.findByText('18')
    await screen.findByText('37')
    expect(screen.getByText('API Gateway')).not.toBeNull()
    expect(screen.getByText('Scheduler Service')).not.toBeNull()
    expect(screen.getByText('Màn hình quản lý đang được phát triển').closest('[aria-disabled]')?.getAttribute('aria-disabled')).toBe('true')
    expect(view.container.querySelector('aside')).toBeNull()
    expect(get).toHaveBeenCalledTimes(9)

    fireEvent.click(screen.getByRole('button', { name: /Kiểm tra lại toàn bộ/i }))
    await waitFor(() => expect(get).toHaveBeenCalledTimes(18))
  })
})
