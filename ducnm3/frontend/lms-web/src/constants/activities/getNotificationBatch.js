import { API_ERROR_CODES } from '@/constants/apiErrorCodes'

export const GET_NOTIFICATION_BATCH_ACTIVITY = {
  id: 'getNotificationBatch',
  method: 'GET',
  path: '/notification/api/notification-batches/{batchId}',
  title: 'Đọc tiến trình batch để polling',
  lanes: ['Client', 'Gateway', 'Notification API', 'MySQL', 'Fault'],
  nodes: [
    { id: 'start', lane: 0, row: 0, kind: 'start', label: 'Bắt đầu' }, { id: 'request', lane: 0, row: 1, label: 'GET batch' },
    { id: 'gateway', lane: 1, row: 2, label: 'Forward' }, { id: 'read', lane: 2, row: 3, label: 'Đọc counters' },
    { id: 'db', lane: 3, row: 4, label: 'SELECT batch' }, { id: 'response', lane: 2, row: 5, label: '200 no-store' },
    { id: 'endOk', lane: 0, row: 6, kind: 'end', label: 'Cập nhật UI' }, { id: 'missing', lane: 4, row: 4, label: '404 batch missing' },
    { id: 'endFail', lane: 4, row: 6, kind: 'end', label: 'Lỗi' },
  ],
  edges: [{ from: 'start', to: 'request' }, { from: 'request', to: 'gateway' }, { from: 'gateway', to: 'read' }, { from: 'read', to: 'db' }, { from: 'db', to: 'response' }, { from: 'response', to: 'endOk' }, { from: 'read', to: 'missing', outcome: 'fail' }, { from: 'missing', to: 'endFail', outcome: 'fail' }],
  pendingPath: ['start', 'request', 'gateway'], successPath: ['start', 'request', 'gateway', 'read', 'db', 'response', 'endOk'],
  failByCode: { NOTIFICATION_BATCH_NOT_FOUND: { node: 'missing', path: ['start', 'request', 'gateway', 'read', 'missing', 'endFail'] }, [API_ERROR_CODES.validationFailed]: { node: 'missing', path: ['start', 'request', 'gateway', 'read', 'missing', 'endFail'] }, default: { node: 'endFail', path: ['start', 'request', 'gateway', 'endFail'] } },
}
