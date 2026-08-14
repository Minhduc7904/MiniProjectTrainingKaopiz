import { useCallback } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { toastDismissed } from '@/features/toasts/toastsSlice'

export function useApiToasts() {
  const dispatch = useDispatch()
  const items = useSelector((state) => state.toasts.items)

  const dismiss = useCallback(
    (id) => {
      dispatch(toastDismissed(id))
    },
    [dispatch],
  )

  return { items, dismiss }
}
