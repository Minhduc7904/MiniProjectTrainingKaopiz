import { Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from '@/components/layout/AppShell'
import { APP_ROUTES } from '@/constants/appRoutes'
import { MediaUploadPage } from '@/pages/media/MediaUploadPage'
import { MediaDirectUploadPage } from '@/pages/media/MediaDirectUploadPage'
import { MediaJobsPage } from '@/pages/media/MediaJobsPage'
import { PlaceholderPage } from '@/pages/placeholder/PlaceholderPage'
import { StudentsPage } from '@/pages/students/StudentsPage'
import { CourseExportPage } from '@/pages/courses/CourseExportPage'
import { CourseDetailPage, CoursesPage } from '@/pages/courses/CourseDetailPage'
import { NotificationBatchCreatePage } from '@/pages/notifications/NotificationBatchCreatePage'
import { NotificationBatchProgressPage } from '@/pages/notifications/NotificationBatchProgressPage'
import { NotificationBatchListPage } from '@/pages/notifications/NotificationBatchListPage'
import { StudentRouteGuard } from '@/auth/StudentRouteGuard'
import { StudentCourseDetailPage, StudentCoursesPage, StudentHomePage, StudentLoadingPage, StudentLoginPage, StudentLogoutPage, StudentProfilePage, StudentRegisterPage } from '@/pages/student-auth/StudentAuthPages'

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
        <Route path={APP_ROUTES.courseDetails} element={<CourseDetailPage />} />
        <Route path={APP_ROUTES.courseExport} element={<CourseExportPage />} />
        <Route path={APP_ROUTES.mediaUpload} element={<MediaUploadPage />} />
        <Route path={APP_ROUTES.mediaUploadDirect} element={<MediaDirectUploadPage />} />
        <Route path={APP_ROUTES.mediaJobs} element={<MediaJobsPage />} />
        <Route path={APP_ROUTES.notificationSend} element={<NotificationBatchCreatePage />} />
        <Route path={APP_ROUTES.notificationBatches} element={<NotificationBatchListPage />} />
        <Route path={APP_ROUTES.notificationProgress} element={<NotificationBatchProgressPage />} />
        <Route path={APP_ROUTES.notificationProgressDetail} element={<NotificationBatchProgressPage />} />
        <Route path={APP_ROUTES.schedulerJobs} element={<PlaceholderPage />} />
      </Route>
      <Route path={APP_ROUTES.studentRegister} element={<StudentRegisterPage />} />
      <Route path={APP_ROUTES.studentLogin} element={<StudentLoginPage />} />
      <Route path={APP_ROUTES.studentLoading} element={<StudentLoadingPage />} />
      <Route path={APP_ROUTES.studentLogout} element={<StudentLogoutPage />} />
      <Route element={<StudentRouteGuard />}>
        <Route path={APP_ROUTES.studentHome} element={<StudentHomePage />} />
        <Route path={APP_ROUTES.studentCourses} element={<StudentCoursesPage />} />
        <Route path={APP_ROUTES.studentCourseDetail} element={<StudentCourseDetailPage />} />
        <Route path={APP_ROUTES.studentProfile} element={<StudentProfilePage />} />
      </Route>
    </Routes>
  )
}
