import axios from 'axios'
import { ENV } from '@/constants/env'
import { HTTP_CONTENT_TYPES } from '@/constants/http'

export const httpClient = axios.create({
  baseURL: ENV.apiBaseUrl,
  timeout: ENV.apiTimeoutMs,
  headers: {
    Accept: HTTP_CONTENT_TYPES.json,
  },
})
