import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import {
  fetchCoursesSummaryRequest,
  fetchHealthRequest,
  fetchMediaSummaryRequest,
  fetchStudentSummaryRequest,
} from '@/api/dashboardApi'
import { DASHBOARD_HEALTH_SERVICES } from '@/constants/dashboard'
import { toApiError } from '@/api/toApiError'

function createRequestState() {
  return { data: null, error: null, loading: false, success: false, traceId: null, httpStatus: null, latencyMs: null, checkedAt: null }
}

const initialState = {
  metrics: {
    students: createRequestState(),
    media: createRequestState(),
    courses: createRequestState(),
    lessons: createRequestState(),
  },
  health: Object.fromEntries(DASHBOARD_HEALTH_SERVICES.map(({ id }) => [id, createRequestState()])),
}

async function capture(id, request) {
  try {
    return { id, result: await request() }
  } catch (error) {
    return { id, error: toApiError(error) }
  }
}

export const refreshDashboard = createAsyncThunk('dashboard/refresh', async () => {
  const requests = [
    capture('students', fetchStudentSummaryRequest),
    capture('media', fetchMediaSummaryRequest),
    capture('courses', fetchCoursesSummaryRequest),
    ...DASHBOARD_HEALTH_SERVICES.map((service) => capture(`health:${service.id}`, () => fetchHealthRequest(service.route))),
  ]
  return Promise.all(requests)
})

export const refreshHealth = createAsyncThunk('dashboard/refreshHealth', async (serviceId, { rejectWithValue }) => {
  const service = DASHBOARD_HEALTH_SERVICES.find((item) => item.id === serviceId)
  if (!service) return rejectWithValue({ code: 'VALIDATION_FAILED', message: 'Service health không hợp lệ.' })
  return capture(serviceId, () => fetchHealthRequest(service.route))
})

function setLoading(request) {
  request.loading = true
  request.error = null
}

function setResult(request, outcome) {
  request.loading = false
  request.success = !outcome.error
  request.error = outcome.error ?? null
  request.data = outcome.result?.data ?? null
  request.traceId = outcome.result?.meta?.traceId ?? outcome.error?.traceId ?? null
  request.httpStatus = outcome.result?.httpStatus ?? outcome.error?.httpStatus ?? null
  request.latencyMs = outcome.result?.latencyMs ?? null
  request.checkedAt = outcome.result?.checkedAt ?? new Date().toISOString()
}

const dashboardSlice = createSlice({
  name: 'dashboard',
  initialState,
  extraReducers: (builder) => {
    builder
      .addCase(refreshDashboard.pending, (state) => {
        Object.values(state.metrics).forEach(setLoading)
        Object.values(state.health).forEach(setLoading)
      })
      .addCase(refreshDashboard.fulfilled, (state, action) => {
        action.payload.forEach((outcome) => {
          if (outcome.id === 'students') {
            setResult(state.metrics.students, outcome)
          } else if (outcome.id === 'media') {
            setResult(state.metrics.media, outcome)
          } else if (outcome.id === 'courses') {
            setResult(state.metrics.courses, outcome)
            setResult(state.metrics.lessons, outcome)
          } else if (outcome.id.startsWith('health:')) {
            setResult(state.health[outcome.id.replace('health:', '')], outcome)
          }
        })
      })
      .addCase(refreshHealth.pending, (state, action) => setLoading(state.health[action.meta.arg]))
      .addCase(refreshHealth.fulfilled, (state, action) => setResult(state.health[action.payload.id], action.payload))
      .addCase(refreshHealth.rejected, (state, action) => {
        const request = state.health[action.meta.arg]
        request.loading = false
        request.success = false
        request.error = action.payload ?? { message: 'Không thể kiểm tra service.' }
      })
  },
})

export const dashboardReducer = dashboardSlice.reducer
