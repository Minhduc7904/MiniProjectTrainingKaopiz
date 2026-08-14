import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { NOTIFICATION_BATCH_POLL_INTERVAL_MS, NOTIFICATION_BATCH_TERMINAL_STATUSES } from '@/constants/notification'
import { fetchNotificationBatch, fetchNotificationBatchFailures, resetNotificationBatchProgress, setNotificationBatchPollingPaused, setNotificationBatchProgressQuery, startNotificationBatchPolling } from '@/features/notifications/notificationBatchesSlice'

export function useNotificationBatchProgress() {
  const dispatch = useDispatch(); const progress = useSelector((state) => state.notificationBatches.progress); const timer = useRef(null)
  const isTerminal = NOTIFICATION_BATCH_TERMINAL_STATUSES.has(progress.data?.status)
  const watch = useCallback((batchId) => dispatch(startNotificationBatchPolling(batchId)), [dispatch])

  useEffect(() => {
    const batchId = progress.activeBatchId
    if (!batchId || progress.paused || isTerminal) return undefined
    let cancelled = false
    const poll = async () => {
      const result = await dispatch(fetchNotificationBatch(batchId))
      const status = result.payload?.data?.status
      if (!cancelled && result.meta.requestStatus === 'fulfilled' && !NOTIFICATION_BATCH_TERMINAL_STATUSES.has(status)) {
        timer.current = window.setTimeout(poll, NOTIFICATION_BATCH_POLL_INTERVAL_MS)
      }
    }
    poll()
    return () => { cancelled = true; if (timer.current) window.clearTimeout(timer.current) }
  }, [dispatch, isTerminal, progress.activeBatchId, progress.paused])

  useEffect(() => {
    if (progress.data?.failedCount > 0 && isTerminal) dispatch(fetchNotificationBatchFailures(progress.data.id))
  }, [dispatch, isTerminal, progress.data?.failedCount, progress.data?.id])

  return { ...progress, isTerminal, setQuery: useCallback((batchId) => dispatch(setNotificationBatchProgressQuery(batchId)), [dispatch]), watch, pause: useCallback(() => dispatch(setNotificationBatchPollingPaused(true)), [dispatch]), resume: useCallback(() => dispatch(setNotificationBatchPollingPaused(false)), [dispatch]), reset: useCallback(() => dispatch(resetNotificationBatchProgress()), [dispatch]) }
}
