import { useCallback, useEffect, useRef, useState } from 'react'
import {
  createLessonAiJobRequest,
  fetchLessonAiDocumentsRequest,
  fetchLessonAiJobsRequest,
  syncLessonAiDocumentsRequest,
} from '@/api/lessonAiApi'
import { toApiError } from '@/api/toApiError'
import {
  LESSON_AI_DOCUMENT_STATUS,
  LESSON_AI_JOB_STATUS,
  LESSON_AI_JOB_TYPE,
  LESSON_AI_POLL_INTERVAL_MS,
} from '@/constants/lessonAi'

const ACTIVE_DOCUMENT_STATUSES = [LESSON_AI_DOCUMENT_STATUS.pending, LESSON_AI_DOCUMENT_STATUS.extracting]
const ACTIVE_JOB_STATUSES = [LESSON_AI_JOB_STATUS.pending, LESSON_AI_JOB_STATUS.running]

export const isDocumentSelectable = (document) => document.status === LESSON_AI_DOCUMENT_STATUS.extracted
export const isJobActive = (job) => ACTIVE_JOB_STATUSES.includes(job.status)

/**
 * Trạng thái AI của một lesson: tài liệu đã trích xuất, tài liệu được chọn, các job gần đây.
 * Đồng bộ tài liệu khi mở hoặc khi danh sách tệp đính kèm đổi (`attachmentKey`), và poll im lặng
 * chừng nào còn tài liệu hoặc job đang chạy. `onContentChanged` chỉ gọi khi một job tổng hợp
 * do phiên này theo dõi chuyển sang COMPLETED.
 */
export function useLessonAi({ courseId, lessonId, attachmentKey, onContentChanged }) {
  const [documents, setDocuments] = useState([])
  const [jobs, setJobs] = useState([])
  const [selectedIds, setSelectedIds] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)
  const watchedJobs = useRef(new Set())
  const onContentChangedRef = useRef(onContentChanged)
  const polling = useRef(false)

  useEffect(() => {
    onContentChangedRef.current = onContentChanged
  }, [onContentChanged])

  const applyDocuments = useCallback((items) => {
    setDocuments(items)
    // Chỉ giữ lựa chọn còn hợp lệ: tài liệu bị gỡ hoặc chưa còn ở trạng thái đã trích xuất sẽ tự bỏ chọn.
    setSelectedIds((current) => current.filter((id) => items.some((item) => item.mediaId === id && isDocumentSelectable(item))))
  }, [])

  const applyJobs = useCallback((items) => {
    setJobs(items)
    for (const job of items) {
      if (isJobActive(job)) watchedJobs.current.add(job.id)
      else if (watchedJobs.current.delete(job.id) && job.status === LESSON_AI_JOB_STATUS.completed && job.jobType === LESSON_AI_JOB_TYPE.synthesize) {
        onContentChangedRef.current?.(job)
      }
    }
  }, [])

  useEffect(() => {
    setSelectedIds([])
    setJobs([])
    watchedJobs.current = new Set()
  }, [lessonId])

  useEffect(() => {
    if (!courseId || !lessonId) return undefined
    const controller = new AbortController()
    setLoading(true)
    setError(null)
    Promise.all([
      syncLessonAiDocumentsRequest(courseId, lessonId, { silent: true, signal: controller.signal }),
      fetchLessonAiJobsRequest(courseId, lessonId, { silent: true, signal: controller.signal }),
    ])
      .then(([docs, jobsResult]) => {
        applyDocuments(docs.data.items)
        // Job đang chạy từ trước khi mở panel vẫn được theo dõi để báo khi xong.
        applyJobs(jobsResult.data.items)
        setLoading(false)
      })
      .catch((requestError) => {
        if (controller.signal.aborted) return
        setError(toApiError(requestError))
        setLoading(false)
      })
    return () => controller.abort()
  }, [courseId, lessonId, attachmentKey, applyDocuments, applyJobs])

  const needsPolling = documents.some((item) => ACTIVE_DOCUMENT_STATUSES.includes(item.status)) || jobs.some(isJobActive)

  useEffect(() => {
    if (!needsPolling || !courseId || !lessonId) return undefined
    const controller = new AbortController()
    const timer = setInterval(async () => {
      if (polling.current) return
      polling.current = true
      try {
        const [docs, jobsResult] = await Promise.all([
          fetchLessonAiDocumentsRequest(courseId, lessonId, { silent: true, signal: controller.signal }),
          fetchLessonAiJobsRequest(courseId, lessonId, { silent: true, signal: controller.signal }),
        ])
        if (controller.signal.aborted) return
        applyDocuments(docs.data.items)
        applyJobs(jobsResult.data.items)
      } catch {
        // Lỗi poll tạm thời bỏ qua; lần poll sau hoặc thao tác của người dùng sẽ hiện lỗi.
      } finally {
        polling.current = false
      }
    }, LESSON_AI_POLL_INTERVAL_MS)
    return () => {
      clearInterval(timer)
      controller.abort()
    }
  }, [needsPolling, courseId, lessonId, applyDocuments, applyJobs])

  const toggleDocument = useCallback((mediaId) => {
    setSelectedIds((current) => (current.includes(mediaId) ? current.filter((id) => id !== mediaId) : [...current, mediaId]))
  }, [])

  const selectAllExtracted = useCallback(() => {
    setSelectedIds(documents.filter(isDocumentSelectable).map((item) => item.mediaId))
  }, [documents])

  const clearSelection = useCallback(() => setSelectedIds([]), [])

  const retryFailed = useCallback(async () => {
    try {
      const { data } = await syncLessonAiDocumentsRequest(courseId, lessonId, { retryFailed: true })
      applyDocuments(data.items)
    } catch (requestError) {
      setError(toApiError(requestError))
    }
  }, [courseId, lessonId, applyDocuments])

  const startJob = useCallback(async (jobType, { questionCount, overwrite = false } = {}) => {
    setSubmitting(true)
    try {
      const { data } = await createLessonAiJobRequest(courseId, lessonId, {
        jobType,
        mediaIds: selectedIds,
        ...(jobType === LESSON_AI_JOB_TYPE.quiz ? { questionCount } : {}),
        ...(overwrite ? { overwriteContent: true } : {}),
      })
      watchedJobs.current.add(data.id)
      setJobs((current) => [data, ...current.filter((job) => job.id !== data.id)])
      return { ok: true }
    } catch (requestError) {
      const apiError = toApiError(requestError)
      return { ok: false, code: apiError.code, message: apiError.message }
    } finally {
      setSubmitting(false)
    }
  }, [courseId, lessonId, selectedIds])

  return {
    documents,
    jobs,
    selectedIds,
    loading,
    error,
    submitting,
    toggleDocument,
    selectAllExtracted,
    clearSelection,
    retryFailed,
    startJob,
  }
}
