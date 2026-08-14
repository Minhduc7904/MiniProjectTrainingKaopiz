import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { createNotificationBatchRequest, fetchNotificationBatchFailedItemsRequest, fetchNotificationBatchRequest } from '@/api/notificationBatchesApi'
import { toApiError } from '@/api/toApiError'
import { GET_NOTIFICATION_BATCH_DEFAULT_QUERY } from '@/constants/inputs/getNotificationBatch'
import { POST_NOTIFICATION_BATCH_DEFAULT_QUERY } from '@/constants/inputs/postNotificationBatch'
import { createInitialMutationState, mutationRequestFulfilled, mutationRequestPending, mutationRequestRejected } from '@/features/createApiMutationSlice'

const initialProgress = {
  data: null, query: GET_NOTIFICATION_BATCH_DEFAULT_QUERY, loading: false, success: false,
  error: null, traceId: null, paused: false, activeBatchId: null, failures: [], failuresLoading: false, failuresError: null,
}

export const createNotificationBatch = createAsyncThunk('notificationBatches/create', async (payload, { rejectWithValue }) => {
  try { const { data, meta, location } = await createNotificationBatchRequest(payload); return { data, traceId: meta.traceId, location } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

export const fetchNotificationBatch = createAsyncThunk('notificationBatches/fetch', async (batchId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchNotificationBatchRequest(batchId); return { data, traceId: meta.traceId } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

export const fetchNotificationBatchFailures = createAsyncThunk('notificationBatches/failures', async (batchId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchNotificationBatchFailedItemsRequest(batchId); return { data: data.items ?? [], traceId: meta.traceId } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

const notificationBatchesSlice = createSlice({
  name: 'notificationBatches',
  initialState: { create: createInitialMutationState(POST_NOTIFICATION_BATCH_DEFAULT_QUERY), progress: initialProgress },
  reducers: {
    setNotificationBatchCreateQuery(state, action) { state.create.query = action.payload },
    resetNotificationBatchCreate(state) { state.create = createInitialMutationState(POST_NOTIFICATION_BATCH_DEFAULT_QUERY) },
    setNotificationBatchProgressQuery(state, action) { state.progress.query = { batchId: action.payload } },
    startNotificationBatchPolling(state, action) { const batchId = action.payload.trim(); state.progress.query = { batchId }; state.progress.activeBatchId = batchId; state.progress.data = null; state.progress.error = null; state.progress.success = false; state.progress.paused = false; state.progress.failures = []; state.progress.failuresError = null },
    setNotificationBatchPollingPaused(state, action) { state.progress.paused = action.payload },
    resetNotificationBatchProgress(state) { state.progress = initialProgress },
  },
  extraReducers: (builder) => builder
    .addCase(createNotificationBatch.pending, (state, action) => mutationRequestPending(state.create, action.meta.arg))
    .addCase(createNotificationBatch.fulfilled, (state, action) => mutationRequestFulfilled(state.create, action.payload))
    .addCase(createNotificationBatch.rejected, (state, action) => mutationRequestRejected(state.create, action.payload))
    .addCase(fetchNotificationBatch.pending, (state) => { state.progress.loading = true; state.progress.error = null })
    .addCase(fetchNotificationBatch.fulfilled, (state, action) => { state.progress.loading = false; state.progress.success = true; state.progress.data = action.payload.data; state.progress.traceId = action.payload.traceId })
    .addCase(fetchNotificationBatch.rejected, (state, action) => { state.progress.loading = false; state.progress.success = false; state.progress.error = action.payload ?? { code: 'UNEXPECTED_ERROR', message: 'Đã xảy ra lỗi không xác định.' } })
    .addCase(fetchNotificationBatchFailures.pending, (state) => { state.progress.failuresLoading = true; state.progress.failuresError = null })
    .addCase(fetchNotificationBatchFailures.fulfilled, (state, action) => { state.progress.failuresLoading = false; state.progress.failures = action.payload.data; state.progress.failuresTraceId = action.payload.traceId })
    .addCase(fetchNotificationBatchFailures.rejected, (state, action) => { state.progress.failuresLoading = false; state.progress.failuresError = action.payload ?? { code: 'UNEXPECTED_ERROR', message: 'Không đọc được lỗi người nhận.' } }),
})

export const { setNotificationBatchCreateQuery, resetNotificationBatchCreate, setNotificationBatchProgressQuery, startNotificationBatchPolling, setNotificationBatchPollingPaused, resetNotificationBatchProgress } = notificationBatchesSlice.actions
export const notificationBatchesReducer = notificationBatchesSlice.reducer
