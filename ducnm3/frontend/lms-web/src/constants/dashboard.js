import { API_ROUTES } from '@/constants/apiRoutes'

export const DASHBOARD_HEALTH_SERVICES = [
  { id: 'gateway', label: 'API Gateway', route: API_ROUTES.health.gateway },
  { id: 'student', label: 'Student Service', route: API_ROUTES.health.student },
  { id: 'course', label: 'Course Service', route: API_ROUTES.health.course },
  { id: 'media', label: 'Media Service', route: API_ROUTES.health.media },
  { id: 'notification', label: 'Notification Service', route: API_ROUTES.health.notification },
  { id: 'scheduler', label: 'Scheduler Service', route: API_ROUTES.health.scheduler },
]

export const DASHBOARD_METRICS = [
  { id: 'students', label: 'Học viên', description: 'Tổng Student trong database' },
  { id: 'media', label: 'Media', description: 'Tổng media object trong database' },
  { id: 'courses', label: 'Khóa học', description: 'Tổng Course trong database' },
  { id: 'lessons', label: 'Bài học', description: 'Tổng Lesson trong database' },
]
