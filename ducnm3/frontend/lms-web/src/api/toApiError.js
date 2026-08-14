import { API_ERROR_CODES, AXIOS_ERROR_CODES } from '@/constants/apiErrorCodes'
import { TOAST_MESSAGES } from '@/constants/toast'

export function toApiError(error) {
  const payload = error?.response?.data?.error
  if (payload?.code) {
    return {
      code: payload.code,
      message: payload.message || payload.code,
      details: payload.details ?? null,
      httpStatus: error.response?.status ?? null,
      traceId: error.response?.data?.meta?.traceId ?? null,
    }
  }

  if (error?.code === AXIOS_ERROR_CODES.timeout) {
    return {
      code: API_ERROR_CODES.requestTimeout,
      message: TOAST_MESSAGES.timeout,
      httpStatus: null,
      traceId: null,
    }
  }

  if (
    error?.code === AXIOS_ERROR_CODES.network ||
    error?.message === 'Network Error'
  ) {
    return {
      code: API_ERROR_CODES.networkError,
      message: 'Không kết nối được API Gateway.',
      httpStatus: null,
      traceId: null,
    }
  }

  return {
    code: API_ERROR_CODES.unexpectedError,
    message: error?.message || 'Đã xảy ra lỗi không xác định.',
    httpStatus: error?.response?.status ?? null,
    traceId: null,
  }
}
