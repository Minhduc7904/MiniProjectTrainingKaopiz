export function toAiOutputJson({ data, pagination, traceId, error, success, query }) {
  if (error) {
    return { data: null, error, meta: { traceId, query } }
  }

  return { data, meta: { pagination, traceId, query }, success }
}

export function formatAiDate(value) {
  if (!value) {
    return '—'
  }

  return new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' }).format(
    new Date(value),
  )
}

export function emptyToUndefined(value) {
  const trimmed = typeof value === 'string' ? value.trim() : value
  return trimmed === '' ? undefined : trimmed
}
