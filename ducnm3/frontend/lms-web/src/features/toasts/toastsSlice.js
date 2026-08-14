import { createSlice } from '@reduxjs/toolkit'
import { TOAST_DURATION_MS, TOAST_PHASE } from '@/constants/toast'

const MAX_TOASTS = 5

function upsertToast(items, toast) {
  const next = items.filter((item) => item.id !== toast.id)
  next.unshift(toast)
  return next.slice(0, MAX_TOASTS)
}

const toastsSlice = createSlice({
  name: 'toasts',
  initialState: {
    items: [],
  },
  reducers: {
    toastRequestStarted(state, action) {
      const { id, method, path, durationMs } = action.payload
      state.items = upsertToast(state.items, {
        id,
        phase: TOAST_PHASE.pending,
        method,
        path,
        httpStatus: null,
        code: null,
        message: action.payload.message,
        traceId: id,
        durationMs,
      })
    },
    toastRequestSucceeded(state, action) {
      const current = state.items.find((item) => item.id === action.payload.id)
      if (!current) {
        return
      }

      current.phase = TOAST_PHASE.success
      current.httpStatus = action.payload.httpStatus
      current.code = action.payload.code
      current.message = action.payload.message
      current.traceId = action.payload.traceId ?? current.traceId
      current.durationMs = TOAST_DURATION_MS.dismiss
    },
    toastRequestFailed(state, action) {
      const current = state.items.find((item) => item.id === action.payload.id)
      const next = {
        id: action.payload.id,
        phase: TOAST_PHASE.error,
        method: current?.method ?? action.payload.method ?? null,
        path: current?.path ?? action.payload.path ?? null,
        httpStatus: action.payload.httpStatus,
        code: action.payload.code,
        message: action.payload.message,
        traceId: action.payload.traceId ?? current?.traceId ?? action.payload.id,
        durationMs: TOAST_DURATION_MS.dismiss,
      }
      state.items = upsertToast(state.items, next)
    },
    toastDismissed(state, action) {
      state.items = state.items.filter((item) => item.id !== action.payload)
    },
  },
})

export const {
  toastRequestStarted,
  toastRequestSucceeded,
  toastRequestFailed,
  toastDismissed,
} = toastsSlice.actions

export const toastsReducer = toastsSlice.reducer
