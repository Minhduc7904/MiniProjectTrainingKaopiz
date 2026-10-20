// @vitest-environment jsdom
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { fetchQuizAttemptRequest, fetchQuizAttemptsRequest, fetchStudentQuizRequest, saveQuizAnswersRequest, startQuizAttemptRequest, submitQuizAttemptRequest } from '@/api/studentQuizApi'
import { StudentQuizPanel } from './StudentQuizPanel'

vi.mock('@/api/studentQuizApi', () => ({
  fetchStudentQuizRequest: vi.fn(),
  fetchQuizAttemptsRequest: vi.fn(),
  fetchQuizAttemptRequest: vi.fn(),
  startQuizAttemptRequest: vi.fn(),
  saveQuizAnswersRequest: vi.fn(),
  submitQuizAttemptRequest: vi.fn(),
}))

const quiz = {
  id: 'quiz-1',
  title: 'Kiểm tra cuối bài',
  descriptionHtml: '<p>Làm cẩn thận</p>',
  questionCount: 2,
  totalPoints: 5,
  questions: [
    { id: 'q1', questionType: 'SINGLE_CHOICE', points: 2, questionHtml: '<p>2+2?</p>', options: [{ id: 'a', optionHtml: '<p>3</p>' }, { id: 'b', optionHtml: '<p>4</p>' }] },
    { id: 'q2', questionType: 'MULTIPLE_CHOICE', points: 3, questionHtml: '<p>Số chẵn?</p>', options: [{ id: 'c', optionHtml: '<p>2</p>' }, { id: 'd', optionHtml: '<p>3</p>' }] },
  ],
}
const result = {
  score: 2, maxScore: 5, correctCount: 1,
  questions: [
    { questionId: 'q1', answered: true, correct: true, pointsEarned: 2, pointsPossible: 2, selectedOptionIds: ['b'], correctOptionIds: ['b'] },
    { questionId: 'q2', answered: false, correct: false, pointsEarned: 0, pointsPossible: 3, selectedOptionIds: [], correctOptionIds: ['c'] },
  ],
}
const env = (data) => ({ data, meta: {} })

function renderPanel(props = {}) {
  return render(<StudentQuizPanel courseId="course-1" lessonId="lesson-1" quizId="quiz-1" {...props} />)
}

beforeEach(() => {
  fetchStudentQuizRequest.mockResolvedValue(env(quiz))
  fetchQuizAttemptsRequest.mockResolvedValue(env([]))
  saveQuizAnswersRequest.mockResolvedValue(env({}))
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('StudentQuizPanel', () => {
  it('starts a new attempt from the overview, grades on submit and shows the result with icons and text', async () => {
    startQuizAttemptRequest.mockResolvedValue({ ...env({ id: 'attempt-1', answers: [] }), created: true })
    submitQuizAttemptRequest.mockResolvedValue(env({ attempt: { id: 'attempt-1' }, result }))
    const onChanged = vi.fn()
    renderPanel({ onChanged })

    expect(await screen.findByRole('heading', { name: 'Kiểm tra cuối bài' })).not.toBeNull()
    expect(screen.getByText('Không giới hạn thời gian')).not.toBeNull()
    fireEvent.click(screen.getByRole('button', { name: /Bắt đầu làm bài/ }))
    fireEvent.click(await screen.findByRole('radio', { name: 'Đáp án B' }))
    fireEvent.click(screen.getAllByRole('button', { name: 'Nộp bài' })[0])
    const dialog = await screen.findByRole('dialog')
    fireEvent.click(within(dialog).getByRole('button', { name: 'Nộp bài' }))

    expect(await screen.findByText('2/5 điểm')).not.toBeNull()
    expect(screen.getByText('1 đúng')).not.toBeNull()
    expect(screen.getByText('1 bỏ trống')).not.toBeNull()
    expect(submitQuizAttemptRequest).toHaveBeenCalledWith('course-1', 'lesson-1', 'quiz-1', 'attempt-1', [
      { questionId: 'q1', optionIds: ['b'] },
      { questionId: 'q2', optionIds: [] },
    ])
    await waitFor(() => expect(onChanged).toHaveBeenCalled())
  })

  it('resumes an in-progress attempt with the previously saved answers', async () => {
    fetchQuizAttemptsRequest.mockResolvedValue(env([
      { id: 'attempt-1', status: 'IN_PROGRESS', startedAtUtc: '2026-10-20T08:00:00Z', updatedAtUtc: new Date().toISOString(), score: null, maxScore: null, answeredQuestions: 1 },
    ]))
    startQuizAttemptRequest.mockResolvedValue({ ...env({ id: 'attempt-1', updatedAtUtc: new Date().toISOString(), answers: [{ questionId: 'q1', optionIds: ['b'] }] }), created: false })
    renderPanel()

    fireEvent.click(await screen.findByRole('button', { name: /Tiếp tục làm bài \(đã trả lời 1\/2\)/ }))

    expect((await screen.findByRole('radio', { name: 'Đáp án B' })).checked).toBe(true)
    expect(screen.getByText(/Đã khôi phục bài làm dở/, { selector: 'p' })).not.toBeNull()
  })

  it('opens a submitted attempt from history and reviews it', async () => {
    fetchQuizAttemptsRequest.mockResolvedValue(env([
      { id: 'attempt-9', status: 'SUBMITTED', startedAtUtc: '2026-10-19T08:00:00Z', updatedAtUtc: '2026-10-19T08:10:00Z', submittedAtUtc: '2026-10-19T08:10:00Z', score: 2, maxScore: 5, answeredQuestions: 1 },
    ]))
    fetchQuizAttemptRequest.mockResolvedValue(env({ attempt: { id: 'attempt-9' }, result }))
    renderPanel()

    fireEvent.click(await screen.findByRole('button', { name: /2\/5 điểm · 40%/ }))

    expect(await screen.findByText('Xem lại bài làm')).not.toBeNull()
    expect(screen.getAllByText('Đáp án đúng').length).toBeGreaterThan(0)
    expect(fetchQuizAttemptRequest).toHaveBeenCalledWith('course-1', 'lesson-1', 'quiz-1', 'attempt-9', expect.anything())
  })

  it('shows a retryable error when the quiz cannot be loaded', async () => {
    fetchStudentQuizRequest.mockRejectedValueOnce({ response: { status: 403, data: { error: { code: 'STUDENT_NOT_ENROLLED', message: 'Chưa ghi danh' } } } })
    renderPanel()

    expect((await screen.findByRole('alert')).textContent).toBe('Chưa ghi danh')
    fetchStudentQuizRequest.mockResolvedValue(env(quiz))
    fireEvent.click(screen.getByRole('button', { name: 'Thử lại' }))
    expect(await screen.findByRole('heading', { name: 'Kiểm tra cuối bài' })).not.toBeNull()
  })
})
