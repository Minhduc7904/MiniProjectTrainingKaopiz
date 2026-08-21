import axios from 'axios'
import { ENV } from '@/constants/env'
import { HTTP_CONTENT_TYPES } from '@/constants/http'
import { readStudentActor } from '@/auth/studentAuthStorage'
import { attachHttpLoggingInterceptors } from '@/api/httpLoggingInterceptors'

export const studentHttpClient = axios.create({
  baseURL: ENV.apiBaseUrl,
  timeout: ENV.apiTimeoutMs,
  headers: { Accept: HTTP_CONTENT_TYPES.json },
})

attachHttpLoggingInterceptors(studentHttpClient)

studentHttpClient.interceptors.request.use((config) => {
  const actor = readStudentActor()
  if (actor) {
    config.headers.set('X-Actor-Type', actor.type)
    config.headers.set('X-Actor-Id', actor.id)
  }
  return config
})
