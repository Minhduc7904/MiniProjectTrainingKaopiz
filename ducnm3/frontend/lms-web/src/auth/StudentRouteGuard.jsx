import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { readStudentActor } from '@/auth/studentAuthStorage'
import { APP_ROUTES } from '@/constants/appRoutes'
import { isStudentSessionVerified } from '@/auth/studentSession'

export function StudentRouteGuard() {
  const location = useLocation()
  const actor = readStudentActor()
  if (!actor) return <Navigate to={APP_ROUTES.studentLogin} replace />
  if (isStudentSessionVerified(actor)) return <Outlet />
  const returnTo = `${location.pathname}${location.search}`
  return <Navigate to={`${APP_ROUTES.studentLoading}?returnTo=${encodeURIComponent(returnTo)}`} replace />
}

export function VerifiedStudentRoute() {
  return <Outlet />
}
