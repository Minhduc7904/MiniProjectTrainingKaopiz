import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { fetchMediaJobsRequest } from '@/api/mediaJobsApi'
import { toApiError } from '@/api/toApiError'
import { GET_MEDIA_JOBS_DEFAULT_QUERY } from '@/constants/inputs/getMediaJobs'
import { assignListQuery, createInitialListState, listRequestFulfilled, listRequestPending, listRequestRejected } from '@/features/createApiListSlice'

export const fetchMediaJobs = createAsyncThunk('mediaJobs/fetchList', async (query, { rejectWithValue }) => {
  try {
    const { data, meta } = await fetchMediaJobsRequest(query)
    return { data, pagination: meta.pagination, traceId: meta.traceId }
  } catch (error) {
    return rejectWithValue(toApiError(error))
  }
})

const mediaJobsSlice = createSlice({
  name: 'mediaJobs',
  initialState: { list: createInitialListState(GET_MEDIA_JOBS_DEFAULT_QUERY) },
  reducers: {
    setMediaJobsQuery(state, action) { assignListQuery(state.list, action.payload) },
  },
  extraReducers: (builder) => builder
    .addCase(fetchMediaJobs.pending, (state, action) => listRequestPending(state.list, action))
    .addCase(fetchMediaJobs.fulfilled, (state, action) => listRequestFulfilled(state.list, action))
    .addCase(fetchMediaJobs.rejected, (state, action) => listRequestRejected(state.list, action)),
})

export const { setMediaJobsQuery } = mediaJobsSlice.actions
export const mediaJobsReducer = mediaJobsSlice.reducer
