import { ENV } from '@/constants/env'

export const TOAST_PHASE = {
  pending: 'pending',
  success: 'success',
  error: 'error',
}

export const TOAST_MESSAGES = {
  pending: 'Đang gọi API',
  success: 'Thành công',
  timeout: 'Hết thời gian chờ API.',
}

export const TOAST_PHASE_LABELS = {
  pending: 'Đang gọi API',
  success: 'Thành công',
  error: 'Lỗi',
}

export const TOAST_DURATION_MS = {
  pending: ENV.apiTimeoutMs,
  dismiss: 2000,
}

export const TOAST_HOVER_SCALE = 1.04

export const TOAST_CONFIG_KEY = 'apiToast'
export const SILENT_TOAST_CONFIG = { skip: true }
