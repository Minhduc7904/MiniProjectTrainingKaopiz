import { describe, expect, it } from 'vitest'
import { mediaThumbnailActions, mediaThumbnailReducer } from '@/features/media/mediaThumbnailSlice'

describe('media thumbnail state', () => {
  it('stores a refreshed thumbnail and clears transient errors', () => {
    let state = mediaThumbnailReducer(undefined, { type: 'init' })
    state = mediaThumbnailReducer(state, mediaThumbnailActions.pollStarted({ mediaId: 'media-1' }))
    state = mediaThumbnailReducer(state, mediaThumbnailActions.pollFailed({ message: 'temporary failure' }))
    state = mediaThumbnailReducer(state, mediaThumbnailActions.pollSucceeded({
      mediaId: 'media-1',
      thumbnail: { status: 'READY', thumbnailContentUrl: '/media/api/media/thumb-1/content' },
    }))

    expect(state).toMatchObject({
      mediaId: 'media-1',
      data: { status: 'READY', thumbnailContentUrl: '/media/api/media/thumb-1/content' },
      loading: false,
      success: true,
      error: null,
    })
  })

  it('clears thumbnail data on reset', () => {
    const state = mediaThumbnailReducer(
      undefined,
      mediaThumbnailActions.pollSucceeded({ mediaId: 'media-1', thumbnail: { status: 'READY' } }),
    )

    expect(mediaThumbnailReducer(state, mediaThumbnailActions.reset())).toMatchObject({
      mediaId: null,
      data: null,
      loading: false,
      success: false,
      error: null,
    })
  })
})
