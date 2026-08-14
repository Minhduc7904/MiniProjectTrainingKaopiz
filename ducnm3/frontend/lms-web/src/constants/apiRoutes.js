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
  students: {
    list: joinPath(GATEWAY_PREFIXES.student, '/api/students'),
    detail: (studentId) =>
      joinPath(GATEWAY_PREFIXES.student, '/api/students', studentId),
  },
  media: {
    upload: joinPath(GATEWAY_PREFIXES.media, '/api/media'),
    usages: joinPath(GATEWAY_PREFIXES.media, '/api/media/usages'),
    usageUrl: (usageId) =>
      joinPath(GATEWAY_PREFIXES.media, '/api/media/usages', usageId, 'url'),
    usageUrls: joinPath(GATEWAY_PREFIXES.media, '/api/media/usages/urls'),
    content: (mediaId) =>
      joinPath(GATEWAY_PREFIXES.media, '/api/media', mediaId, 'content'),
  },
}
