import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { enrollStudentInCourseRequest, fetchMyCourseProgressRequest, fetchStudentCourseCatalogRequest, fetchStudentEnrollmentDetailRequest, fetchStudentEnrollmentsRequest } from '@/api/studentLearningApi'
import { toApiError } from '@/api/toApiError'
import { createInitialListState, listRequestFulfilled, listRequestPending, listRequestRejected } from '@/features/createApiListSlice'

export const STUDENT_ENROLLMENTS_DEFAULT_QUERY = { page: 1, pageSize: 12 }
export const STUDENT_COURSE_CATALOG_DEFAULT_QUERY = { page: 1, pageSize: 12 }

export const fetchStudentEnrollments = createAsyncThunk('studentLearning/fetchEnrollments', async (query, { dispatch, rejectWithValue }) => {
  try {
    const { data, meta } = await fetchStudentEnrollmentsRequest(query)
    void Promise.all(data.map((course) => dispatch(fetchMyCourseProgress(course.courseId))))
    return { data, pagination: meta.pagination, traceId: meta.traceId }
  } catch (error) { return rejectWithValue(toApiError(error)) }
})

export const fetchMyCourseProgress = createAsyncThunk('studentLearning/fetchProgress', async (courseId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchMyCourseProgressRequest(courseId); return { courseId, data, traceId: meta.traceId } } catch (error) { return rejectWithValue({ courseId, error: toApiError(error) }) }
})

export const fetchStudentCourseCatalog = createAsyncThunk('studentLearning/fetchCatalog', async (query, { rejectWithValue }) => {
  try { const { data, meta } = await fetchStudentCourseCatalogRequest(query); return { data, pagination: meta.pagination, traceId: meta.traceId } } catch (error) { return rejectWithValue(toApiError(error)) }
})

export const enrollStudentCourse = createAsyncThunk('studentLearning/enrollCourse', async (courseId, { rejectWithValue }) => {
  try { const { data, meta } = await enrollStudentInCourseRequest(courseId); return { courseId, data, traceId: meta.traceId } } catch (error) { return rejectWithValue({ courseId, error: toApiError(error) }) }
})

export const fetchStudentEnrollmentDetail = createAsyncThunk('studentLearning/fetchDetail', async (courseId, { rejectWithValue }) => {
  try { const { data, meta } = await fetchStudentEnrollmentDetailRequest(courseId); return { data, traceId: meta.traceId } } catch (error) { return rejectWithValue(toApiError(error)) }
})

const slice = createSlice({
  name: 'studentLearning',
  initialState: {
    enrollments: createInitialListState(STUDENT_ENROLLMENTS_DEFAULT_QUERY),
    catalog: createInitialListState(STUDENT_COURSE_CATALOG_DEFAULT_QUERY),
    detail: { data: null, loading: false, error: null, traceId: null },
    progressByCourseId: {},
    enrollmentByCourseId: {},
  },
  reducers: {},
  extraReducers: (builder) => builder
    .addCase(fetchStudentEnrollments.pending, (state, action) => listRequestPending(state.enrollments, action))
    .addCase(fetchStudentEnrollments.fulfilled, (state, action) => listRequestFulfilled(state.enrollments, action))
    .addCase(fetchStudentEnrollments.rejected, (state, action) => listRequestRejected(state.enrollments, action))
    .addCase(fetchStudentCourseCatalog.pending, (state, action) => listRequestPending(state.catalog, action))
    .addCase(fetchStudentCourseCatalog.fulfilled, (state, action) => listRequestFulfilled(state.catalog, action))
    .addCase(fetchStudentCourseCatalog.rejected, (state, action) => listRequestRejected(state.catalog, action))
    .addCase(enrollStudentCourse.pending, (state, action) => { state.enrollmentByCourseId[action.meta.arg] = { loading: true, error: null } })
    .addCase(enrollStudentCourse.fulfilled, (state, action) => { state.enrollmentByCourseId[action.payload.courseId] = { loading: false, error: null }; state.catalog.data = state.catalog.data.filter((course) => course.courseId !== action.payload.courseId) })
    .addCase(enrollStudentCourse.rejected, (state, action) => { const { courseId, error } = action.payload ?? {}; if (courseId) state.enrollmentByCourseId[courseId] = { loading: false, error } })
    .addCase(fetchMyCourseProgress.pending, (state, action) => { state.progressByCourseId[action.meta.arg] = { loading: true, data: null, error: null } })
    .addCase(fetchMyCourseProgress.fulfilled, (state, action) => { state.progressByCourseId[action.payload.courseId] = { loading: false, data: action.payload.data, error: null, traceId: action.payload.traceId } })
    .addCase(fetchMyCourseProgress.rejected, (state, action) => { const { courseId, error } = action.payload ?? {}; if (courseId) state.progressByCourseId[courseId] = { loading: false, data: null, error } })
    .addCase(fetchStudentEnrollmentDetail.pending, (state) => { state.detail = { data: null, loading: true, error: null, traceId: null } })
    .addCase(fetchStudentEnrollmentDetail.fulfilled, (state, action) => { state.detail = { data: action.payload.data, loading: false, error: null, traceId: action.payload.traceId } })
    .addCase(fetchStudentEnrollmentDetail.rejected, (state, action) => { state.detail.loading = false; state.detail.error = action.payload ?? { message: 'Không tải được khóa học.' } }),
})

export const studentLearningReducer = slice.reducer
