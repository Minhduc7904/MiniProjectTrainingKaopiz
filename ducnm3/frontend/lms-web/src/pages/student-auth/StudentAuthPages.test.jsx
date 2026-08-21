// @vitest-environment jsdom
import { afterEach, describe, expect, it, vi } from 'vitest'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'

vi.mock('@/auth/studentAuthStorage', () => ({
  clearStudentActor: vi.fn(),
  readStudentActor: () => null,
  writeStudentActor: vi.fn(),
}))

vi.mock('@/auth/studentSession', () => ({
  getVerifiedStudentProfile: () => ({
    displayName: 'Student One',
    email: 'student@example.com',
    id: '11111111-1111-1111-1111-111111111111',
    status: 'ACTIVE',
  }),
  verifyStudentSession: vi.fn(),
}))

import { StudentHomePage, StudentLoadingPage } from '@/pages/student-auth/StudentAuthPages'
import { StudentInput } from '@/components/ui/student/StudentInput'

afterEach(cleanup)

describe('StudentHomePage', () => {
  it('shows a learning-focused empty dashboard after a valid Student session', () => {
    render(
      <MemoryRouter>
        <StudentHomePage />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Tiếp tục hành trình học' })).not.toBeNull()
    expect(screen.getByText('Bạn chưa có khóa học đang học.')).not.toBeNull()
    expect(screen.getByText('Student One')).not.toBeNull()
    expect(screen.getByRole('link', { name: 'Xem thông tin người dùng' })).not.toBeNull()
    expect(screen.getByRole('link', { name: 'Đăng xuất' })).not.toBeNull()
  })

  it('renders Student controls with accessible labels and form metadata', () => {
    render(<StudentInput autoComplete="email" id="email" label="Email" name="email" type="email" value="" onChange={() => {}} />)

    const input = screen.getByLabelText('Email')
    expect(input.className).toContain('student-control')
    expect(input.getAttribute('name')).toBe('email')
    expect(input.getAttribute('autocomplete')).toBe('email')
  })

  it('keeps the loading redirect when no Student actor is stored', async () => {
    render(
      <MemoryRouter initialEntries={['/student/loading']}>
        <Routes>
          <Route path="/student/loading" element={<StudentLoadingPage />} />
          <Route path="/student/login" element={<p>Login target</p>} />
        </Routes>
      </MemoryRouter>,
    )

    await waitFor(() => expect(screen.getByText('Login target')).not.toBeNull())
  })
})
