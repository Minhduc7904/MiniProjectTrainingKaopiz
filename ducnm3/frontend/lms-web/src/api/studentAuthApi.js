import axios from 'axios'
import { ENV } from '@/constants/env'
import { HTTP_CONTENT_TYPES } from '@/constants/http'
import { readStudentActor } from '@/auth/studentAuthStorage'
import { attachHttpLoggingInterceptors } from '@/api/httpLoggingInterceptors'
import { unwrapEnvelope } from '@/api/unwrapEnvelope'

const studentHttpClient = axios.create({
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

function unwrapStudentAuthData(response) {
  return unwrapEnvelope(response).data
}

export const studentAuthApi = {
  register: (payload) => studentHttpClient.post('/student/api/auth/register', payload).then(unwrapStudentAuthData),
  login: (payload) => studentHttpClient.post('/student/api/auth/login', payload).then(unwrapStudentAuthData),
  me: () => studentHttpClient.get('/student/api/auth/me').then(unwrapStudentAuthData),
}
