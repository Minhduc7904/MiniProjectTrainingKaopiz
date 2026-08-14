export const GET_NOTIFICATION_BATCH_DEFAULT_QUERY = { batchId: '' }

export const GET_NOTIFICATION_BATCH_INPUT_FIELDS = [
  { key: 'batchId', label: 'Batch ID', type: 'uuid', required: true, nullable: false, defaultValue: null, allowlist: null, hint: 'UUID từ response 202 hoặc Location của batch.' },
]
