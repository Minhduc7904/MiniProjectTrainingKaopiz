// @vitest-environment jsdom
import { act, cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  createLessonAiJobRequest,
  fetchLessonAiDocumentsRequest,
  fetchLessonAiJobsRequest,
  syncLessonAiDocumentsRequest,
} from '@/api/lessonAiApi'
import { LESSON_AI_POLL_INTERVAL_MS } from '@/constants/lessonAi'
import { LessonAiPanel } from './LessonAiPanel'

vi.mock('@/api/lessonAiApi', () => ({
  fetchLessonAiDocumentsRequest: vi.fn(),
  fetchLessonAiJobsRequest: vi.fn(),
  syncLessonAiDocumentsRequest: vi.fn(),
  createLessonAiJobRequest: vi.fn(),
}))

const env = (data) => ({ data, meta: {} })
const doc = (mediaId, fileName, status, extra = {}) => ({ mediaId, fileName, contentType: 'text/plain', mediaType: 'DOCUMENT', status, errorCode: null, errorMessage: null, textLength: 10, updatedAtUtc: null, ...extra })
const job = (id, jobType, status, extra = {}) => ({ id, lessonId: 'lesson-1', courseId: 'course-1', jobType, status, mediaIds: ['m1'], questionCount: null, overwriteContent: false, hasPreviousContent: false, resultQuizId: null, stepCount: 0, errorCode: null, errorMessage: null, createdAtUtc: '2026-08-20T01:00:00Z', ...extra })

const documents = [
  doc('m1', 'bai-giang.pdf', 'EXTRACTED'),
  doc('m2', 'de-cuong.docx', 'EXTRACTED'),
  doc('m3', 'dang-doc.pdf', 'EXTRACTING'),
  doc('m4', 'hong.pdf', 'FAILED', { errorCode: 'AI_TASK_NOT_CONFIGURED' }),
  doc('m5', 'video.mp4', 'UNSUPPORTED', { errorCode: 'UNSUPPORTED_FILE_TYPE' }),
]

function renderPanel(props = {}) {
  return render(<LessonAiPanel courseId="course-1" lessonId="lesson-1" attachmentKey="m1,m2" lessonHasContent={false} onLessonContentChanged={vi.fn()} onOpenQuizzes={vi.fn()} {...props} />)
}

beforeEach(() => {
  HTMLDialogElement.prototype.showModal = vi.fn(function showModal() { this.open = true })
  HTMLDialogElement.prototype.close = vi.fn(function close() { this.open = false })
  syncLessonAiDocumentsRequest.mockResolvedValue(env({ items: documents }))
  fetchLessonAiDocumentsRequest.mockResolvedValue(env({ items: documents }))
  fetchLessonAiJobsRequest.mockResolvedValue(env({ items: [] }))
  createLessonAiJobRequest.mockResolvedValue(env(job('j1', 'GENERATE_QUIZ', 'PENDING')))
})

afterEach(() => {
  cleanup()
  vi.useRealTimers()
  vi.clearAllMocks()
})

async function ready() {
  await screen.findByText('bai-giang.pdf')
}

describe('LessonAiPanel', () => {
  it('syncs documents on open, selects nothing by default and locks both actions', async () => {
    renderPanel()
    await ready()

    expect(syncLessonAiDocumentsRequest).toHaveBeenCalledWith('course-1', 'lesson-1', expect.any(Object))
    for (const checkbox of screen.getAllByRole('checkbox')) expect(checkbox.checked).toBe(false)
    expect(screen.getByRole('button', { name: 'Tự động tổng hợp bài học' }).disabled).toBe(true)
    expect(screen.getByRole('button', { name: 'Tự động tạo quiz' }).disabled).toBe(true)
    expect(screen.getByText('Chọn ít nhất một tài liệu đã trích xuất.')).not.toBeNull()
  })

  it('only lets extracted documents be selected and explains failed or unsupported ones', async () => {
    renderPanel()
    await ready()

    expect(screen.getByLabelText(/bai-giang\.pdf/).disabled).toBe(false)
    expect(screen.getByLabelText(/dang-doc\.pdf/).disabled).toBe(true)
    expect(screen.getByLabelText(/hong\.pdf/).disabled).toBe(true)
    expect(screen.getByLabelText(/video\.mp4/).disabled).toBe(true)
    expect(screen.getByText(/Chưa gán model cho tác vụ này/)).not.toBeNull()
    expect(screen.getByText(/Chỉ hỗ trợ PDF, DOCX/)).not.toBeNull()
  })

  it('creates a quiz job with only the selected documents and the chosen question count', async () => {
    renderPanel()
    await ready()

    fireEvent.click(screen.getByLabelText(/de-cuong\.docx/))
    fireEvent.change(screen.getByLabelText('Số câu hỏi'), { target: { value: '5' } })
    fireEvent.click(screen.getByRole('button', { name: 'Tự động tạo quiz' }))

    await waitFor(() => expect(createLessonAiJobRequest).toHaveBeenCalledWith('course-1', 'lesson-1', { jobType: 'GENERATE_QUIZ', mediaIds: ['m2'], questionCount: 5 }))
    expect(await screen.findByText('Tạo quiz')).not.toBeNull()
  })

  it('synthesizes straight away when the lesson is empty', async () => {
    createLessonAiJobRequest.mockResolvedValue(env(job('j2', 'SYNTHESIZE_LESSON', 'PENDING')))
    renderPanel()
    await ready()

    fireEvent.click(screen.getByLabelText(/bai-giang\.pdf/))
    fireEvent.click(screen.getByRole('button', { name: 'Tự động tổng hợp bài học' }))

    await waitFor(() => expect(createLessonAiJobRequest).toHaveBeenCalledWith('course-1', 'lesson-1', { jobType: 'SYNTHESIZE_LESSON', mediaIds: ['m1'] }))
  })

  it('asks before overwriting existing lesson content and then sends overwriteContent', async () => {
    createLessonAiJobRequest.mockResolvedValue(env(job('j2', 'SYNTHESIZE_LESSON', 'PENDING')))
    renderPanel({ lessonHasContent: true })
    await ready()

    fireEvent.click(screen.getByLabelText(/bai-giang\.pdf/))
    fireEvent.click(screen.getByRole('button', { name: 'Tự động tổng hợp bài học' }))

    expect(createLessonAiJobRequest).not.toHaveBeenCalled()
    expect(screen.getByText('Ghi đè nội dung lesson?')).not.toBeNull()
    fireEvent.click(screen.getByRole('button', { name: 'Ghi đè' }))

    await waitFor(() => expect(createLessonAiJobRequest).toHaveBeenCalledWith('course-1', 'lesson-1', { jobType: 'SYNTHESIZE_LESSON', mediaIds: ['m1'], overwriteContent: true }))
  })

  it('does not call the API when the overwrite prompt is cancelled', async () => {
    renderPanel({ lessonHasContent: true })
    await ready()

    fireEvent.click(screen.getByLabelText(/bai-giang\.pdf/))
    fireEvent.click(screen.getByRole('button', { name: 'Tự động tổng hợp bài học' }))
    fireEvent.click(screen.getByRole('button', { name: 'Hủy' }))

    expect(createLessonAiJobRequest).not.toHaveBeenCalled()
  })

  it('disables an action while a job of the same type is still running', async () => {
    fetchLessonAiJobsRequest.mockResolvedValue(env({ items: [job('j1', 'GENERATE_QUIZ', 'RUNNING')] }))
    renderPanel()
    await ready()

    fireEvent.click(screen.getByLabelText(/bai-giang\.pdf/))

    expect(screen.getByRole('button', { name: 'Tự động tạo quiz' }).disabled).toBe(true)
    expect(screen.getByRole('button', { name: 'Tự động tổng hợp bài học' }).disabled).toBe(false)
  })

  it('shows the failure reason of a failed job and the review notice with a quiz link for a completed one', async () => {
    const onOpenQuizzes = vi.fn()
    fetchLessonAiJobsRequest.mockResolvedValue(env({ items: [
      job('j2', 'GENERATE_QUIZ', 'COMPLETED', { resultQuizId: 'quiz-1' }),
      job('j1', 'SYNTHESIZE_LESSON', 'FAILED', { errorCode: 'AI_PROVIDER_AUTH_FAILED' }),
    ] }))
    renderPanel({ onOpenQuizzes })
    await ready()

    expect(screen.getByText(/Provider từ chối API key/)).not.toBeNull()
    expect(screen.getByText(/cần kiểm tra trước khi cho học viên dùng/)).not.toBeNull()
    fireEvent.click(screen.getByRole('button', { name: 'Mở quiz nháp' }))
    expect(onOpenQuizzes).toHaveBeenCalledWith('lesson-1')
  })

  it('retries failed documents through the sync endpoint', async () => {
    renderPanel()
    await ready()

    fireEvent.click(screen.getByRole('button', { name: 'Thử lại tài liệu lỗi' }))

    await waitFor(() => expect(syncLessonAiDocumentsRequest).toHaveBeenCalledWith('course-1', 'lesson-1', { retryFailed: true }))
  })

  it('polls silently while documents are extracting and drops selections that are no longer valid', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true })
    renderPanel()
    await ready()
    fireEvent.click(screen.getByLabelText(/bai-giang\.pdf/))
    fetchLessonAiDocumentsRequest.mockResolvedValue(env({ items: [doc('m1', 'bai-giang.pdf', 'FAILED'), doc('m3', 'dang-doc.pdf', 'EXTRACTED')] }))

    await act(async () => { await vi.advanceTimersByTimeAsync(LESSON_AI_POLL_INTERVAL_MS + 50) })

    expect(fetchLessonAiDocumentsRequest).toHaveBeenCalledWith('course-1', 'lesson-1', expect.objectContaining({ silent: true }))
    expect(screen.getByLabelText(/dang-doc\.pdf/).disabled).toBe(false)
    expect(screen.getByText('Đã chọn 0 tài liệu')).not.toBeNull()
  })

  it('tells the page to reload the lesson once a watched synthesis job completes', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true })
    const onLessonContentChanged = vi.fn()
    fetchLessonAiJobsRequest.mockResolvedValue(env({ items: [job('j9', 'SYNTHESIZE_LESSON', 'RUNNING')] }))
    renderPanel({ onLessonContentChanged })
    await ready()
    expect(onLessonContentChanged).not.toHaveBeenCalled()
    fetchLessonAiJobsRequest.mockResolvedValue(env({ items: [job('j9', 'SYNTHESIZE_LESSON', 'COMPLETED')] }))

    await act(async () => { await vi.advanceTimersByTimeAsync(LESSON_AI_POLL_INTERVAL_MS + 50) })

    expect(onLessonContentChanged).toHaveBeenCalledTimes(1)
  })

  it('does not reload the lesson for a synthesis job that was already finished when the panel opened', async () => {
    const onLessonContentChanged = vi.fn()
    fetchLessonAiJobsRequest.mockResolvedValue(env({ items: [job('j9', 'SYNTHESIZE_LESSON', 'COMPLETED')] }))
    renderPanel({ onLessonContentChanged })
    await ready()

    expect(onLessonContentChanged).not.toHaveBeenCalled()
  })

  it('shows the load error without crashing', async () => {
    syncLessonAiDocumentsRequest.mockRejectedValue({ response: { status: 503, data: { error: { code: 'SERVICE_UNAVAILABLE', message: 'Service is temporarily unavailable.' }, meta: {} } } })
    renderPanel()

    expect(await screen.findByText(/Không tải được trạng thái tài liệu AI/)).not.toBeNull()
  })
})
