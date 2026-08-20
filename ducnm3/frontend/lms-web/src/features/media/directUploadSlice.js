import { createSlice } from '@reduxjs/toolkit'
import { API_ERROR_CODES } from '@/constants/apiErrorCodes'
import { POST_MEDIA_DEFAULT_QUERY } from '@/constants/inputs/postMedia'
export const DIRECT_UPLOAD_PHASES = {
  idle: 'idle',
  checksum: 'checksum',
  preparing: 'preparing',
  upload: 'upload',
  finalize: 'finalize',
  complete: 'complete',
  error: 'error',
}

export const DIRECT_UPLOAD_PHASE_LABELS = {
  [DIRECT_UPLOAD_PHASES.idle]: 'Chưa bắt đầu',
  [DIRECT_UPLOAD_PHASES.checksum]: 'Đang tính checksum',
  [DIRECT_UPLOAD_PHASES.preparing]: 'Đang tạo upload intent',
  [DIRECT_UPLOAD_PHASES.upload]: 'Đang upload',
  [DIRECT_UPLOAD_PHASES.finalize]: 'Đang finalize',
  [DIRECT_UPLOAD_PHASES.complete]: 'Hoàn tất',
  [DIRECT_UPLOAD_PHASES.error]: 'Có lỗi',
}

export function validateDirectUploadFile(file) {
  if (!file?.name || !file.type || !Number.isFinite(file.size) || file.size <= 0) {
    return { code: API_ERROR_CODES.invalidMedia, message: 'Chọn file hợp lệ để upload.' }
  }

  return null
}

export function normalizeProgress(loaded, total) {
  if (!Number.isFinite(total) || total <= 0 || !Number.isFinite(loaded)) return 0
  return Math.min(100, Math.max(0, Math.round((loaded / total) * 100)))
}

function createInitialState() {
  return {
    query: { ...POST_MEDIA_DEFAULT_QUERY },
    phase: DIRECT_UPLOAD_PHASES.idle,
    progress: 0,
    checksumSha256: null,
    mediaId: null,
    data: null,
    success: false,
    error: null,
    failedPhase: null,
    traceId: null,
  }
}

const directUploadSlice = createSlice({
  name: 'directUpload',
  initialState: createInitialState(),
  reducers: {
    setQuery(state, action) {
      state.query = action.payload
    },
    checksumStarted(state) {
      state.phase = DIRECT_UPLOAD_PHASES.checksum
      state.progress = 0
      state.success = false
      state.error = null
      state.failedPhase = null
      state.data = null
      state.mediaId = null
      state.checksumSha256 = null
      state.traceId = null
    },
    checksumProgress(state, action) {
      state.progress = action.payload
    },
    checksumCompleted(state, action) {
      state.checksumSha256 = action.payload
      state.progress = 100
    },
    preparingStarted(state) {
      state.phase = DIRECT_UPLOAD_PHASES.preparing
      state.progress = 0
    },
    uploadStarted(state, action) {
      state.phase = DIRECT_UPLOAD_PHASES.upload
      state.progress = 0
      state.mediaId = action.payload.mediaId
      state.traceId = action.payload.traceId ?? null
    },
    uploadProgress(state, action) {
      state.progress = action.payload
    },
    finalizeStarted(state) {
      state.phase = DIRECT_UPLOAD_PHASES.finalize
      state.progress = 0
    },
    completed(state, action) {
      state.phase = DIRECT_UPLOAD_PHASES.complete
      state.progress = 100
      state.data = action.payload.data
      state.traceId = action.payload.traceId ?? null
      state.success = true
      state.error = null
      state.checksumSha256 = null
      state.mediaId = null
    },
    failed(state, action) {
      state.failedPhase = state.phase
      state.phase = DIRECT_UPLOAD_PHASES.error
      state.success = false
      state.error = action.payload
      state.checksumSha256 = null
    },
    reset() {
      return createInitialState()
    },
  },
})

export const directUploadActions = directUploadSlice.actions
export const directUploadReducer = directUploadSlice.reducer
