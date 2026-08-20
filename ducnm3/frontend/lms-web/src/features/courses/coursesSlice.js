import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { fetchCourseDetailsRequest, fetchCoursesListRequest } from '@/api/coursesApi'
import { toApiError } from '@/api/toApiError'
import { GET_COURSES_DEFAULT_QUERY } from '@/constants/inputs/getCourses'
import { assignListQuery, createInitialListState, listRequestFulfilled, listRequestPending, listRequestRejected } from '@/features/createApiListSlice'

export const fetchCoursesList = createAsyncThunk('courses/fetchList', async (query, { rejectWithValue }) => {
  try { const { data, meta } = await fetchCoursesListRequest(query); return { data, pagination: meta.pagination, traceId: meta.traceId } } catch (error) { return rejectWithValue(toApiError(error)) }
})
export const fetchCourseDetails = createAsyncThunk('courses/fetchDetails', async (courseId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchCourseDetailsRequest(courseId); return { data, traceId: meta.traceId } } catch (error) { return rejectWithValue(toApiError(error)) }
})
const slice = createSlice({ name: 'courses', initialState: { list: createInitialListState(GET_COURSES_DEFAULT_QUERY), detail: { data: null, loading: false, error: null, traceId: null } }, reducers: { setCoursesQuery(state, action) { assignListQuery(state.list, action.payload) } }, extraReducers: (builder) => builder.addCase(fetchCoursesList.pending, (s, a) => listRequestPending(s.list, a)).addCase(fetchCoursesList.fulfilled, (s, a) => listRequestFulfilled(s.list, a)).addCase(fetchCoursesList.rejected, (s, a) => listRequestRejected(s.list, a)).addCase(fetchCourseDetails.pending, (s) => { s.detail.loading = true; s.detail.error = null }).addCase(fetchCourseDetails.fulfilled, (s, a) => { s.detail.loading = false; s.detail.data = a.payload.data; s.detail.traceId = a.payload.traceId }).addCase(fetchCourseDetails.rejected, (s, a) => { s.detail.loading = false; s.detail.error = a.payload ?? { message: 'Không tải được khóa học.' } }) })
export const { setCoursesQuery } = slice.actions
export const coursesReducer = slice.reducer
