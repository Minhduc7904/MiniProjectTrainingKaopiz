import { PAGINATION } from '@/constants/pagination'
import { API_ERROR_CODES } from '@/constants/apiErrorCodes'

export function createInitialListState(query = {}) {
  return {
    data: [],
    pagination: {
      type: PAGINATION.types.offset,
      page: query.page ?? PAGINATION.defaults.page,
      pageSize: query.pageSize ?? PAGINATION.defaults.pageSize,
      totalItems: 0,
      totalPages: 0,
    },
    query,
    loading: false,
    success: false,
    error: null,
    traceId: null,
  }
}

export function listRequestPending(state, action) {
  state.loading = true
  state.success = false
  state.error = null
  if (action.meta.arg) {
    state.query = action.meta.arg
  }
}

export function listRequestFulfilled(state, action) {
  state.loading = false
  state.success = true
  state.data = action.payload.data ?? []
  state.pagination = {
    ...state.pagination,
    ...(action.payload.pagination ?? {}),
  }
  state.traceId = action.payload.traceId ?? null
}

export function listRequestRejected(state, action) {
  state.loading = false
  state.success = false
  state.error = action.payload ?? {
    code: API_ERROR_CODES.unexpectedError,
    message: 'Đã xảy ra lỗi không xác định.',
  }
}

export function assignListQuery(state, query) {
  state.query = query
}
