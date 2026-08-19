import { API_ERROR_CODES } from '@/constants/apiErrorCodes'

export const GET_NOTIFICATION_BATCH_ACTIVITY = {
  id: 'getNotificationBatchProgress',
  method: 'GET × 3',
  path: 'snapshot-status → delivery-status → media usage job status',
  title: 'Polling tuần tự tiến trình Notification Batch',
  lanes: ['Client', 'Notification API', 'Notification DB', 'Media API', 'Media DB', 'Fault'],
  nodes: [
    { id: 'start', lane: 0, row: 0, kind: 'start', label: 'Batch ID' },
    { id: 'snapshot', lane: 1, row: 1, label: 'GET snapshot status' },
    { id: 'snapshotDb', lane: 2, row: 2, label: 'Đọc snapshot count' },
    { id: 'snapshotDone', lane: 0, row: 3, label: 'Snapshot completed?' },
    { id: 'delivery', lane: 1, row: 4, label: 'GET delivery status' },
    { id: 'deliveryDb', lane: 2, row: 5, label: 'Đọc counters' },
    { id: 'deliveryDone', lane: 0, row: 6, label: 'Delivery terminal?' },
    { id: 'media', lane: 3, row: 7, label: 'GET Media Usage job' },
    { id: 'mediaDb', lane: 4, row: 8, label: 'Đọc job counters' },
    { id: 'endOk', lane: 0, row: 9, kind: 'end', label: 'Hoàn tất pipeline' },
    { id: 'endFail', lane: 5, row: 9, kind: 'end', label: 'Lỗi an toàn' },
  ],
  edges: [
    { from: 'start', to: 'snapshot' }, { from: 'snapshot', to: 'snapshotDb' },
    { from: 'snapshotDb', to: 'snapshotDone' }, { from: 'snapshotDone', to: 'delivery' },
    { from: 'delivery', to: 'deliveryDb' }, { from: 'deliveryDb', to: 'deliveryDone' },
    { from: 'deliveryDone', to: 'media' }, { from: 'media', to: 'mediaDb' },
    { from: 'mediaDb', to: 'endOk' }, { from: 'snapshot', to: 'endFail', outcome: 'fail' },
    { from: 'delivery', to: 'endFail', outcome: 'fail' }, { from: 'media', to: 'endFail', outcome: 'fail' },
  ],
  pendingPath: ['start', 'snapshot'],
  successPath: ['start', 'snapshot', 'snapshotDb', 'snapshotDone', 'delivery', 'deliveryDb', 'deliveryDone', 'media', 'mediaDb', 'endOk'],
  failByCode: {
    NOTIFICATION_BATCH_NOT_FOUND: { node: 'endFail', path: ['start', 'snapshot', 'endFail'] },
    NOTIFICATION_MEDIA_USAGE_JOB_NOT_FOUND: { node: 'endFail', path: ['start', 'snapshot', 'snapshotDb', 'snapshotDone', 'delivery', 'deliveryDb', 'deliveryDone', 'media', 'endFail'] },
    [API_ERROR_CODES.validationFailed]: { node: 'endFail', path: ['start', 'snapshot', 'endFail'] },
    default: { node: 'endFail', path: ['start', 'endFail'] },
  },
}
