import { describe, expect, it } from 'vitest'
import {
  fetchNotificationBatches,
  fetchNotificationBatchSnapshotStatus,
  fetchNotificationBatchDeliveryStatus,
  fetchNotificationMediaUsageJobStatus,
  notificationBatchesReducer,
  retryNotificationBatchFailures,
} from './notificationBatchesSlice'

describe('notification batch management state', () => {
  it('keeps requestedCount nullable so blank means all recipients', () => {
    const state = notificationBatchesReducer(undefined, { type: 'init' })
    expect(state.create.query.requestedCount).toBeNull()
    expect(state.list.query).toEqual({ page: 1, pageSize: 20, status: '' })
  })

  it('stores offset pagination returned by list API', () => {
    const payload = {
      data: [{ id: 'batch-1' }],
      pagination: { page: 2, pageSize: 20, totalItems: 21, totalPages: 2 },
      traceId: 'trace-1',
    }
    const state = notificationBatchesReducer(undefined, {
      type: fetchNotificationBatches.fulfilled.type,
      payload,
    })
    expect(state.list.data).toEqual(payload.data)
    expect(state.list.pagination.totalItems).toBe(21)
  })

  it('switches progress to the child batch returned by retry', () => {
    const state = notificationBatchesReducer(undefined, {
      type: retryNotificationBatchFailures.fulfilled.type,
      payload: { data: { id: 'child-batch', sourceBatchId: 'source-batch' }, traceId: 'trace-2' },
    })
    expect(state.progress.activeBatchId).toBe('child-batch')
    expect(state.progress.currentStep).toBe('snapshot')
    expect(state.progress.data).toBeNull()
    expect(state.list.retryingBatchId).toBeNull()
  })

  it('stores each owning-service status independently', () => {
    let state = notificationBatchesReducer(undefined, {
      type: fetchNotificationBatchSnapshotStatus.fulfilled.type,
      payload: { data: { status: 'COMPLETED', progressPercent: 100 }, traceId: 'snapshot-trace' },
    })
    state = notificationBatchesReducer(state, {
      type: fetchNotificationBatchDeliveryStatus.fulfilled.type,
      payload: { data: { status: 'RUNNING', progressPercent: 50 }, traceId: 'delivery-trace' },
    })
    state = notificationBatchesReducer(state, {
      type: fetchNotificationMediaUsageJobStatus.fulfilled.type,
      payload: { data: { status: 'PROCESSING', progressPercent: 25 }, traceId: 'media-trace' },
    })
    expect(state.progress.snapshot.status).toBe('COMPLETED')
    expect(state.progress.delivery.progressPercent).toBe(50)
    expect(state.progress.mediaUsage.progressPercent).toBe(25)
    expect(state.progress.stepTraceIds.mediaUsage).toBe('media-trace')
  })

  it('tracks the source row while retry is pending', () => {
    const state = notificationBatchesReducer(undefined, {
      type: retryNotificationBatchFailures.pending.type,
      meta: { arg: { batchId: 'source-batch' } },
    })
    expect(state.list.retryingBatchId).toBe('source-batch')
  })
})
