import { API_ERROR_CODES } from '@/constants/apiErrorCodes'
import { API_ROUTES } from '@/constants/apiRoutes'

export const GET_NOTIFICATION_BATCHES_ACTIVITY = {
  id: 'getNotificationBatches',
  method: 'GET',
  path: API_ROUTES.notifications.batches,
  title: 'Liệt kê Notification Batch',
  lanes: ['Client', 'Gateway', 'Notification API', 'MySQL', 'Fault'],
  nodes: [
    { id: 'start', lane: 0, row: 0, kind: 'start', label: 'Bắt đầu' },
    { id: 'request', lane: 0, row: 1, label: 'GET status + offset' },
    { id: 'gateway', lane: 1, row: 2, label: 'Forward query' },
    { id: 'validate', lane: 2, row: 3, label: 'Validate status/page' },
    { id: 'invalid', lane: 4, row: 4, label: '400 VALIDATION_FAILED' },
    { id: 'db', lane: 3, row: 4, label: 'Count + SELECT page' },
    { id: 'response', lane: 2, row: 5, label: '200 offset + duration' },
    { id: 'endOk', lane: 0, row: 6, kind: 'end', label: 'Hiển thị bảng' },
    { id: 'endFail', lane: 4, row: 6, kind: 'end', label: 'Lỗi' },
  ],
  edges: [
    { from: 'start', to: 'request' },
    { from: 'request', to: 'gateway' },
    { from: 'gateway', to: 'validate' },
    { from: 'validate', to: 'db' },
    { from: 'validate', to: 'invalid', outcome: 'fail' },
    { from: 'invalid', to: 'endFail', outcome: 'fail' },
    { from: 'db', to: 'response' },
    { from: 'response', to: 'endOk' },
  ],
  pendingPath: ['start', 'request', 'gateway'],
  successPath: ['start', 'request', 'gateway', 'validate', 'db', 'response', 'endOk'],
  failByCode: {
    [API_ERROR_CODES.validationFailed]: {
      node: 'invalid',
      path: ['start', 'request', 'gateway', 'validate', 'invalid', 'endFail'],
    },
    default: { node: 'endFail', path: ['start', 'request', 'gateway', 'endFail'] },
  },
}
