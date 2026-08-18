import { describe, expect, it, vi } from 'vitest'
import { createDirectUploadRuntime } from '@/hooks/media/directUploadRuntime'

describe('direct upload runtime cancellation', () => {
  it('aborts backend requests, checksum worker, and signed XHR together', () => {
    const runtime = createDirectUploadRuntime()
    const checksum = { abort: vi.fn() }
    const upload = { abort: vi.fn() }
    runtime.setChecksum(checksum)
    runtime.setUpload(upload)

    runtime.abort()

    expect(runtime.signal.aborted).toBe(true)
    expect(checksum.abort).toHaveBeenCalledOnce()
    expect(upload.abort).toHaveBeenCalledOnce()
  })

  it('does not abort operations already cleared from the runtime', () => {
    const runtime = createDirectUploadRuntime()
    const checksum = { abort: vi.fn() }
    const upload = { abort: vi.fn() }
    runtime.setChecksum(checksum)
    runtime.clearChecksum(checksum)
    runtime.setUpload(upload)
    runtime.clearUpload(upload)

    runtime.abort()

    expect(checksum.abort).not.toHaveBeenCalled()
    expect(upload.abort).not.toHaveBeenCalled()
  })
})
