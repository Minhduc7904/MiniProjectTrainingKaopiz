export const REDACTED_VALUE = '[REDACTED]'

export function isSensitiveKey(key) {
  const normalized = String(key).toLowerCase().replaceAll('_', '-')
  const compact = normalized.replaceAll('-', '')
  return compact === 'uploadurl' ||
    compact === 'formfields' ||
    compact === 'policy' ||
    compact === 'signature' ||
    normalized.startsWith('x-amz-') ||
    compact.includes('credential') ||
    compact.includes('token') ||
    compact === 'authorization' ||
    compact === 'proxyauthorization' ||
    compact === 'cookie' ||
    compact === 'setcookie'
}

export function sanitizeSensitiveValue(value, seen = new WeakSet()) {
  if (value == null || typeof value !== 'object') return value
  if (seen.has(value)) return '[CIRCULAR]'
  seen.add(value)

  if (Array.isArray(value)) {
    return value.map((item) => sanitizeSensitiveValue(item, seen))
  }

  return Object.fromEntries(
    Object.entries(value).map(([key, item]) => [
      key,
      isSensitiveKey(key)
        ? REDACTED_VALUE
        : sanitizeSensitiveValue(item, seen),
    ]),
  )
}
