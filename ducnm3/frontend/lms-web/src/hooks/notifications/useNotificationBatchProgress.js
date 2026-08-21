import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { NOTIFICATION_BATCH_POLL_INTERVAL_MS } from '@/constants/notification'
import {
  fetchNotificationBatchDeliveryStatus,
  fetchNotificationBatchFailures,
  fetchNotificationBatchSnapshotStatus,
  fetchNotificationMediaUsageJobStatus,
  resetNotificationBatchProgress,
  retryNotificationBatchFailures,
  setNotificationBatchPollingPaused,
  setNotificationBatchProgressQuery,
  startNotificationBatchPolling,
} from '@/features/notifications/notificationBatchesSlice'
import {
  isProgressStepTerminal,
  nextNotificationBatchProgressStep,
} from '@/features/notifications/notificationBatchProgressFlow'

const stepActions = {
  snapshot: fetchNotificationBatchSnapshotStatus,
  delivery: fetchNotificationBatchDeliveryStatus,
  mediaUsage: fetchNotificationMediaUsageJobStatus,
}

export function useNotificationBatchProgress() {
  const dispatch = useDispatch()
  const progress = useSelector((state) => state.notificationBatches.progress)
  const timer = useRef(null)
  const currentStep = nextNotificationBatchProgressStep(progress)
  const isTerminal = Boolean(progress.activeBatchId && !currentStep)
  const watch = useCallback(
    (batchId) => dispatch(startNotificationBatchPolling(batchId)),
    [dispatch],
  )

  useEffect(() => {
    const batchId = progress.activeBatchId
    if (!batchId || !currentStep || progress.paused || progress.error) return undefined
    let cancelled = false
    const poll = async () => {
      const result = await dispatch(stepActions[currentStep](batchId))
      const status = result.payload?.data?.status
      if (
        !cancelled &&
        result.meta.requestStatus === 'fulfilled' &&
        !isProgressStepTerminal(status)
      ) {
        timer.current = window.setTimeout(poll, NOTIFICATION_BATCH_POLL_INTERVAL_MS)
      }
    }
    poll()
    return () => {
      cancelled = true
      if (timer.current) window.clearTimeout(timer.current)
    }
  }, [currentStep, dispatch, progress.activeBatchId, progress.error, progress.paused])

  useEffect(() => {
    if (
      progress.delivery?.failedCount > 0 &&
      isProgressStepTerminal(progress.delivery.status)
    ) {
      dispatch(fetchNotificationBatchFailures(progress.activeBatchId))
    }
  }, [dispatch, progress.activeBatchId, progress.delivery?.failedCount, progress.delivery?.status])

  return {
    ...progress,
    currentStep,
    isTerminal,
    setQuery: useCallback(
      (batchId) => dispatch(setNotificationBatchProgressQuery(batchId)),
      [dispatch],
    ),
    watch,
    retryFailed: useCallback(
      (batchId) => dispatch(retryNotificationBatchFailures(batchId)),
      [dispatch],
    ),
    pause: useCallback(() => dispatch(setNotificationBatchPollingPaused(true)), [dispatch]),
    resume: useCallback(() => dispatch(setNotificationBatchPollingPaused(false)), [dispatch]),
    reset: useCallback(() => dispatch(resetNotificationBatchProgress()), [dispatch]),
  }
}
