import {
  createNotificationBatchDefaultQuery,
  NOTIFICATION_BATCH_FIELDS,
  NOTIFICATION_BATCH_DEFAULT_SIZE,
} from '@/constants/notification'

export const POST_NOTIFICATION_BATCH_DEFAULT_QUERY = createNotificationBatchDefaultQuery()

export const POST_NOTIFICATION_BATCH_INPUT_FIELDS = [
  { key: NOTIFICATION_BATCH_FIELDS.title, label: 'Tiêu đề', type: 'string', required: true, nullable: false, defaultValue: null, allowlist: null, hint: 'Tối đa 200 ký tự.' },
  { key: NOTIFICATION_BATCH_FIELDS.bodyMarkdown, label: 'Nội dung Markdown', type: 'string', required: true, nullable: false, defaultValue: null, allowlist: null, hint: 'Nội dung gửi đến toàn bộ học viên đang hoạt động.' },
  { key: NOTIFICATION_BATCH_FIELDS.targetScope, label: 'Phạm vi gửi', type: 'string', required: true, nullable: false, defaultValue: 'ALL_STUDENTS', allowlist: ['ALL_STUDENTS'], hint: 'MVP chỉ hỗ trợ ALL_STUDENTS.' },
  { key: NOTIFICATION_BATCH_FIELDS.createdBy, label: 'Người tạo', type: 'uuid', required: true, nullable: false, defaultValue: 'UUID mới', allowlist: null, hint: 'UUID quản trị viên tạo lô.' },
  { key: NOTIFICATION_BATCH_FIELDS.batchSize, label: 'Kích thước chunk', type: 'integer', required: false, nullable: false, defaultValue: NOTIFICATION_BATCH_DEFAULT_SIZE, allowlist: null, hint: 'Từ 1 đến 1000; mặc định 500.' },
]
