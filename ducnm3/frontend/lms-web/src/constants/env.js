function requiredEnv(name) {
  const value = import.meta.env[name]
  if (typeof value !== 'string' || value.trim().length === 0) {
    throw new Error(`${name} is required. Copy frontend/lms-web/.env.example to .env.`)
  }

  return value.trim()
}

function parseFlag(value, fallback) {
  if (value == null || value === '') {
    return fallback
  }

  return value === 'true' || value === '1'
}

export const ENV = {
  appName: requiredEnv('VITE_APP_NAME'),
  apiBaseUrl: requiredEnv('VITE_API_BASE_URL').replace(/\/$/, ''),
  apiTimeoutMs: Number(import.meta.env.VITE_API_TIMEOUT_MS || 15000),
  isDev: import.meta.env.DEV,
  httpLog: parseFlag(import.meta.env.VITE_HTTP_LOG, import.meta.env.DEV),
}
