import { describe, expect, it, vi } from 'vitest'
import { preventParentBatchSubmit } from './notificationMarkdownSubmission'

describe('preventParentBatchSubmit', () => {
  it('prevents the media dialog submit from reaching the batch form', () => {
    const event = {
      preventDefault: vi.fn(),
      stopPropagation: vi.fn(),
    }

    preventParentBatchSubmit(event)

    expect(event.preventDefault).toHaveBeenCalledOnce()
    expect(event.stopPropagation).toHaveBeenCalledOnce()
  })
})
