export const APP_ROUTES = {
  home: '/',
  students: '/student/students',
  courses: '/course/courses',
  mediaUpload: '/media/upload',
  notificationSend: '/notification/batches/send',
  notificationProgress: '/notification/batches/progress',
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
        ready: false,
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
        to: APP_ROUTES.notificationProgress,
        label: 'Tiến trình gửi',
        icon: 'progress',
        ready: true,
        activityId: 'getNotificationBatch',
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

export function getServiceByPath(pathname) {
  return (
    SERVICES.find((service) =>
      service.menus.some(
        (menu) =>
          pathname === menu.to || pathname.startsWith(`${menu.to}/`),
      ),
    ) ?? SERVICES[0]
  )
}

export function getMenuByPath(pathname) {
  for (const service of SERVICES) {
    const menu = service.menus.find(
      (item) => pathname === item.to || pathname.startsWith(`${item.to}/`),
    )

    if (menu) {
      return menu
    }
  }

  return SERVICES[0].menus[0]
}
