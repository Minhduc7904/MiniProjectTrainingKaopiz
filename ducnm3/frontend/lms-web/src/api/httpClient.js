import axios from 'axios'
import { ENV } from '@/constants/env'
import { HTTP_CONTENT_TYPES } from '@/constants/http'
import { readActor } from '@/auth/actorStorage'

export const httpClient = axios.create({
  baseURL: ENV.apiBaseUrl,
  timeout: ENV.apiTimeoutMs,
  headers: {
    Accept: HTTP_CONTENT_TYPES.json,
  },
})

httpClient.interceptors.request.use((config) => {
  const actor = readActor()
  if (actor) {
    config.headers.set('X-Actor-Type', actor.type)
    config.headers.set('X-Actor-Id', actor.id)
  }
  return config
})
