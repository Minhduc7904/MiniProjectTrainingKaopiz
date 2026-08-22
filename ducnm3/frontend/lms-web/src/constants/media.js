export const POST_MEDIA_FIELDS = {
  file: 'file',
  mediaType: 'mediaType',
  uploadedByType: 'uploadedByType',
  uploadedBy: 'uploadedBy',
}

export const MEDIA_TYPES = {
  image: 'IMAGE',
  video: 'VIDEO',
  document: 'DOCUMENT',
  audio: 'AUDIO',
  other: 'OTHER',
}

export const MEDIA_STATUSES = {
  pending: 'PENDING',
  ready: 'READY',
  failed: 'FAILED',
}

export const MEDIA_STATUS_LABELS = {
  [MEDIA_STATUSES.pending]: 'Đang chờ hoàn tất',
  [MEDIA_STATUSES.ready]: 'Đã sẵn sàng',
  [MEDIA_STATUSES.failed]: 'Tải lên thất bại',
}

export const MEDIA_TYPE_LABELS = {
  [MEDIA_TYPES.image]: 'Ảnh',
  [MEDIA_TYPES.video]: 'Video',
  [MEDIA_TYPES.document]: 'Tài liệu',
  [MEDIA_TYPES.audio]: 'Audio',
  [MEDIA_TYPES.other]: 'Khác',
}

export const MEDIA_OWNER_SERVICES = {
  course: 'COURSE',
  media: 'MEDIA',
  notification: 'NOTIFICATION',
  student: 'STUDENT',
}

export const COURSE_MEDIA = {
  thumbnail: {
    ownerService: MEDIA_OWNER_SERVICES.course,
    ownerType: 'COURSE_THUMBNAIL',
    usageType: 'THUMBNAIL',
    displayOrder: 0,
  },
  gallery: {
    ownerService: MEDIA_OWNER_SERVICES.course,
    ownerType: 'COURSE_GALLERY',
    usageType: 'ATTACHMENT',
  },
}

export const ACTOR_TYPES = {
  admin: 'ADMIN',
  student: 'STUDENT',
}

export const ACTOR_TYPE_LABELS = {
  [ACTOR_TYPES.admin]: 'Admin',
  [ACTOR_TYPES.student]: 'Student',
}

export const MEDIA_LIMITS_MIB = {
  [MEDIA_TYPES.image]: 10,
  [MEDIA_TYPES.video]: 500,
  [MEDIA_TYPES.document]: 50,
  [MEDIA_TYPES.audio]: 100,
  [MEDIA_TYPES.other]: 25,
  multipart: 525,
}
