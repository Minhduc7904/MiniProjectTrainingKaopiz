// @vitest-environment jsdom
import { act, renderHook } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { saveQuizAnswersRequest, startQuizAttemptRequest, submitQuizAttemptRequest } from '@/api/studentQuizApi'
import { QUESTION_TYPE, QUIZ_SAVE_STATE } from '@/constants/quiz'
import { useQuizAttempt } from './useQuizAttempt'

vi.mock('@/api/studentQuizApi', () => ({
  startQuizAttemptRequest: vi.fn(),
  saveQuizAnswersRequest: vi.fn(),
  submitQuizAttemptRequest: vi.fn(),
}))

const quiz = {
  id: 'quiz-1',
  questions: [
    { id: 'q1', questionType: QUESTION_TYPE.single, options: [{ id: 'a' }, { id: 'b' }] },
    { id: 'q2', questionType: QUESTION_TYPE.multiple, options: [{ id: 'c' }, { id: 'd' }] },
  ],
}

function renderAttempt() {
  return renderHook(() => useQuizAttempt({ courseId: 'course-1', lessonId: 'lesson-1', quiz }))
}

async function settle(ms = 0) {
  await act(async () => { await vi.advanceTimersByTimeAsync(ms) })
}

beforeEach(() => {
  vi.useFakeTimers()
  startQuizAttemptRequest.mockResolvedValue({ data: { id: 'attempt-1', answers: [] }, created: true })
  saveQuizAnswersRequest.mockResolvedValue({ data: {} })
})

afterEach(() => {
  vi.useRealTimers()
  vi.clearAllMocks()
})

describe('useQuizAttempt', () => {
  it('restores saved answers when resuming an in-progress attempt', async () => {
    startQuizAttemptRequest.mockResolvedValue({ data: { id: 'attempt-1', answers: [{ questionId: 'q1', optionIds: ['b'] }] }, created: false })

    const { result } = renderAttempt()
    await settle()

    expect(result.current.start.status).toBe('ready')
    expect(result.current.resumed).toBe(true)
    expect(result.current.answers).toEqual({ q1: ['b'] })
  })

  it('debounces rapid selections into one save carrying the latest answers', async () => {
    const { result } = renderAttempt()
    await settle()

    act(() => result.current.select(quiz.questions[0], 'a'))
    act(() => result.current.select(quiz.questions[0], 'b'))
    act(() => result.current.select(quiz.questions[1], 'c'))
    expect(result.current.saveState).toBe(QUIZ_SAVE_STATE.dirty)
    expect(saveQuizAnswersRequest).not.toHaveBeenCalled()
    await settle(600)

    expect(saveQuizAnswersRequest).toHaveBeenCalledTimes(1)
    expect(saveQuizAnswersRequest).toHaveBeenCalledWith('course-1', 'lesson-1', 'quiz-1', 'attempt-1', [
      { questionId: 'q1', optionIds: ['b'] },
      { questionId: 'q2', optionIds: ['c'] },
    ])
    expect(result.current.saveState).toBe(QUIZ_SAVE_STATE.saved)
  })

  it('saves again with the newest answers when the student changes an answer mid-request', async () => {
    let release
    saveQuizAnswersRequest.mockImplementationOnce(() => new Promise((resolve) => { release = resolve }))
    const { result } = renderAttempt()
    await settle()

    act(() => result.current.select(quiz.questions[0], 'a'))
    await settle(600)
    expect(result.current.saveState).toBe(QUIZ_SAVE_STATE.saving)
    act(() => result.current.select(quiz.questions[0], 'b'))
    await act(async () => { release({ data: {} }) })
    await settle(600)

    expect(saveQuizAnswersRequest).toHaveBeenCalledTimes(2)
    expect(saveQuizAnswersRequest).toHaveBeenLastCalledWith('course-1', 'lesson-1', 'quiz-1', 'attempt-1', [
      { questionId: 'q1', optionIds: ['b'] },
      { questionId: 'q2', optionIds: [] },
    ])
    expect(result.current.saveState).toBe(QUIZ_SAVE_STATE.saved)
  })

  it('reports a failed save and retries with backoff until it succeeds', async () => {
    saveQuizAnswersRequest.mockRejectedValueOnce({ message: 'Network Error' })
    const { result } = renderAttempt()
    await settle()

    act(() => result.current.select(quiz.questions[0], 'a'))
    await settle(600)
    expect(result.current.saveState).toBe(QUIZ_SAVE_STATE.error)
    await settle(1000)

    expect(saveQuizAnswersRequest).toHaveBeenCalledTimes(2)
    expect(result.current.saveState).toBe(QUIZ_SAVE_STATE.saved)
    expect(result.current.saveError).toBeNull()
  })

  it('warns before unload only while there are unsaved changes', async () => {
    const { result } = renderAttempt()
    await settle()
    const clean = new Event('beforeunload', { cancelable: true })
    window.dispatchEvent(clean)
    act(() => result.current.select(quiz.questions[0], 'a'))
    const dirty = new Event('beforeunload', { cancelable: true })
    window.dispatchEvent(dirty)

    expect(clean.defaultPrevented).toBe(false)
    expect(dirty.defaultPrevented).toBe(true)
  })

  it('submits the final answers directly and cancels the pending autosave', async () => {
    submitQuizAttemptRequest.mockResolvedValue({ data: { attempt: { id: 'attempt-1' }, result: { score: 1, maxScore: 2 } } })
    const { result } = renderAttempt()
    await settle()
    act(() => result.current.select(quiz.questions[0], 'a'))

    let submitted
    await act(async () => { submitted = await result.current.submitAttempt() })
    await settle(1000)

    expect(submitQuizAttemptRequest).toHaveBeenCalledWith('course-1', 'lesson-1', 'quiz-1', 'attempt-1', [
      { questionId: 'q1', optionIds: ['a'] },
      { questionId: 'q2', optionIds: [] },
    ])
    expect(saveQuizAnswersRequest).not.toHaveBeenCalled()
    expect(submitted.result.score).toBe(1)
    expect(result.current.saveState).toBe(QUIZ_SAVE_STATE.saved)
  })

  it('keeps the answers and exposes the error when submitting fails', async () => {
    submitQuizAttemptRequest.mockRejectedValue({ response: { status: 500, data: { error: { code: 'X', message: 'Lỗi máy chủ' } } } })
    const { result } = renderAttempt()
    await settle()
    act(() => result.current.select(quiz.questions[0], 'a'))

    await act(async () => { await result.current.submitAttempt().catch(() => {}) })

    expect(result.current.submit.error.message).toBe('Lỗi máy chủ')
    expect(result.current.answers).toEqual({ q1: ['a'] })
  })

  it('flushes unsaved answers when the quiz view unmounts', async () => {
    const { result, unmount } = renderAttempt()
    await settle()
    act(() => result.current.select(quiz.questions[0], 'a'))

    unmount()
    await settle()

    expect(saveQuizAnswersRequest).toHaveBeenCalledTimes(1)
  })
})
