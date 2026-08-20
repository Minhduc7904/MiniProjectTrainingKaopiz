import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { fetchMediaLibraryRequest } from '@/api/mediaApi'
import { toApiError } from '@/api/toApiError'

export const MEDIA_LIBRARY_ALL = 'ALL'

function createBucket() {
  return {
    data: [],
    nextCursor: null,
    hasMore: true,
    loaded: false,
    loading: false,
    error: null,
  }
}

const EMPTY_BUCKET = createBucket()

function createInitialState() {
  return { buckets: {} }
}

function getBucket(state, mediaType) {
  const key = mediaType || MEDIA_LIBRARY_ALL
  state.buckets[key] ??= createBucket()
  return state.buckets[key]
}

export const fetchMediaLibrary = createAsyncThunk(
  'mediaLibrary/fetch',
  async ({ mediaType = '', cursor = null, pageSize = 24 }, { rejectWithValue }) => {
    try {
      const response = await fetchMediaLibraryRequest({ mediaType, cursor, pageSize })
      return { ...response, mediaType: mediaType || MEDIA_LIBRARY_ALL, cursor }
    } catch (error) {
      return rejectWithValue({
        error: toApiError(error),
        mediaType: mediaType || MEDIA_LIBRARY_ALL,
      })
    }
  },
)

const mediaLibrarySlice = createSlice({
  name: 'mediaLibrary',
  initialState: createInitialState(),
  reducers: {
    appendMedia(state, action) {
      const media = action.payload
      if (!media?.id) return
      const mediaType = media.mediaType ?? MEDIA_LIBRARY_ALL
      const keys = new Set([MEDIA_LIBRARY_ALL, mediaType, ...Object.keys(state.buckets)])
      keys.forEach((key) => {
        const bucket = state.buckets[key] ?? (state.buckets[key] = createBucket())
        if (!bucket.data.some((item) => item.id === media.id)) bucket.data.unshift(media)
      })
    },
    clearMediaLibrary(state) {
      state.buckets = {}
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchMediaLibrary.pending, (state, action) => {
        const { mediaType = '', cursor = null } = action.meta.arg ?? {}
        const bucket = getBucket(state, mediaType)
        bucket.loading = true
        bucket.error = null
        if (!cursor) {
          bucket.data = []
          bucket.loaded = false
          bucket.nextCursor = null
        }
      })
      .addCase(fetchMediaLibrary.fulfilled, (state, action) => {
        const { mediaType, cursor } = action.payload
        const bucket = getBucket(state, mediaType)
        const incoming = action.payload.data?.items ?? []
        const existing = cursor ? bucket.data : []
        bucket.data = [...existing, ...incoming.filter((item) => !existing.some((known) => known.id === item.id))]
        bucket.nextCursor = action.payload.data?.nextCursor ?? null
        bucket.hasMore = Boolean(action.payload.data?.hasMore)
        bucket.loaded = true
        bucket.loading = false
        bucket.error = null
      })
      .addCase(fetchMediaLibrary.rejected, (state, action) => {
        const mediaType = action.payload?.mediaType ?? MEDIA_LIBRARY_ALL
        const bucket = getBucket(state, mediaType)
        bucket.loading = false
        bucket.error = action.payload?.error ?? { code: 'UNEXPECTED_ERROR', message: 'Không tải được thư viện media.' }
      })
  },
})

export const { appendMedia, clearMediaLibrary } = mediaLibrarySlice.actions
export const mediaLibraryReducer = mediaLibrarySlice.reducer
export const selectMediaLibraryBucket = (state, mediaType = '') =>
  state.mediaLibrary.buckets[mediaType || MEDIA_LIBRARY_ALL] ?? EMPTY_BUCKET
