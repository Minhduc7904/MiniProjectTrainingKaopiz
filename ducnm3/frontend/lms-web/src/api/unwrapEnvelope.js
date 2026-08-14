export function unwrapEnvelope(response) {
  const body = response?.data ?? {}

  return {
    data: body.data ?? null,
    meta: body.meta ?? {},
    error: body.error ?? null,
  }
}
