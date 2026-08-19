import { describe, expect, it } from 'vitest'
import { nextNotificationBatchProgressStep } from './notificationBatchProgressFlow'

describe('notification batch progress flow', () => {
  it('polls snapshot first and waits while snapshot is running', () => {
    expect(nextNotificationBatchProgressStep({ snapshot: null })).toBe('snapshot')
    expect(nextNotificationBatchProgressStep({ snapshot: { status: 'RUNNING' } })).toBe('snapshot')
  })

  it('polls delivery only after snapshot completes', () => {
    expect(nextNotificationBatchProgressStep({
      snapshot: { status: 'COMPLETED' },
      delivery: { status: 'RUNNING' },
    })).toBe('delivery')
  })

  it('polls media usage only after delivery reaches a terminal status', () => {
    expect(nextNotificationBatchProgressStep({
      snapshot: { status: 'COMPLETED' },
      delivery: { status: 'COMPLETED' },
      mediaUsage: { status: 'PROCESSING' },
    })).toBe('mediaUsage')
  })

  it('stops after media usage completes or an earlier step fails', () => {
    expect(nextNotificationBatchProgressStep({
      snapshot: { status: 'COMPLETED' },
      delivery: { status: 'COMPLETED' },
      mediaUsage: { status: 'COMPLETED' },
    })).toBeNull()
    expect(nextNotificationBatchProgressStep({ snapshot: { status: 'FAILED' } })).toBeNull()
  })
})
