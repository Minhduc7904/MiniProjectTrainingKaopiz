import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { createNotificationBatchRequest, fetchNotificationBatchDeliveryStatusRequest, fetchNotificationBatchFailedItemsRequest, fetchNotificationBatchRequest, fetchNotificationBatchSnapshotStatusRequest, fetchNotificationBatchesRequest, fetchNotificationMediaUsageJobStatusRequest, retryNotificationBatchFailuresRequest } from '@/api/notificationBatchesApi'
import { toApiError } from '@/api/toApiError'
import { GET_NOTIFICATION_BATCH_DEFAULT_QUERY } from '@/constants/inputs/getNotificationBatch'
import { POST_NOTIFICATION_BATCH_DEFAULT_QUERY } from '@/constants/inputs/postNotificationBatch'
import { createInitialMutationState, mutationRequestFulfilled, mutationRequestPending, mutationRequestRejected } from '@/features/createApiMutationSlice'

const initialProgress = {
  data: null, query: GET_NOTIFICATION_BATCH_DEFAULT_QUERY, loading: false, success: false,
  error: null, traceId: null, paused: false, activeBatchId: null, failures: [], failuresLoading: false, failuresError: null, retryLoading: false, retryError: null,
  currentStep: 'snapshot', snapshot: null, delivery: null, mediaUsage: null,
  stepTraceIds: { snapshot: null, delivery: null, mediaUsage: null },
}
const initialList = { data: [], pagination: { page: 1, pageSize: 20, totalItems: 0, totalPages: 0 }, query: { page: 1, pageSize: 20, status: '' }, loading: false, success: false, error: null, traceId: null, retryingBatchId: null, retryError: null }

export const createNotificationBatch = createAsyncThunk('notificationBatches/create', async (payload, { rejectWithValue }) => {
  try { const { data, meta, location } = await createNotificationBatchRequest(payload); return { data, traceId: meta.traceId, location } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

export const fetchNotificationBatch = createAsyncThunk('notificationBatches/fetch', async (batchId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchNotificationBatchRequest(batchId); return { data, traceId: meta.traceId } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

export const fetchNotificationBatchSnapshotStatus = createAsyncThunk('notificationBatches/fetchSnapshotStatus', async (batchId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchNotificationBatchSnapshotStatusRequest(batchId); return { data, traceId: meta.traceId } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

export const fetchNotificationBatchDeliveryStatus = createAsyncThunk('notificationBatches/fetchDeliveryStatus', async (batchId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchNotificationBatchDeliveryStatusRequest(batchId); return { data, traceId: meta.traceId } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

export const fetchNotificationMediaUsageJobStatus = createAsyncThunk('notificationBatches/fetchMediaUsageStatus', async (jobId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchNotificationMediaUsageJobStatusRequest(jobId); return { data, traceId: meta.traceId } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

export const fetchNotificationBatchFailures = createAsyncThunk('notificationBatches/failures', async (batchId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchNotificationBatchFailedItemsRequest(batchId); return { data: data.items ?? [], traceId: meta.traceId } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

export const fetchNotificationBatches = createAsyncThunk('notificationBatches/list', async (query, { rejectWithValue }) => {
  try { const { data, meta } = await fetchNotificationBatchesRequest({ ...query, status: query.status || undefined }); return { data, pagination: meta.pagination, traceId: meta.traceId, query } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

export const retryNotificationBatchFailures = createAsyncThunk('notificationBatches/retryFailures', async (batchId, { rejectWithValue }) => {
  try { const { data, meta, location } = await retryNotificationBatchFailuresRequest(batchId); return { data, traceId: meta.traceId, location } }
  catch (error) { return rejectWithValue(toApiError(error)) }
})

const notificationBatchesSlice = createSlice({
  name: 'notificationBatches',
  initialState: { create: createInitialMutationState(POST_NOTIFICATION_BATCH_DEFAULT_QUERY), progress: initialProgress, list: initialList },
  reducers: {
    setNotificationBatchCreateQuery(state, action) { state.create.query = action.payload },
    resetNotificationBatchCreate(state) { state.create = createInitialMutationState(POST_NOTIFICATION_BATCH_DEFAULT_QUERY) },
    setNotificationBatchProgressQuery(state, action) { state.progress.query = { batchId: action.payload } },
    startNotificationBatchPolling(state, action) { const batchId = action.payload.trim(); state.progress = { ...initialProgress, query: { batchId }, activeBatchId: batchId } },
    setNotificationBatchPollingPaused(state, action) { state.progress.paused = action.payload },
    resetNotificationBatchProgress(state) { state.progress = initialProgress },
    setNotificationBatchListQuery(state, action) { state.list.query = action.payload },
  },
  extraReducers: (builder) => builder
    .addCase(createNotificationBatch.pending, (state, action) => mutationRequestPending(state.create, action.meta.arg))
    .addCase(createNotificationBatch.fulfilled, (state, action) => mutationRequestFulfilled(state.create, action.payload))
    .addCase(createNotificationBatch.rejected, (state, action) => mutationRequestRejected(state.create, action.payload))
    .addCase(fetchNotificationBatch.pending, (state) => { state.progress.loading = true; state.progress.error = null })
    .addCase(fetchNotificationBatch.fulfilled, (state, action) => { state.progress.loading = false; state.progress.success = true; state.progress.data = action.payload.data; state.progress.traceId = action.payload.traceId })
    .addCase(fetchNotificationBatch.rejected, (state, action) => { state.progress.loading = false; state.progress.success = false; state.progress.error = action.payload ?? { code: 'UNEXPECTED_ERROR', message: 'Đã xảy ra lỗi không xác định.' } })
    .addCase(fetchNotificationBatchSnapshotStatus.pending, (state) => { state.progress.loading = true; state.progress.currentStep = 'snapshot'; state.progress.error = null })
    .addCase(fetchNotificationBatchSnapshotStatus.fulfilled, (state, action) => { state.progress.loading = false; state.progress.success = true; state.progress.snapshot = action.payload.data; state.progress.stepTraceIds.snapshot = action.payload.traceId; state.progress.traceId = action.payload.traceId })
    .addCase(fetchNotificationBatchSnapshotStatus.rejected, (state, action) => { state.progress.loading = false; state.progress.success = false; state.progress.error = action.payload ?? { code: 'UNEXPECTED_ERROR', message: 'Không đọc được trạng thái snapshot.' } })
    .addCase(fetchNotificationBatchDeliveryStatus.pending, (state) => { state.progress.loading = true; state.progress.currentStep = 'delivery'; state.progress.error = null })
    .addCase(fetchNotificationBatchDeliveryStatus.fulfilled, (state, action) => { state.progress.loading = false; state.progress.success = true; state.progress.delivery = action.payload.data; state.progress.data = action.payload.data; state.progress.stepTraceIds.delivery = action.payload.traceId; state.progress.traceId = action.payload.traceId })
    .addCase(fetchNotificationBatchDeliveryStatus.rejected, (state, action) => { state.progress.loading = false; state.progress.success = false; state.progress.error = action.payload ?? { code: 'UNEXPECTED_ERROR', message: 'Không đọc được trạng thái delivery.' } })
    .addCase(fetchNotificationMediaUsageJobStatus.pending, (state) => { state.progress.loading = true; state.progress.currentStep = 'mediaUsage'; state.progress.error = null })
    .addCase(fetchNotificationMediaUsageJobStatus.fulfilled, (state, action) => { state.progress.loading = false; state.progress.success = true; state.progress.mediaUsage = action.payload.data; state.progress.stepTraceIds.mediaUsage = action.payload.traceId; state.progress.traceId = action.payload.traceId })
    .addCase(fetchNotificationMediaUsageJobStatus.rejected, (state, action) => { state.progress.loading = false; state.progress.success = false; state.progress.error = action.payload ?? { code: 'UNEXPECTED_ERROR', message: 'Không đọc được trạng thái Media Usage.' } })
    .addCase(fetchNotificationBatchFailures.pending, (state) => { state.progress.failuresLoading = true; state.progress.failuresError = null })
    .addCase(fetchNotificationBatchFailures.fulfilled, (state, action) => { state.progress.failuresLoading = false; state.progress.failures = action.payload.data; state.progress.failuresTraceId = action.payload.traceId })
    .addCase(fetchNotificationBatchFailures.rejected, (state, action) => { state.progress.failuresLoading = false; state.progress.failuresError = action.payload ?? { code: 'UNEXPECTED_ERROR', message: 'Không đọc được lỗi người nhận.' } })
    .addCase(fetchNotificationBatches.pending, (state, action) => { state.list.loading = true; state.list.error = null; state.list.query = action.meta.arg })
    .addCase(fetchNotificationBatches.fulfilled, (state, action) => { state.list.loading = false; state.list.success = true; state.list.data = action.payload.data; state.list.pagination = action.payload.pagination; state.list.traceId = action.payload.traceId })
    .addCase(fetchNotificationBatches.rejected, (state, action) => { state.list.loading = false; state.list.success = false; state.list.error = action.payload ?? { code: 'UNEXPECTED_ERROR', message: 'Không đọc được danh sách batch.' } })
    .addCase(retryNotificationBatchFailures.pending, (state, action) => { state.progress.retryLoading = true; state.progress.retryError = null; state.list.retryingBatchId = action.meta.arg; state.list.retryError = null })
    .addCase(retryNotificationBatchFailures.fulfilled, (state, action) => { state.progress = { ...initialProgress, activeBatchId: action.payload.data.id, query: { batchId: action.payload.data.id }, traceId: action.payload.traceId }; state.list.retryingBatchId = null })
    .addCase(retryNotificationBatchFailures.rejected, (state, action) => { const error = action.payload ?? { code: 'UNEXPECTED_ERROR', message: 'Không retry được batch.' }; state.progress.retryLoading = false; state.progress.retryError = error; state.list.retryingBatchId = null; state.list.retryError = error }),
})

export const { setNotificationBatchCreateQuery, resetNotificationBatchCreate, setNotificationBatchProgressQuery, startNotificationBatchPolling, setNotificationBatchPollingPaused, resetNotificationBatchProgress, setNotificationBatchListQuery } = notificationBatchesSlice.actions
export const notificationBatchesReducer = notificationBatchesSlice.reducer
