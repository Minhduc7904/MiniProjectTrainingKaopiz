export const NOTIFICATION_PROGRESS_STEPS = {
  snapshot: 'snapshot',
  delivery: 'delivery',
  mediaUsage: 'mediaUsage',
}

const failedStatuses = new Set(['FAILED', 'PARTIAL_FAILED'])
const completedStatuses = new Set(['COMPLETED'])

export function isProgressStepTerminal(status) {
  return completedStatuses.has(status) || failedStatuses.has(status)
}

export function nextNotificationBatchProgressStep({ snapshot, delivery, mediaUsage }) {
  if (!snapshot || !isProgressStepTerminal(snapshot.status)) return NOTIFICATION_PROGRESS_STEPS.snapshot
  if (failedStatuses.has(snapshot.status)) return null
  if (!delivery || !isProgressStepTerminal(delivery.status)) return NOTIFICATION_PROGRESS_STEPS.delivery
  if (!mediaUsage || !isProgressStepTerminal(mediaUsage.status)) return NOTIFICATION_PROGRESS_STEPS.mediaUsage
  return null
}
