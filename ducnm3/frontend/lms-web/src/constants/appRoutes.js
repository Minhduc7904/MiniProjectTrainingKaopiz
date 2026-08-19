export const APP_ROUTES = {
  home: '/',
  students: '/student/students',
  courses: '/course/courses',
  courseExport: '/course/courses/export',
  mediaUpload: '/media/upload',
  mediaUploadDirect: '/media/upload-direct',
  notificationSend: '/notification/batches/send',
  notificationBatches: '/notification/batches',
  notificationProgress: '/notification/batches/progress',
  notificationProgressDetail: '/notification/batches/:batchId/progress',
  schedulerJobs: '/scheduler/jobs',
}

export const SERVICE_IDS = {
  student: 'student',
  course: 'course',
  media: 'media',
  notification: 'notification',
  scheduler: 'scheduler',
}

export const SERVICES = [
  {
    id: SERVICE_IDS.student,
    label: 'Student',
    icon: 'student',
    menus: [
      {
        to: APP_ROUTES.students,
        label: 'Danh sách học viên',
        icon: 'students',
        ready: true,
        activityId: 'getStudents',
      },
    ],
  },
  {
    id: SERVICE_IDS.course,
    label: 'Course',
    icon: 'course',
    menus: [
      {
        to: APP_ROUTES.courses,
        label: 'Danh sách khóa học',
        icon: 'courses',
        ready: true,
        activityId: 'getCourses',
      },
      {
        to: APP_ROUTES.courseExport,
        label: 'Xuất CSV khóa học',
        icon: 'courseExport',
        ready: true,
        activityId: 'exportCourses',
      },
    ],
  },
  {
    id: SERVICE_IDS.media,
    label: 'Media',
    icon: 'media',
    menus: [
      {
        to: APP_ROUTES.mediaUpload,
        label: 'Upload media',
        icon: 'upload',
        ready: true,
        activityId: 'postMedia',
      },
      {
        to: APP_ROUTES.mediaUploadDirect,
        label: 'Direct upload',
        icon: 'uploadDirect',
        ready: true,
        activityId: 'postMediaDirect',
      },
    ],
  },
  {
    id: SERVICE_IDS.notification,
    label: 'Notification',
    icon: 'notification',
    menus: [
      {
        to: APP_ROUTES.notificationSend,
        label: 'Gửi hàng loạt',
        icon: 'send',
        ready: true,
        activityId: 'postNotificationBatch',
      },
      {
        to: APP_ROUTES.notificationBatches,
        label: 'Quản lý batch',
        icon: 'batches',
        ready: true,
        activityId: 'getNotificationBatches',
      },
      {
        to: APP_ROUTES.notificationProgress,
        label: 'Tiến trình gửi',
        icon: 'progress',
        ready: true,
        activityId: 'getNotificationBatch',
        matchRoutes: [
          APP_ROUTES.notificationProgress,
          APP_ROUTES.notificationProgressDetail,
        ],
      },
    ],
  },
  {
    id: SERVICE_IDS.scheduler,
    label: 'Scheduler',
    icon: 'scheduler',
    menus: [
      {
        to: APP_ROUTES.schedulerJobs,
        label: 'Job',
        icon: 'jobs',
        ready: false,
      },
    ],
  },
]

export function getServiceById(serviceId) {
  return SERVICES.find((service) => service.id === serviceId) ?? SERVICES[0]
}

export function notificationBatchProgressPath(batchId) {
  return APP_ROUTES.notificationProgressDetail.replace(':batchId', batchId)
}

function matchesRoute(pathname, route) {
  const pathSegments = pathname.split('/').filter(Boolean)
  const routeSegments = route.split('/').filter(Boolean)
  return pathSegments.length === routeSegments.length && routeSegments.every(
    (segment, index) => segment.startsWith(':') || segment === pathSegments[index],
  )
}

export function matchesMenuPath(pathname, menu) {
  return (menu.matchRoutes ?? [menu.to]).some((route) =>
    matchesRoute(pathname, route),
  )
}

export function getServiceByPath(pathname) {
  return (
    SERVICES.find((service) =>
      service.menus.some((menu) => matchesMenuPath(pathname, menu)),
    ) ?? SERVICES[0]
  )
}

export function getMenuByPath(pathname) {
  for (const service of SERVICES) {
    const menu = service.menus.find((item) => matchesMenuPath(pathname, item))

    if (menu) {
      return menu
    }
  }

  return SERVICES[0].menus[0]
}
