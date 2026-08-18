export const API_ERROR_CODES = {
  databaseUnavailable: 'DATABASE_UNAVAILABLE',
  storageUnavailable: 'STORAGE_UNAVAILABLE',
  dependencyUnavailable: 'DEPENDENCY_UNAVAILABLE',
  serviceUnavailable: 'SERVICE_UNAVAILABLE',
  unexpectedError: 'UNEXPECTED_ERROR',
  validationFailed: 'VALIDATION_FAILED',
  payloadTooLarge: 'PAYLOAD_TOO_LARGE',
  invalidMedia: 'INVALID_MEDIA',
  invalidActorType: 'INVALID_ACTOR_TYPE',
  actorNotFound: 'ACTOR_NOT_FOUND',
  unsupportedMediaType: 'UNSUPPORTED_MEDIA_TYPE',
  studentServiceUnavailable: 'STUDENT_SERVICE_UNAVAILABLE',
  mediaUploadFailed: 'MEDIA_UPLOAD_FAILED',
  networkError: 'NETWORK_ERROR',
  requestTimeout: 'REQUEST_TIMEOUT',
  directUploadFailed: 'DIRECT_UPLOAD_FAILED',
  uploadCanceled: 'UPLOAD_CANCELED',
  checksumFailed: 'CHECKSUM_FAILED',
}

export const AXIOS_ERROR_CODES = {
  network: 'ERR_NETWORK',
  timeout: 'ECONNABORTED',
  canceled: 'ERR_CANCELED',
}
