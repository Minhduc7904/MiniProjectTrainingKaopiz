import { PAGINATION } from '@/constants/pagination'
import { MEDIA_JOB_STATUS, MEDIA_JOB_TYPE, MEDIA_JOB_COPY } from '@/constants/mediaJobs'
import { QUERY_PARAMS } from '@/constants/queryParams'

export const GET_MEDIA_JOBS_DEFAULT_QUERY = {
  page: PAGINATION.defaults.page,
  pageSize: PAGINATION.defaults.pageSize,
  jobType: '',
  status: '',
  correlationId: '',
}

export const GET_MEDIA_JOBS_INPUT_FIELDS = [
  { key: QUERY_PARAMS.jobType, label: MEDIA_JOB_COPY.jobType, type: 'string', required: false, nullable: true, defaultValue: null, allowlist: Object.values(MEDIA_JOB_TYPE), hint: 'Lọc theo loại job nền.' },
  { key: QUERY_PARAMS.status, label: MEDIA_JOB_COPY.status, type: 'string', required: false, nullable: true, defaultValue: null, allowlist: Object.values(MEDIA_JOB_STATUS), hint: 'Lọc theo trạng thái vận hành.' },
  { key: QUERY_PARAMS.correlationId, label: MEDIA_JOB_COPY.correlationId, type: 'string', required: false, nullable: true, defaultValue: null, allowlist: null, hint: 'UUID correlation, ví dụ Notification Batch ID.' },
  { key: QUERY_PARAMS.page, label: 'Trang', type: 'integer', required: false, nullable: false, defaultValue: PAGINATION.defaults.page, allowlist: null, hint: 'Từ 1.' },
  { key: QUERY_PARAMS.pageSize, label: 'Số job mỗi trang', type: 'integer', required: false, nullable: false, defaultValue: PAGINATION.defaults.pageSize, allowlist: PAGINATION.pageSizeOptions, hint: 'Từ 1 đến 100.' },
]
