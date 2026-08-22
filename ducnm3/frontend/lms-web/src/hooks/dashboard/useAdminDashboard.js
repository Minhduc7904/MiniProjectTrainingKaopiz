import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { refreshDashboard, refreshHealth } from '@/features/dashboard/dashboardSlice'

export function useAdminDashboard() {
  const dispatch = useDispatch()
  const dashboard = useSelector((state) => state.dashboard)
  const didLoad = useRef(false)

  useEffect(() => {
    if (!didLoad.current) {
      didLoad.current = true
      dispatch(refreshDashboard())
    }
  }, [dispatch])

  return {
    ...dashboard,
    refresh: useCallback(() => dispatch(refreshDashboard()), [dispatch]),
    refreshHealth: useCallback((serviceId) => dispatch(refreshHealth(serviceId)), [dispatch]),
  }
}
