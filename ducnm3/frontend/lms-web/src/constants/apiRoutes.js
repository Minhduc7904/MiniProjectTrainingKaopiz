const GATEWAY_PREFIXES = {
  course: '/course',
  student: '/student',
  media: '/media',
  notification: '/notification',
  scheduler: '/scheduler',
}

function joinPath(...parts) {
  return parts
    .map((part, index) => {
      if (index === 0) {
        return part.replace(/\/$/, '')
      }

      return part.replace(/^\/+|\/+$/g, '')
    })
    .filter(Boolean)
    .join('/')
    .replace(/^(?!\/)/, '/')
}

export const API_ROUTES = {
  health: {
    gateway: '/health',
    course: joinPath(GATEWAY_PREFIXES.course, '/health'),
    student: joinPath(GATEWAY_PREFIXES.student, '/health'),
    media: joinPath(GATEWAY_PREFIXES.media, '/health'),
    notification: joinPath(GATEWAY_PREFIXES.notification, '/health'),
    scheduler: joinPath(GATEWAY_PREFIXES.scheduler, '/health'),
  },
  courses: {
    list: joinPath(GATEWAY_PREFIXES.course, '/api/courses'),
    summary: joinPath(GATEWAY_PREFIXES.course, '/api/courses/summary'),
    byId: (courseId) => joinPath(GATEWAY_PREFIXES.course, '/api/courses', courseId),
    detail: (courseId) => joinPath(GATEWAY_PREFIXES.course, '/api/courses', courseId, 'details'),
    lessons: (courseId) => joinPath(GATEWAY_PREFIXES.course, '/api/courses', courseId, 'lessons'),
    lessonById: (courseId, lessonId) => joinPath(GATEWAY_PREFIXES.course, '/api/courses', courseId, 'lessons', lessonId),
    lessonReorder: (courseId) => joinPath(GATEWAY_PREFIXES.course, '/api/courses', courseId, 'lessons', 'reorder'),
    export: joinPath(GATEWAY_PREFIXES.course, '/api/courses/export'),
  },
  students: {
    list: joinPath(GATEWAY_PREFIXES.student, '/api/students'),
    summary: joinPath(GATEWAY_PREFIXES.student, '/api/students/summary'),
    detail: (studentId) =>
      joinPath(GATEWAY_PREFIXES.student, '/api/students', studentId),
  },
  studentLearning: {
    enrollments: joinPath(GATEWAY_PREFIXES.course, '/api/student/enrollments'),
    catalog: joinPath(GATEWAY_PREFIXES.course, '/api/student/courses'),
    enrollmentDetail: (courseId) => joinPath(GATEWAY_PREFIXES.course, '/api/student/enrollments', courseId),
    lessonDetail: (courseId, lessonId) => joinPath(GATEWAY_PREFIXES.course, '/api/student/enrollments', courseId, 'lessons', lessonId),
    enroll: (courseId) => joinPath(GATEWAY_PREFIXES.course, '/api/courses', courseId, 'enrollments'),
    progress: (courseId) => joinPath(GATEWAY_PREFIXES.course, '/api/courses', courseId, 'my-progress'),
    completeLesson: (courseId, lessonId) => joinPath(GATEWAY_PREFIXES.course, '/api/courses', courseId, 'lessons', lessonId, 'progress', 'complete'),
  },
  media: {
    upload: joinPath(GATEWAY_PREFIXES.media, '/api/media'),
    summary: joinPath(GATEWAY_PREFIXES.media, '/api/media/summary'),
    library: joinPath(GATEWAY_PREFIXES.media, '/api/media/library'),
    uploadIntents: joinPath(GATEWAY_PREFIXES.media, '/api/media/upload-intents'),
    uploadComplete: (mediaId) =>
      joinPath(GATEWAY_PREFIXES.media, '/api/media', mediaId, 'upload-complete'),
    usages: joinPath(GATEWAY_PREFIXES.media, '/api/media/usages'),
    usageBatch: joinPath(GATEWAY_PREFIXES.media, '/api/media/usages/batch'),
    usageReorder: joinPath(GATEWAY_PREFIXES.media, '/api/media/usages/reorder'),
    usageById: (usageId) => joinPath(GATEWAY_PREFIXES.media, '/api/media/usages', usageId),
    usageUrl: (usageId) =>
      joinPath(GATEWAY_PREFIXES.media, '/api/media/usages', usageId, 'url'),
    usageUrls: joinPath(GATEWAY_PREFIXES.media, '/api/media/usages/urls'),
    content: (mediaId) =>
      joinPath(GATEWAY_PREFIXES.media, '/api/media', mediaId, 'content'),
    thumbnail: (mediaId) =>
      joinPath(GATEWAY_PREFIXES.media, '/api/media', mediaId, 'thumbnail'),
    retryThumbnail: (mediaId) =>
      joinPath(GATEWAY_PREFIXES.media, '/api/media', mediaId, 'thumbnail', 'retry'),
    jobs: joinPath(GATEWAY_PREFIXES.media, '/api/media/jobs'),
    notificationMediaUsageJobStatus: (jobId) =>
      joinPath(GATEWAY_PREFIXES.media, '/api/media/usage-jobs', jobId, 'status'),
  },
  notifications: {
    batches: joinPath(GATEWAY_PREFIXES.notification, '/api/notification-batches'),
    batchById: (batchId) =>
      joinPath(GATEWAY_PREFIXES.notification, '/api/notification-batches', batchId),
    batchFailedItems: (batchId) =>
      joinPath(
        GATEWAY_PREFIXES.notification,
        '/api/notification-batches',
        batchId,
        'failed-items',
      ),
    retryBatchFailures: (batchId) =>
      joinPath(
        GATEWAY_PREFIXES.notification,
        '/api/notification-batches',
        batchId,
        'retry-failed',
      ),
    batchSnapshotStatus: (batchId) =>
      joinPath(
        GATEWAY_PREFIXES.notification,
        '/api/notification-batches',
        batchId,
        'snapshot-status',
      ),
    batchDeliveryStatus: (batchId) =>
      joinPath(
        GATEWAY_PREFIXES.notification,
        '/api/notification-batches',
        batchId,
        'delivery-status',
      ),
  },
}
