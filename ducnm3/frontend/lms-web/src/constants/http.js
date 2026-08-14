export const HTTP_HEADERS = {
  correlationId: 'X-Correlation-Id',
  contentType: 'Content-Type',
  location: 'location',
}

export const HTTP_CONTENT_TYPES = {
  json: 'application/json',
  multipart: 'multipart/form-data',
}

export const HTTP_STATUS = {
  ok: 200,
  created: 201,
  badRequest: 400,
  unauthorized: 401,
  forbidden: 403,
  notFound: 404,
  conflict: 409,
  payloadTooLarge: 413,
  unsupportedMediaType: 415,
  unprocessable: 422,
  serviceUnavailable: 503,
}

export const HTTP_SUCCESS_CODES = {
  [HTTP_STATUS.ok]: 'OK',
  [HTTP_STATUS.created]: 'CREATED',
}
