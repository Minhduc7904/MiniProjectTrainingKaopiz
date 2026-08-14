import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import { fetchStudentsListRequest } from '@/api/studentsApi'
import { toApiError } from '@/api/toApiError'
import { GET_STUDENTS_DEFAULT_QUERY } from '@/constants/inputs/getStudents'
import {
  assignListQuery,
  createInitialListState,
  listRequestFulfilled,
  listRequestPending,
  listRequestRejected,
} from '@/features/createApiListSlice'

export const fetchStudentsList = createAsyncThunk(
  'students/fetchList',
  async (query, { rejectWithValue }) => {
    try {
      const { data, meta } = await fetchStudentsListRequest(query)
      return {
        data,
        pagination: meta.pagination,
        traceId: meta.traceId,
      }
    } catch (error) {
      return rejectWithValue(toApiError(error))
    }
  },
)

const studentsSlice = createSlice({
  name: 'students',
  initialState: {
    list: createInitialListState(GET_STUDENTS_DEFAULT_QUERY),
  },
  reducers: {
    setStudentsQuery(state, action) {
      assignListQuery(state.list, action.payload)
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchStudentsList.pending, (state, action) => {
        listRequestPending(state.list, action)
      })
      .addCase(fetchStudentsList.fulfilled, (state, action) => {
        listRequestFulfilled(state.list, action)
      })
      .addCase(fetchStudentsList.rejected, (state, action) => {
        listRequestRejected(state.list, action)
      })
  },
})

export const { setStudentsQuery } = studentsSlice.actions
export const studentsReducer = studentsSlice.reducer
