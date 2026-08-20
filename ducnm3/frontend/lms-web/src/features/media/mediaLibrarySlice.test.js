import { describe, expect, it } from 'vitest'
import {
  appendMedia,
  fetchMediaLibrary,
  mediaLibraryReducer,
  selectMediaLibraryBucket,
} from '@/features/media/mediaLibrarySlice'

const image = { id: 'image-1', mediaType: 'IMAGE', originalFileName: 'one.png' }

describe('media library cache', () => {
  it('adds a direct upload to all and matching type buckets without refetching', () => {
    const state = mediaLibraryReducer(undefined, appendMedia(image))

    expect(selectMediaLibraryBucket({ mediaLibrary: state }, '').data).toEqual([image])
    expect(selectMediaLibraryBucket({ mediaLibrary: state }, 'IMAGE').data).toEqual([image])
  })

  it('deduplicates appended media', () => {
    let state = mediaLibraryReducer(undefined, appendMedia(image))
    state = mediaLibraryReducer(state, appendMedia(image))

    expect(state.buckets.ALL.data).toHaveLength(1)
  })

  it('appends a fetched cursor page to the cached filter', () => {
    let state = mediaLibraryReducer(undefined, fetchMediaLibrary.pending('request-1', { mediaType: 'IMAGE' }))
    state = mediaLibraryReducer(state, fetchMediaLibrary.fulfilled(
      { data: { items: [image], nextCursor: 'cursor-1', hasMore: true }, mediaType: 'IMAGE', cursor: null },
      'request-1',
      { mediaType: 'IMAGE' },
    ))
    state = mediaLibraryReducer(state, fetchMediaLibrary.fulfilled(
      { data: { items: [{ id: 'image-2', mediaType: 'IMAGE' }], nextCursor: null, hasMore: false }, mediaType: 'IMAGE', cursor: 'cursor-1' },
      'request-2',
      { mediaType: 'IMAGE', cursor: 'cursor-1' },
    ))

    expect(state.buckets.IMAGE.data.map((item) => item.id)).toEqual(['image-1', 'image-2'])
    expect(state.buckets.IMAGE.hasMore).toBe(false)
  })
})
