import { Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from '@/components/layout/AppShell'
import { APP_ROUTES } from '@/constants/appRoutes'
import { MediaUploadPage } from '@/pages/media/MediaUploadPage'
import { MediaDirectUploadPage } from '@/pages/media/MediaDirectUploadPage'
import { PlaceholderPage } from '@/pages/placeholder/PlaceholderPage'
import { StudentsPage } from '@/pages/students/StudentsPage'
import { CourseExportPage } from '@/pages/courses/CourseExportPage'
import { CoursesPage } from '@/pages/courses/CoursesPage'
import { NotificationBatchCreatePage } from '@/pages/notifications/NotificationBatchCreatePage'
import { NotificationBatchProgressPage } from '@/pages/notifications/NotificationBatchProgressPage'
import { NotificationBatchListPage } from '@/pages/notifications/NotificationBatchListPage'

export function AppRouter() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route
          path={APP_ROUTES.home}
          element={<Navigate to={APP_ROUTES.students} replace />}
        />
        <Route path={APP_ROUTES.students} element={<StudentsPage />} />
        <Route path={APP_ROUTES.courses} element={<CoursesPage />} />
        <Route path={APP_ROUTES.courseExport} element={<CourseExportPage />} />
        <Route path={APP_ROUTES.mediaUpload} element={<MediaUploadPage />} />
        <Route path={APP_ROUTES.mediaUploadDirect} element={<MediaDirectUploadPage />} />
        <Route path={APP_ROUTES.notificationSend} element={<NotificationBatchCreatePage />} />
        <Route path={APP_ROUTES.notificationBatches} element={<NotificationBatchListPage />} />
        <Route path={APP_ROUTES.notificationProgress} element={<NotificationBatchProgressPage />} />
        <Route path={APP_ROUTES.notificationProgressDetail} element={<NotificationBatchProgressPage />} />
        <Route path={APP_ROUTES.schedulerJobs} element={<PlaceholderPage />} />
      </Route>
    </Routes>
  )
}
