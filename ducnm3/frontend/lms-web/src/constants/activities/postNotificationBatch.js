import { API_ERROR_CODES } from '@/constants/apiErrorCodes'

export const POST_NOTIFICATION_BATCH_ACTIVITY = {
  id: 'postNotificationBatch',
  method: 'POST',
  path: '/notification/api/notification-batches',
  title: 'Tạo batch và durable handoff cho Worker',
  lanes: ['Client', 'Gateway', 'Notification API', 'MySQL / Outbox', 'Fault'],
  nodes: [
    { id: 'start', lane: 0, row: 0, kind: 'start', label: 'Bắt đầu' },
    { id: 'request', lane: 0, row: 1, label: 'POST batch' },
    { id: 'gateway', lane: 1, row: 2, label: 'Forward' },
    { id: 'validate', lane: 2, row: 3, label: 'Validate payload' },
    { id: 'persist', lane: 3, row: 4, label: 'PENDING + outbox' },
    { id: 'accepted', lane: 2, row: 5, label: '202 + Location' },
    { id: 'endOk', lane: 0, row: 6, kind: 'end', label: 'Đã tạo' },
    { id: 'invalid', lane: 4, row: 4, label: '400 validation' },
    { id: 'endFail', lane: 4, row: 6, kind: 'end', label: 'Lỗi' },
  ],
  edges: [
    { from: 'start', to: 'request' }, { from: 'request', to: 'gateway' }, { from: 'gateway', to: 'validate' },
    { from: 'validate', to: 'persist' }, { from: 'persist', to: 'accepted' }, { from: 'accepted', to: 'endOk' },
    { from: 'validate', to: 'invalid', outcome: 'fail' }, { from: 'invalid', to: 'endFail', outcome: 'fail' },
  ],
  pendingPath: ['start', 'request', 'gateway'],
  successPath: ['start', 'request', 'gateway', 'validate', 'persist', 'accepted', 'endOk'],
  failByCode: {
    [API_ERROR_CODES.validationFailed]: { node: 'invalid', path: ['start', 'request', 'gateway', 'validate', 'invalid', 'endFail'] },
    default: { node: 'endFail', path: ['start', 'request', 'gateway', 'endFail'] },
  },
}
