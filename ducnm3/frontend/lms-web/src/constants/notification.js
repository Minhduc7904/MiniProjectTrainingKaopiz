export const NOTIFICATION_BATCH_FIELDS = {
  title: 'title',
  bodyMarkdown: 'bodyMarkdown',
  targetScope: 'targetScope',
  createdBy: 'createdBy',
  batchSize: 'batchSize',
  requestedCount: 'requestedCount',
}

export const NOTIFICATION_BATCH_STATUS = {
  pending: 'PENDING',
  snapshotting: 'SNAPSHOTTING',
  snapshotReady: 'SNAPSHOT_READY',
  processing: 'PROCESSING',
  completed: 'COMPLETED',
  partialFailed: 'PARTIAL_FAILED',
  failed: 'FAILED',
}

export const NOTIFICATION_BATCH_TERMINAL_STATUSES = new Set([
  NOTIFICATION_BATCH_STATUS.completed,
  NOTIFICATION_BATCH_STATUS.partialFailed,
  NOTIFICATION_BATCH_STATUS.failed,
])

export function canRetryNotificationBatch(batch) {
  return Boolean(
    batch &&
    NOTIFICATION_BATCH_TERMINAL_STATUSES.has(batch.status) &&
    batch.failedCount > 0,
  )
}

export const NOTIFICATION_BATCH_POLL_INTERVAL_MS = 3000

export const NOTIFICATION_PROGRESS_STEP_STATUS = {
  pending: 'PENDING',
  running: 'RUNNING',
  completed: 'COMPLETED',
  partialFailed: 'PARTIAL_FAILED',
  failed: 'FAILED',
}
export const NOTIFICATION_BATCH_DEFAULT_SIZE = 500
export const NOTIFICATION_BATCH_MAX_FAILURE_ITEMS = 100

export function createNotificationBatchDefaultQuery() {
  return {
    [NOTIFICATION_BATCH_FIELDS.title]: '',
    [NOTIFICATION_BATCH_FIELDS.bodyMarkdown]: '',
    [NOTIFICATION_BATCH_FIELDS.targetScope]: 'ALL_STUDENTS',
    [NOTIFICATION_BATCH_FIELDS.createdBy]: crypto.randomUUID(),
    [NOTIFICATION_BATCH_FIELDS.batchSize]: NOTIFICATION_BATCH_DEFAULT_SIZE,
    [NOTIFICATION_BATCH_FIELDS.requestedCount]: null,
  }
}
