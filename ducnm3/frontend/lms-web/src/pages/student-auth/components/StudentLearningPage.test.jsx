// @vitest-environment jsdom
import { describe, expect, it, vi } from 'vitest'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import { StudentLearningPage } from './StudentLearningPage'
import { studentLearningReducer } from '@/features/studentLearning/studentLearningSlice'
import { fetchStudentEnrollmentDetailRequest, fetchStudentLessonDetailRequest } from '@/api/studentLearningApi'

vi.mock('@/api/studentLearningApi', () => ({
  completeStudentLessonRequest: vi.fn().mockResolvedValue({ data: { lessonId: 'lesson-1', progressPercent: 100, completedAtUtc: '2026-08-22T00:00:00Z' }, meta: { traceId: 'trace-complete' } }),
  fetchStudentEnrollmentDetailRequest: vi.fn(),
  fetchStudentLessonDetailRequest: vi.fn(),
  fetchStudentMediaContentRequest: vi.fn().mockResolvedValue(new Blob(['image'])),
}))

const course = { id: 'course-1', name: 'React căn bản', lessons: [{ id: 'lesson-1', title: 'Mở đầu', displayOrder: 1, progressPercent: 0, completedAtUtc: null }, { id: 'lesson-2', title: 'Props', displayOrder: 2, progressPercent: 100, completedAtUtc: '2026-08-20T00:00:00Z' }] }
const lesson = { id: 'lesson-1', courseId: 'course-1', title: 'Mở đầu', contentHtml: '<h2>Chào mừng</h2>', attachments: [{ usageId: 'usage-image', contentUrl: 'https://example.test/image.jpg', thumbnailUrl: null, mediaType: 'IMAGE', contentType: 'image/jpeg', originalFileName: 'intro.jpg' }] }

describe('StudentLearningPage', () => {
  it('keeps the lesson list and media rail fixed while selecting media and completing the current lesson', async () => {
    fetchStudentEnrollmentDetailRequest.mockResolvedValue({ data: course, meta: { traceId: 'trace-course' } })
    fetchStudentLessonDetailRequest.mockResolvedValue({ data: lesson, meta: { traceId: 'trace-lesson' } })
    const store = configureStore({ reducer: { studentLearning: studentLearningReducer }, preloadedState: { studentLearning: { enrollments: { data: [], pagination: null, query: {}, loading: false, success: false, error: null, traceId: null }, catalog: { data: [], pagination: null, query: {}, loading: false, success: false, error: null, traceId: null }, detail: { data: course, loading: false, error: null, traceId: 'trace-course' }, lessonDetail: { data: lesson, loading: false, error: null, traceId: 'trace-lesson' }, completion: { loading: false, error: null, traceId: null }, progressByCourseId: {}, enrollmentByCourseId: {} } } })

    render(<Provider store={store}><MemoryRouter initialEntries={['/student/courses/course-1/learn/lesson-1']}><Routes><Route path="/student/courses/:courseId/learn/:lessonId" element={<StudentLearningPage student={{ displayName: 'Student', email: 'student@example.test' }} />} /></Routes></MemoryRouter></Provider>)

    expect(screen.getByLabelText('Danh sách bài học').className).toContain('overflow-y-auto')
    expect(screen.getByLabelText('Nội dung bài học').className).toContain('overflow-y-auto')
    expect(screen.getByRole('button', { name: 'Nội dung' }).getAttribute('aria-pressed')).toBe('true')
    expect(await screen.findByText('Chào mừng')).not.toBeNull()
    expect(screen.getByRole('button', { name: 'Bài trước' }).disabled).toBe(true)
    fireEvent.click(screen.getByRole('button', { name: 'Bài tiếp theo' }))
    await waitFor(() => expect(screen.getByText('Bài 2/2')).not.toBeNull())
    fireEvent.click(screen.getByRole('button', { name: 'Bài trước' }))
    await waitFor(() => expect(screen.getByText('Bài 1/2')).not.toBeNull())
    fireEvent.click(screen.getByRole('button', { name: 'Xem intro.jpg' }))
    expect(screen.getByRole('heading', { name: 'intro.jpg' })).not.toBeNull()
    fireEvent.click(screen.getByRole('button', { name: 'Nội dung' }))
    fireEvent.click(screen.getByRole('button', { name: 'Hoàn thành bài học' }))
    expect(await screen.findByText('Đã hoàn thành')).not.toBeNull()
  })
})
