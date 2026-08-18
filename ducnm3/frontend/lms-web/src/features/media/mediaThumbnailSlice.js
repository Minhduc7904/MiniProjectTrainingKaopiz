import { createSlice } from '@reduxjs/toolkit'

const initialState = {
  mediaId: null,
  data: null,
  loading: false,
  success: false,
  error: null,
  retrying: false,
}

const mediaThumbnailSlice = createSlice({
  name: 'mediaThumbnail',
  initialState,
  reducers: {
    initialized(state, action) {
      state.mediaId = action.payload.mediaId ?? null
      state.data = action.payload.thumbnail ?? null
      state.loading = false
      state.success = Boolean(action.payload.thumbnail)
      state.error = null
      state.retrying = false
    },
    pollStarted(state, action) {
      if (state.mediaId && state.mediaId !== action.payload.mediaId) return
      state.mediaId = action.payload.mediaId
      state.loading = true
      state.error = null
    },
    pollSucceeded(state, action) {
      if (state.mediaId !== action.payload.mediaId) return
      state.data = { ...state.data, ...action.payload.thumbnail }
      state.loading = false
      state.success = true
      state.error = null
    },
    pollFailed(state, action) {
      state.loading = false
      state.error = action.payload
    },
    retryStarted(state) {
      state.retrying = true
      state.error = null
    },
    retrySucceeded(state, action) {
      if (state.mediaId !== action.payload.mediaId) return
      state.data = { ...state.data, ...action.payload.thumbnail }
      state.retrying = false
      state.success = true
      state.error = null
    },
    retryFailed(state, action) {
      state.retrying = false
      state.error = action.payload
    },
    reset() {
      return initialState
    },
  },
})

export const mediaThumbnailActions = mediaThumbnailSlice.actions
export const mediaThumbnailReducer = mediaThumbnailSlice.reducer
