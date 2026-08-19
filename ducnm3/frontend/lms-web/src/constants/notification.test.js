import { describe, expect, it } from 'vitest'
import { canRetryNotificationBatch } from './notification'

describe('notification batch retry action', () => {
  it.each([
    ['PARTIAL_FAILED', 2, true],
    ['FAILED', 1, true],
    ['COMPLETED', 1, true],
    ['PROCESSING', 1, false],
    ['COMPLETED', 0, false],
  ])('status %s with %i failures has retry=%s', (status, failedCount, expected) => {
    expect(canRetryNotificationBatch({ status, failedCount })).toBe(expected)
  })
})
