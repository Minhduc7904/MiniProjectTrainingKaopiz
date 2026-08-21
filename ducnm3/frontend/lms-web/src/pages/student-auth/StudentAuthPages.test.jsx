// @vitest-environment jsdom
import { afterEach, describe, expect, it, vi } from 'vitest'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'

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

vi.mock('@/api/studentLearningApi', () => ({
  enrollStudentInCourseRequest: vi.fn().mockResolvedValue({ data: { id: '33333333-3333-3333-3333-333333333333' }, meta: { traceId: 'trace-enroll' } }),
  fetchMyCourseProgressRequest: vi.fn(),
  fetchStudentCourseCatalogRequest: vi.fn().mockResolvedValue({
    data: [{ courseId: '22222222-2222-2222-2222-222222222222', name: 'Catalog Course', status: 'PUBLISHED', thumbnailUrl: null }],
    meta: { pagination: { page: 1, pageSize: 12, totalItems: 1, totalPages: 1 }, traceId: 'trace-catalog' },
  }),
  fetchStudentEnrollmentDetailRequest: vi.fn(),
  fetchStudentEnrollmentsRequest: vi.fn().mockResolvedValue({
    data: [],
    meta: { pagination: { page: 1, pageSize: 12, totalItems: 0, totalPages: 0 }, traceId: 'trace-1' },
  }),
}))

import { StudentCoursesPage, StudentHomePage, StudentLoadingPage } from '@/pages/student-auth/StudentAuthPages'
import { StudentInput } from '@/components/ui/student/StudentInput'
import { studentLearningReducer } from '@/features/studentLearning/studentLearningSlice'
import { StudentDashboard } from '@/pages/student-auth/components/StudentDashboard'

afterEach(cleanup)

describe('StudentHomePage', () => {
  it('shows a learning-focused empty dashboard after a valid Student session', async () => {
    render(<Provider store={configureStore({ reducer: { studentLearning: studentLearningReducer } })}><MemoryRouter><StudentHomePage /></MemoryRouter></Provider>)

    expect(screen.getByRole('heading', { name: 'Tiếp tục hành trình học' })).not.toBeNull()
    await waitFor(() => expect(screen.getByText('Bạn chưa có khóa học đang học.')).not.toBeNull())
    expect(screen.getByText('Student One')).not.toBeNull()
    expect(screen.getByRole('link', { name: 'Xem thông tin người dùng' })).not.toBeNull()
    expect(screen.getByRole('link', { name: 'Đăng xuất' })).not.toBeNull()
    expect(screen.getByRole('link', { name: 'Home' })).not.toBeNull()
    expect(screen.getByRole('link', { name: 'Khóa học' })).not.toBeNull()
  })

  it('renders Student controls with accessible labels and form metadata', () => {
    render(<StudentInput autoComplete="email" id="email" label="Email" name="email" type="email" value="" onChange={() => {}} />)

    const input = screen.getByLabelText('Email')
    expect(input.className).toContain('student-control')
    expect(input.getAttribute('name')).toBe('email')
    expect(input.getAttribute('autocomplete')).toBe('email')
  })

  it('renders an enrolled course card that leads to the read-only Course detail', () => {
    const courseId = '22222222-2222-2222-2222-222222222222'
    const store = configureStore({
      reducer: { studentLearning: studentLearningReducer },
      preloadedState: {
        studentLearning: {
          enrollments: { data: [{ enrollmentId: '33333333-3333-3333-3333-333333333333', courseId, name: 'Backend Fundamentals', thumbnailUrl: null }], pagination: { page: 1, pageSize: 12, totalItems: 1, totalPages: 1 }, query: { page: 1, pageSize: 12 }, loading: false, success: true, error: null, traceId: null },
          detail: { data: null, loading: false, error: null, traceId: null },
          progressByCourseId: { [courseId]: { loading: false, data: { progressPercent: 50, completedLessons: 1, totalLessons: 2 }, error: null } },
        },
      },
    })

    render(<Provider store={store}><MemoryRouter><StudentDashboard student={{ displayName: 'Student One', email: 'student@example.com' }} /></MemoryRouter></Provider>)

    expect(screen.getByRole('link', { name: 'Mở khóa học Backend Fundamentals' }).getAttribute('href')).toBe(`/student/courses/${courseId}`)
    expect(screen.getByText('50%')).not.toBeNull()
  })

  it('redirects to Course detail after a successful enrollment from the catalog', async () => {
    render(
      <Provider store={configureStore({ reducer: { studentLearning: studentLearningReducer } })}>
        <MemoryRouter initialEntries={['/student/courses']}>
          <Routes>
            <Route path="/student/courses" element={<StudentCoursesPage />} />
            <Route path="/student/courses/:courseId" element={<p>Course detail target</p>} />
          </Routes>
        </MemoryRouter>
      </Provider>,
    )

    const enrollButton = await screen.findByRole('button', { name: 'Đăng kí Catalog Course' })
    enrollButton.click()

    await waitFor(() => expect(screen.getByText('Course detail target')).not.toBeNull())
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
