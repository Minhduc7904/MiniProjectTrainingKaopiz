import { describe, expect, it } from 'vitest'
import {
  DIRECT_UPLOAD_PHASES,
  directUploadReducer,
  directUploadActions,
  normalizeProgress,
  validateDirectUploadFile,
} from '@/features/media/directUploadSlice'

describe('direct upload file validation', () => {
  it('accepts a valid file and leaves type classification to the backend', () => {
    const file = new File(['image'], 'lesson.png', { type: 'image/png' })

    expect(validateDirectUploadFile(file)).toBeNull()
  })
})

describe('direct upload progress state', () => {
  it('normalizes progress into an integer between zero and one hundred', () => {
    expect(normalizeProgress(2, 3)).toBe(67)
    expect(normalizeProgress(5, 0)).toBe(0)
    expect(normalizeProgress(12, 10)).toBe(100)
  })

  it('moves through checksum, preparing, upload, finalize and complete with final trace', () => {
    let state = directUploadReducer(undefined, { type: 'init' })
    state = directUploadReducer(state, directUploadActions.checksumStarted())
    expect(state.phase).toBe(DIRECT_UPLOAD_PHASES.checksum)

    state = directUploadReducer(state, directUploadActions.preparingStarted())
    expect(state.phase).toBe(DIRECT_UPLOAD_PHASES.preparing)

    state = directUploadReducer(state, directUploadActions.uploadStarted({ mediaId: 'media-1', traceId: 'intent-trace' }))
    expect(state).toMatchObject({ phase: DIRECT_UPLOAD_PHASES.upload, mediaId: 'media-1' })

    state = directUploadReducer(state, directUploadActions.finalizeStarted())
    expect(state.phase).toBe(DIRECT_UPLOAD_PHASES.finalize)

    state = directUploadReducer(state, directUploadActions.completed({
      data: { id: 'media-1', status: 'READY' },
      traceId: 'complete-trace',
    }))
    expect(state).toMatchObject({
      phase: DIRECT_UPLOAD_PHASES.complete,
      data: { id: 'media-1', status: 'READY' },
      success: true,
      traceId: 'complete-trace',
    })
  })

  it('keeps only serializable ephemeral metadata and clears it on reset', () => {
    let state = directUploadReducer(undefined, directUploadActions.checksumStarted())
    state = directUploadReducer(state, directUploadActions.failed({ code: 'UPLOAD_FAILED', message: 'Thử lại.' }))
    state = directUploadReducer(state, directUploadActions.reset())

    expect(state.phase).toBe(DIRECT_UPLOAD_PHASES.idle)
    expect(state.error).toBeNull()
    expect(state.data).toBeNull()
    expect(JSON.stringify(state)).not.toContain('File')
  })

  it('fresh and reset state contain no file, intent, or signed transfer material', () => {
    const fresh = directUploadReducer(undefined, { type: 'init' })
    const reset = directUploadReducer(
      directUploadReducer(fresh, directUploadActions.uploadStarted({ mediaId: 'media-1' })),
      directUploadActions.reset(),
    )

    for (const state of [fresh, reset]) {
      expect(state.mediaId).toBeNull()
      expect(state.checksumSha256).toBeNull()
      expect(state.data).toBeNull()
      expect(state).not.toHaveProperty('file')
      expect(state).not.toHaveProperty('uploadUrl')
      expect(state).not.toHaveProperty('formFields')
    }
  })
})
