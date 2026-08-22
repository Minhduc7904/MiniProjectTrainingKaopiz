// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom'
import { Sidebar } from './Sidebar'

vi.mock('@/auth/actorStorage', () => ({
  readAdmin: () => ({ displayName: 'Admin', id: 'admin-id' }),
}))

function CurrentPath() {
  return <p data-testid="current-path">{useLocation().pathname}</p>
}

describe('Sidebar', () => {
  it('returns to the Admin dashboard when the MiniLMS brand is selected', () => {
    render(
      <MemoryRouter initialEntries={['/admin/course/courses']}>
        <Sidebar />
        <Routes>
          <Route path="*" element={<CurrentPath />} />
        </Routes>
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('link', { name: /Mini LMS/i }))

    expect(screen.getByTestId('current-path').textContent).toBe('/admin/dashboard')
  })
})
