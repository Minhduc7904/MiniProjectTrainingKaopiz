import { describe, expect, it, vi } from 'vitest'
import { executeDirectUploadWorkflow } from '@/hooks/media/directUploadWorkflow'

function deferredOperation() {
  let resolve
  let reject
  const promise = new Promise((onResolve, onReject) => {
    resolve = onResolve
    reject = onReject
  })
  promise.abort = vi.fn(() => reject({ code: 'UPLOAD_CANCELED', message: 'Đã hủy.' }))
  return { promise, resolve, reject }
}

async function nextTurn() {
  await Promise.resolve()
  await Promise.resolve()
}

describe('direct upload workflow invalidation', () => {
  for (const stoppedAt of ['checksum', 'intent', 'upload', 'finalize']) {
    it(`suppresses stale results and errors when invalidated during ${stoppedAt}`, async () => {
      let current = true
      const checksum = deferredOperation()
      const intent = deferredOperation()
      const upload = deferredOperation()
      const finalize = deferredOperation()
      const events = {
        checksumStarted: vi.fn(), checksumProgress: vi.fn(), checksumCompleted: vi.fn(),
        preparingStarted: vi.fn(), uploadStarted: vi.fn(), uploadProgress: vi.fn(),
        finalizeStarted: vi.fn(), completed: vi.fn(), failed: vi.fn(),
      }
      const runtime = {
        signal: new AbortController().signal,
        setChecksum: vi.fn(), clearChecksum: vi.fn(),
        setUpload: vi.fn(), clearUpload: vi.fn(),
      }
      const progressCallbacks = []
      const workflow = executeDirectUploadWorkflow({
        file: new File(['x'], 'x.txt', { type: 'text/plain' }),
        mediaType: 'DOCUMENT',
        actor: { uploadedBy: 'actor-1', uploadedByType: 'STUDENT' },
        runtime,
        isCurrent: () => current,
        operations: {
          hashFile: (_file, onProgress) => {
            progressCallbacks.push(() => onProgress(25))
            return checksum.promise
          },
          createIntent: () => intent.promise,
          uploadSigned: ({ onProgress }) => {
            progressCallbacks.push(() => onProgress(50))
            return upload.promise
          },
          complete: () => finalize.promise,
        },
        events,
      })

      if (stoppedAt !== 'checksum') {
        checksum.resolve('a'.repeat(64))
        await nextTurn()
      }
      if (stoppedAt === 'upload' || stoppedAt === 'finalize') {
        intent.resolve({ data: { mediaId: 'media-1', uploadUrl: 'signed', formFields: {} }, meta: { traceId: 'intent-trace' } })
        await nextTurn()
      }
      if (stoppedAt === 'finalize') {
        upload.resolve()
        await nextTurn()
      }

      current = false
      progressCallbacks.forEach((callback) => callback())
      const active = { checksum, intent, upload, finalize }[stoppedAt]
      active.reject({ code: 'STALE_FAILURE', message: 'Không được hiển thị.' })

      await expect(workflow).resolves.toBe(false)
      expect(events.failed).not.toHaveBeenCalled()
      expect(events.completed).not.toHaveBeenCalled()
      expect(events.checksumProgress).not.toHaveBeenCalled()
      expect(events.uploadProgress).not.toHaveBeenCalled()
    })
  }

  it('returns finalized data and trace only for the current run', async () => {
    const events = {
      checksumStarted: vi.fn(), checksumProgress: vi.fn(), checksumCompleted: vi.fn(),
      preparingStarted: vi.fn(), uploadStarted: vi.fn(), uploadProgress: vi.fn(),
      finalizeStarted: vi.fn(), completed: vi.fn(), failed: vi.fn(),
    }
    const runtime = {
      signal: new AbortController().signal,
      setChecksum: vi.fn(), clearChecksum: vi.fn(),
      setUpload: vi.fn(), clearUpload: vi.fn(),
    }
    const operation = Promise.resolve('a'.repeat(64))
    operation.abort = vi.fn()
    const upload = Promise.resolve()
    upload.abort = vi.fn()

    await executeDirectUploadWorkflow({
      file: new File(['x'], 'x.txt', { type: 'text/plain' }),
      mediaType: 'DOCUMENT',
      actor: { uploadedBy: 'actor-1', uploadedByType: 'STUDENT' },
      runtime,
      isCurrent: () => true,
      operations: {
        hashFile: () => operation,
        createIntent: async () => ({ data: { mediaId: 'media-1', uploadUrl: 'signed', formFields: {} }, meta: { traceId: 'intent-trace' } }),
        uploadSigned: () => upload,
        complete: async () => ({ data: { id: 'media-1', status: 'READY' }, meta: { traceId: 'complete-trace' } }),
      },
      events,
    })

    expect(events.preparingStarted).toHaveBeenCalledBefore(events.uploadStarted)
    expect(events.completed).toHaveBeenCalledWith({ id: 'media-1', status: 'READY' }, 'complete-trace')
    expect(events.failed).not.toHaveBeenCalled()
  })
})
