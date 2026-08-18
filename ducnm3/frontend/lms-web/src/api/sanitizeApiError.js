import { sanitizeSensitiveValue } from '@/utils/sanitizeSensitiveValue'

export function sanitizeApiError(error) {
  return {
    code: error?.code ?? 'UNEXPECTED_ERROR',
    message: error?.message ?? 'Đã xảy ra lỗi không xác định.',
    httpStatus: error?.httpStatus ?? null,
    traceId: error?.traceId ?? null,
    details: sanitizeSensitiveValue(error?.details ?? null),
  }
}
