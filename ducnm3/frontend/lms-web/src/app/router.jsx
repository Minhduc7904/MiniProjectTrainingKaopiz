import { Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from '@/components/layout/AppShell'
import { APP_ROUTES } from '@/constants/appRoutes'
import { MediaUploadPage } from '@/pages/media/MediaUploadPage'
import { PlaceholderPage } from '@/pages/placeholder/PlaceholderPage'
import { StudentsPage } from '@/pages/students/StudentsPage'
import { NotificationBatchCreatePage } from '@/pages/notifications/NotificationBatchCreatePage'
import { NotificationBatchProgressPage } from '@/pages/notifications/NotificationBatchProgressPage'

export function AppRouter() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route
          path={APP_ROUTES.home}
          element={<Navigate to={APP_ROUTES.students} replace />}
        />
        <Route path={APP_ROUTES.students} element={<StudentsPage />} />
        <Route path={APP_ROUTES.courses} element={<PlaceholderPage />} />
        <Route path={APP_ROUTES.mediaUpload} element={<MediaUploadPage />} />
        <Route path={APP_ROUTES.notificationSend} element={<NotificationBatchCreatePage />} />
        <Route path={APP_ROUTES.notificationProgress} element={<NotificationBatchProgressPage />} />
        <Route path={APP_ROUTES.schedulerJobs} element={<PlaceholderPage />} />
      </Route>
    </Routes>
  )
}
