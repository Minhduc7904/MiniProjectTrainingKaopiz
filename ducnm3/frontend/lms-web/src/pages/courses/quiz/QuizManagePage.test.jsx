// @vitest-environment jsdom
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchCourseLessonDetailRequest } from '@/api/coursesApi'
import { createQuizRequest, fetchQuizDetailRequest, fetchQuizzesRequest, updateQuizRequest } from '@/api/quizApi'
import { QuizManagePage } from './QuizManagePage'

vi.mock('@/api/coursesApi', () => ({ fetchCourseLessonDetailRequest: vi.fn() }))
vi.mock('@/api/quizApi', () => ({
  fetchQuizzesRequest: vi.fn(),
  fetchQuizDetailRequest: vi.fn(),
  createQuizRequest: vi.fn(),
  updateQuizRequest: vi.fn(),
  deleteQuizRequest: vi.fn(),
}))
vi.mock('@/components/media/MediaLibraryModal', () => ({ MediaLibraryModal: () => null }))

const env = (data) => ({ data, meta: {} })
const summary = { id: 'quiz-1', title: 'Kiểm tra 1', status: 'PUBLISHED', questionCount: 1, totalPoints: 2, hasAttempts: false }
const detail = (hasAttempts = false) => ({
  ...summary, hasAttempts, descriptionMarkdown: 'Mô tả', descriptionHtml: '<p>Mô tả</p>',
  questions: [{ id: 'q1', questionMarkdown: 'Thủ đô?', questionHtml: '<p>Thủ đô?</p>', questionType: 'SINGLE_CHOICE', points: 2, options: [
    { id: 'a', optionMarkdown: 'Hà Nội', optionHtml: '<p>Hà Nội</p>', isCorrect: true },
    { id: 'b', optionMarkdown: 'Huế', optionHtml: '<p>Huế</p>', isCorrect: false },
  ] }],
})

function renderPage() {
  return render(
    <MemoryRouter initialEntries={['/admin/course/courses/course-1/lessons/lesson-1/quizzes']}>
      <Routes><Route path="/admin/course/courses/:courseId/lessons/:lessonId/quizzes" element={<QuizManagePage />} /></Routes>
    </MemoryRouter>,
  )
}

beforeEach(() => {
  fetchCourseLessonDetailRequest.mockResolvedValue(env({ title: 'Bài 1' }))
  fetchQuizzesRequest.mockResolvedValue(env([summary]))
  fetchQuizDetailRequest.mockResolvedValue(env(detail()))
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('QuizManagePage', () => {
  it('lists the lesson quizzes and shows the rendered detail with the correct answer marked', async () => {
    renderPage()

    fireEvent.click(await screen.findByRole('button', { name: /Kiểm tra 1/ }))

    expect(await screen.findByText('Đáp án đúng')).not.toBeNull()
    expect(screen.getByText('Thủ đô?')).not.toBeNull()
    expect(fetchQuizDetailRequest).toHaveBeenCalledWith('course-1', 'lesson-1', 'quiz-1')
  })

  it('blocks saving an invalid new quiz and shows field errors without calling the API', async () => {
    renderPage()
    fireEvent.click(await screen.findByRole('button', { name: 'Tạo quiz' }))

    fireEvent.submit(screen.getByLabelText('Tiêu đề quiz').closest('form'))

    expect((await screen.findAllByRole('alert')).length).toBeGreaterThan(0)
    expect(screen.getByText(/Còn \d+ lỗi cần sửa/)).not.toBeNull()
    expect(createQuizRequest).not.toHaveBeenCalled()
  })

  it('creates a quiz with the typed content', async () => {
    createQuizRequest.mockResolvedValue(env(detail()))
    renderPage()
    fireEvent.click(await screen.findByRole('button', { name: 'Tạo quiz' }))

    fireEvent.change(screen.getByLabelText('Tiêu đề quiz'), { target: { value: 'Kiểm tra mới' } })
    fireEvent.change(screen.getByLabelText('Nội dung câu hỏi'), { target: { value: 'Câu 1?' } })
    fireEvent.change(screen.getByLabelText('Lựa chọn A'), { target: { value: 'Đúng' } })
    fireEvent.change(screen.getByLabelText('Lựa chọn B'), { target: { value: 'Sai' } })
    fireEvent.submit(screen.getByLabelText('Tiêu đề quiz').closest('form'))

    await waitFor(() => expect(createQuizRequest).toHaveBeenCalled())
    const [, , payload] = createQuizRequest.mock.calls[0]
    expect(payload).toMatchObject({ title: 'Kiểm tra mới', status: 'DRAFT' })
    expect(payload.questions[0]).toMatchObject({ questionMarkdown: 'Câu 1?', points: 1, questionType: 'SINGLE_CHOICE' })
    expect(payload.questions[0].options).toEqual([{ optionMarkdown: 'Đúng', isCorrect: true }, { optionMarkdown: 'Sai', isCorrect: false }])
  })

  it('locks the question tree once attempts exist and sends only metadata', async () => {
    fetchQuizDetailRequest.mockResolvedValue(env(detail(true)))
    updateQuizRequest.mockResolvedValue(env(detail(true)))
    renderPage()
    fireEvent.click(await screen.findByRole('button', { name: /Kiểm tra 1/ }))
    fireEvent.click(await screen.findByRole('button', { name: /Sửa/ }))

    expect(screen.getByText(/chỉ sửa được tiêu đề, mô tả và trạng thái/)).not.toBeNull()
    expect(screen.queryByLabelText('Nội dung câu hỏi')).toBeNull()
    fireEvent.change(screen.getByLabelText('Tiêu đề quiz'), { target: { value: 'Tên mới' } })
    fireEvent.submit(screen.getByLabelText('Tiêu đề quiz').closest('form'))

    await waitFor(() => expect(updateQuizRequest).toHaveBeenCalled())
    expect(updateQuizRequest.mock.calls[0][3]).toEqual({ title: 'Tên mới', descriptionMarkdown: 'Mô tả', status: 'PUBLISHED' })
  })
})
