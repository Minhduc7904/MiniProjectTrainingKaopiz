import { unwrapEnvelope } from '@/api/unwrapEnvelope'
import { studentHttpClient } from '@/api/studentHttpClient'

function unwrapStudentAuthData(response) {
  return unwrapEnvelope(response).data
}

export const studentAuthApi = {
  register: (payload) => studentHttpClient.post('/student/api/auth/register', payload).then(unwrapStudentAuthData),
  login: (payload) => studentHttpClient.post('/student/api/auth/login', payload).then(unwrapStudentAuthData),
  me: () => studentHttpClient.get('/student/api/auth/me').then(unwrapStudentAuthData),
}
