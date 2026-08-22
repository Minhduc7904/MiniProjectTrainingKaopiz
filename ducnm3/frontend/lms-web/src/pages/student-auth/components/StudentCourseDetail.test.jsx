// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import { StudentCourseDetail } from './StudentCourseDetail'
import { studentLearningReducer } from '@/features/studentLearning/studentLearningSlice'

vi.mock('react-redux', async (importOriginal) => {
  const actual = await importOriginal()
  return { ...actual, useDispatch: () => () => Promise.resolve() }
})

vi.mock('@/components/ui/student', () => ({
  StudentAccountMenu: () => null,
  StudentNavigation: () => null,
  StudentShell: ({ children, scrollable }) => <main data-testid="student-shell" data-scrollable={String(scrollable)}>{children}</main>,
  StudentCourseThumbnail: ({ alt, className, contentUrl }) => <img alt={alt} className={className} src={contentUrl} />,
}))

vi.mock('@/api/studentLearningApi', () => ({
  fetchStudentEnrollmentDetailRequest: vi.fn().mockResolvedValue({ data: null, meta: { traceId: 'trace-course' } }),
}))

const course = {
  id: 'course-1',
  name: 'React căn bản',
  status: 'PUBLISHED',
  gallery: [
    { contentUrl: 'https://example.test/first.jpg' },
    { contentUrl: 'https://example.test/second.jpg' },
  ],
  lessons: [],
}

function renderDetail() {
  const store = configureStore({
    reducer: { studentLearning: studentLearningReducer },
    preloadedState: {
      studentLearning: {
        enrollments: { data: [], pagination: null, query: {}, loading: false, success: false, error: null, traceId: null },
        catalog: { data: [], pagination: null, query: {}, loading: false, success: false, error: null, traceId: null },
        detail: { data: course, loading: false, error: null, traceId: 'trace-course' },
        progressByCourseId: {},
        enrollmentByCourseId: {},
      },
    },
  })

  return render(
    <Provider store={store}>
      <MemoryRouter initialEntries={['/student/courses/course-1']}>
        <Routes><Route path="/student/courses/:courseId" element={<StudentCourseDetail student={{ displayName: 'Student', email: 'student@example.test' }} />} /></Routes>
      </MemoryRouter>
    </Provider>,
  )
}

describe('StudentCourseDetail', () => {
  it('uses a scrollable Student shell and smoothly advances the image gallery', () => {
    renderDetail()

    expect(screen.getByTestId('student-shell').dataset.scrollable).toBe('true')
    expect(screen.getByAltText('React căn bản - ảnh 1').className).toContain('student-gallery-slide')

    fireEvent.click(screen.getByRole('button', { name: 'Ảnh tiếp theo' }))

    expect(screen.getByAltText('React căn bản - ảnh 2')).not.toBeNull()
  })
})
