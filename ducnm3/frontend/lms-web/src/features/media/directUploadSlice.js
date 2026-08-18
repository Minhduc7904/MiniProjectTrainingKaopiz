import { createSlice } from '@reduxjs/toolkit'
import { API_ERROR_CODES } from '@/constants/apiErrorCodes'
import { POST_MEDIA_DEFAULT_QUERY } from '@/constants/inputs/postMedia'
import { MEDIA_LIMITS_MIB, MEDIA_TYPES } from '@/constants/media'

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

const DOCUMENT_CONTENT_TYPES = new Set([
  'application/msword',
  'application/pdf',
  'application/rtf',
  'application/vnd.ms-excel',
  'application/vnd.ms-powerpoint',
  'application/vnd.openxmlformats-officedocument.presentationml.presentation',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'text/csv',
  'text/markdown',
  'text/plain',
])

function contentTypeMatches(mediaType, contentType) {
  const normalized = String(contentType ?? '').toLowerCase()
  if (mediaType === MEDIA_TYPES.image) return normalized.startsWith('image/')
  if (mediaType === MEDIA_TYPES.video) return normalized.startsWith('video/')
  if (mediaType === MEDIA_TYPES.audio) return normalized.startsWith('audio/')
  if (mediaType === MEDIA_TYPES.document) return DOCUMENT_CONTENT_TYPES.has(normalized)
  if (mediaType === MEDIA_TYPES.other) {
    return !normalized.startsWith('image/') &&
      !normalized.startsWith('video/') &&
      !normalized.startsWith('audio/') &&
      !DOCUMENT_CONTENT_TYPES.has(normalized)
  }
  return false
}

export function validateDirectUploadFile(file, mediaType) {
  if (!file?.name || !file.type || !Number.isFinite(file.size) || file.size <= 0) {
    return { code: API_ERROR_CODES.invalidMedia, message: 'Chọn file hợp lệ để upload.' }
  }

  if (!contentTypeMatches(mediaType, file.type)) {
    return {
      code: API_ERROR_CODES.unsupportedMediaType,
      message: 'MIME của file không khớp loại media đã chọn.',
    }
  }

  const limitMiB = MEDIA_LIMITS_MIB[mediaType]
  if (!limitMiB || file.size > limitMiB * 1024 * 1024) {
    return {
      code: API_ERROR_CODES.payloadTooLarge,
      message: `File vượt giới hạn ${limitMiB ?? 0} MiB của loại media.`,
    }
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
