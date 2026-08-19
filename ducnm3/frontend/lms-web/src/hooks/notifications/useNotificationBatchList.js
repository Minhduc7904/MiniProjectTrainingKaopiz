import { useCallback, useEffect, useRef, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { NOTIFICATION_BATCH_TERMINAL_STATUSES } from '@/constants/notification'
import { fetchNotificationBatches, retryNotificationBatchFailures } from '@/features/notifications/notificationBatchesSlice'

export function useNotificationBatchList() {
  const dispatch = useDispatch()
  const list = useSelector((state) => state.notificationBatches.list)
  const queryRef = useRef(list.query)
  const [clock, setClock] = useState(Date.now())
  const load = useCallback((query) => {
    queryRef.current = query
    dispatch(fetchNotificationBatches(query))
  }, [dispatch])

  useEffect(() => {
    load(queryRef.current)
  }, [load])

  useEffect(() => {
    const tick = window.setInterval(() => setClock(Date.now()), 1000)
    return () => window.clearInterval(tick)
  }, [])

  useEffect(() => {
    if (!list.data.some((batch) => !NOTIFICATION_BATCH_TERMINAL_STATUSES.has(batch.status))) return undefined
    const timer = window.setTimeout(() => load(queryRef.current), 5000)
    return () => window.clearTimeout(timer)
  }, [list.data, load])

  const retryFailed = useCallback((batchId) => dispatch(
    retryNotificationBatchFailures({ batchId, createdBy: crypto.randomUUID() }),
  ), [dispatch])

  return { ...list, clock, load, retryFailed, reset: () => load({ page: 1, pageSize: 20, status: '' }) }
}
