import { useCallback } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { createNotificationBatch, resetNotificationBatchCreate, setNotificationBatchCreateQuery } from '@/features/notifications/notificationBatchesSlice'

export function useNotificationBatchCreate() {
  const dispatch = useDispatch(); const create = useSelector((state) => state.notificationBatches.create)
  return { ...create, setQuery: useCallback((query) => dispatch(setNotificationBatchCreateQuery(query)), [dispatch]), submit: useCallback((query) => dispatch(createNotificationBatch(query)), [dispatch]), reset: useCallback(() => dispatch(resetNotificationBatchCreate()), [dispatch]) }
}
