import { API_ERROR_CODES } from '@/constants/apiErrorCodes'

export function createInitialMutationState(query = {}) {
  return {
    data: null,
    query,
    loading: false,
    success: false,
    error: null,
    traceId: null,
    location: null,
  }
}

export function mutationRequestPending(state, query) {
  state.loading = true
  state.success = false
  state.error = null
  state.query = query
}

export function mutationRequestFulfilled(state, payload) {
  state.loading = false
  state.success = true
  state.data = payload.data ?? null
  state.traceId = payload.traceId ?? null
  state.location = payload.location ?? null
}

export function mutationRequestRejected(state, payload) {
  state.loading = false
  state.success = false
  state.error = payload ?? {
    code: API_ERROR_CODES.unexpectedError,
    message: 'Đã xảy ra lỗi không xác định.',
  }
}

export function assignMutationQuery(state, query) {
  state.query = query
}
