import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { uploadMediaRequest } from '@/api/mediaApi'
import { toApiError } from '@/api/toApiError'
import { POST_MEDIA_FIELDS } from '@/constants/media'
import { POST_MEDIA_DEFAULT_QUERY } from '@/constants/inputs/postMedia'
import {
  assignMutationQuery,
  createInitialMutationState,
  mutationRequestFulfilled,
  mutationRequestPending,
  mutationRequestRejected,
} from '@/features/createApiMutationSlice'

function toStoredQuery(arg = {}) {
  return {
    [POST_MEDIA_FIELDS.mediaType]: arg.mediaType,
    [POST_MEDIA_FIELDS.uploadedByType]: arg.uploadedByType,
    [POST_MEDIA_FIELDS.uploadedBy]: arg.uploadedBy,
    fileName: arg.file?.name ?? null,
    fileSize: arg.file?.size ?? null,
  }
}

export const uploadMedia = createAsyncThunk(
  'media/upload',
  async (form, { rejectWithValue }) => {
    try {
      const { data, meta, location } = await uploadMediaRequest(form)
      return {
        data,
        traceId: meta.traceId,
        location,
      }
    } catch (error) {
      return rejectWithValue(toApiError(error))
    }
  },
)

const mediaSlice = createSlice({
  name: 'media',
  initialState: {
    upload: createInitialMutationState(POST_MEDIA_DEFAULT_QUERY),
  },
  reducers: {
    setMediaQuery(state, action) {
      assignMutationQuery(state.upload, action.payload)
    },
    resetMediaUpload(state) {
      state.upload = createInitialMutationState(POST_MEDIA_DEFAULT_QUERY)
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(uploadMedia.pending, (state, action) => {
        mutationRequestPending(state.upload, toStoredQuery(action.meta.arg))
      })
      .addCase(uploadMedia.fulfilled, (state, action) => {
        mutationRequestFulfilled(state.upload, action.payload)
      })
      .addCase(uploadMedia.rejected, (state, action) => {
        mutationRequestRejected(state.upload, action.payload)
      })
  },
})

export const { resetMediaUpload, setMediaQuery } = mediaSlice.actions
export const mediaReducer = mediaSlice.reducer
