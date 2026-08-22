import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { fetchMediaLibraryRequest } from '@/api/mediaApi'
import { toApiError } from '@/api/toApiError'
import { GET_MEDIA_LIBRARY_DEFAULT_QUERY } from '@/constants/inputs/getMediaLibrary'

export const MEDIA_LIBRARY_ALL = 'ALL'

function getBucketKey(mediaType = '', status = '') {
  return `${mediaType || MEDIA_LIBRARY_ALL}:${status || MEDIA_LIBRARY_ALL}`
}

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
  return {
    buckets: {},
    explorer: {
      query: GET_MEDIA_LIBRARY_DEFAULT_QUERY,
      selectedId: null,
    },
  }
}

function getBucket(state, mediaType, status = '') {
  const key = getBucketKey(mediaType, status)
  state.buckets[key] ??= createBucket()
  return state.buckets[key]
}

export const fetchMediaLibrary = createAsyncThunk(
  'mediaLibrary/fetch',
  async ({ mediaType = '', status = '', cursor = null, pageSize = 24 }, { rejectWithValue }) => {
    try {
      const response = await fetchMediaLibraryRequest({ mediaType, status, cursor, pageSize })
      return { ...response, mediaType, status, cursor }
    } catch (error) {
      return rejectWithValue({
        error: toApiError(error),
        mediaType,
        status,
      })
    }
  },
)

const mediaLibrarySlice = createSlice({
  name: 'mediaLibrary',
  initialState: createInitialState(),
  reducers: {
    setMediaLibraryExplorerQuery(state, action) {
      state.explorer.query = {
        ...GET_MEDIA_LIBRARY_DEFAULT_QUERY,
        ...action.payload,
      }
      state.explorer.selectedId = null
    },
    setMediaLibraryExplorerSelectedId(state, action) {
      state.explorer.selectedId = action.payload ?? null
    },
    appendMedia(state, action) {
      const media = action.payload
      if (!media?.id) return
      const mediaType = media.mediaType ?? ''
      const status = media.status ?? ''
      const keys = new Set([
        getBucketKey(),
        getBucketKey(mediaType),
        getBucketKey('', status),
        getBucketKey(mediaType, status),
      ])
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
        const { mediaType = '', status = '', cursor = null } = action.meta.arg ?? {}
        const bucket = getBucket(state, mediaType, status)
        bucket.loading = true
        bucket.error = null
        if (!cursor) {
          bucket.data = []
          bucket.loaded = false
          bucket.nextCursor = null
        }
      })
      .addCase(fetchMediaLibrary.fulfilled, (state, action) => {
        const { mediaType, status, cursor } = action.payload
        const bucket = getBucket(state, mediaType, status)
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
        const mediaType = action.payload?.mediaType ?? ''
        const status = action.payload?.status ?? ''
        const bucket = getBucket(state, mediaType, status)
        bucket.loading = false
        bucket.error = action.payload?.error ?? { code: 'UNEXPECTED_ERROR', message: 'Không tải được thư viện media.' }
      })
  },
})

export const {
  appendMedia,
  clearMediaLibrary,
  setMediaLibraryExplorerQuery,
  setMediaLibraryExplorerSelectedId,
} = mediaLibrarySlice.actions
export const mediaLibraryReducer = mediaLibrarySlice.reducer
export const selectMediaLibraryBucket = (state, mediaType = '', status = '') =>
  state.mediaLibrary.buckets[getBucketKey(mediaType, status)] ?? EMPTY_BUCKET
export const selectMediaLibraryExplorer = (state) => state.mediaLibrary.explorer
