import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { fetchCoursesListRequest } from '@/api/coursesApi'
import { toApiError } from '@/api/toApiError'
import { GET_COURSES_DEFAULT_QUERY } from '@/constants/inputs/getCourses'
import { assignListQuery, createInitialListState, listRequestFulfilled, listRequestPending, listRequestRejected } from '@/features/createApiListSlice'

export const fetchCoursesList = createAsyncThunk('courses/fetchList', async (query, { rejectWithValue }) => {
  try { const { data, meta } = await fetchCoursesListRequest(query); return { data, pagination: meta.pagination, traceId: meta.traceId } } catch (error) { return rejectWithValue(toApiError(error)) }
})
const slice = createSlice({ name: 'courses', initialState: { list: createInitialListState(GET_COURSES_DEFAULT_QUERY) }, reducers: { setCoursesQuery(state, action) { assignListQuery(state.list, action.payload) } }, extraReducers: (builder) => builder.addCase(fetchCoursesList.pending, (s, a) => listRequestPending(s.list, a)).addCase(fetchCoursesList.fulfilled, (s, a) => listRequestFulfilled(s.list, a)).addCase(fetchCoursesList.rejected, (s, a) => listRequestRejected(s.list, a)) })
export const { setCoursesQuery } = slice.actions
export const coursesReducer = slice.reducer
