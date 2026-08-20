import { ENV } from '@/constants/env'

export function resolveMediaContentUrl(contentUrl) {
  if (!contentUrl) return contentUrl
  return new URL(contentUrl, `${ENV.apiBaseUrl}/`).toString()
}
