export const MEDIA_JOB_TYPE = {
  thumbnailDerivation: 'THUMBNAIL_DERIVATION',
  markdownUsageSync: 'MARKDOWN_USAGE_SYNC',
  mediaUsageDelete: 'MEDIA_USAGE_DELETE',
  notificationUsage: 'NOTIFICATION_USAGE',
}

export const MEDIA_JOB_STATUS = {
  queued: 'QUEUED',
  processing: 'PROCESSING',
  completed: 'COMPLETED',
  partialFailed: 'PARTIAL_FAILED',
  failed: 'FAILED',
}

export const MEDIA_JOB_COPY = {
  item: 'job',
  title: 'Quản lý job Media',
  description: 'Theo dõi tiến độ và lỗi an toàn của các công việc nền do Media Worker quản lý.',
  jobType: 'Loại job',
  status: 'Trạng thái',
  correlationId: 'Correlation ID',
  empty: 'Chưa có job phù hợp',
  emptyHint: 'Thay đổi bộ lọc hoặc đợi worker tạo công việc mới.',
}
