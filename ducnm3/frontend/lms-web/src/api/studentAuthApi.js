import axios from 'axios'
import { ENV } from '@/constants/env'
import { HTTP_CONTENT_TYPES } from '@/constants/http'
import { readStudentActor } from '@/auth/studentAuthStorage'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'

const studentHttpClient = axios.create({
  baseURL: ENV.apiBaseUrl,
  timeout: ENV.apiTimeoutMs,
  headers: { Accept: HTTP_CONTENT_TYPES.json },
})

studentHttpClient.interceptors.request.use((config) => {
  const actor = readStudentActor()
  if (actor) {
    config.headers.set('X-Actor-Type', actor.type)
    config.headers.set('X-Actor-Id', actor.id)
  }
  return config
})

export const studentAuthApi = {
  register: (payload) => studentHttpClient.post('/student/api/auth/register', payload).then(unwrapEnvelope),
  login: (payload) => studentHttpClient.post('/student/api/auth/login', payload).then(unwrapEnvelope),
  me: () => studentHttpClient.get('/student/api/auth/me').then(unwrapEnvelope),
}
