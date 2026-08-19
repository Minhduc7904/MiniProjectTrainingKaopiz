import { PAGINATION } from '@/constants/pagination'
import { QUERY_PARAMS, SORT_DIRECTIONS } from '@/constants/queryParams'
import { COURSE_STATUS } from '@/constants/courseStatus'

export const COURSE_SORT_BY = { createdAt: 'createdAt', name: 'name' }
export const GET_COURSES_DEFAULT_QUERY = { sortBy: COURSE_SORT_BY.createdAt, sortDirection: SORT_DIRECTIONS.desc, page: PAGINATION.defaults.page, pageSize: PAGINATION.defaults.pageSize }
export const GET_COURSES_INPUT_FIELDS = [
  { key: QUERY_PARAMS.status, label: 'Trạng thái', type: 'string', required: false, nullable: true, defaultValue: null, allowlist: Object.values(COURSE_STATUS), hint: 'DRAFT, PUBLISHED hoặc ARCHIVED.' },
  { key: QUERY_PARAMS.sortBy, label: 'Sắp xếp theo', type: 'string', required: false, nullable: false, defaultValue: COURSE_SORT_BY.createdAt, allowlist: Object.values(COURSE_SORT_BY), hint: 'createdAt hoặc name.' },
  { key: QUERY_PARAMS.sortDirection, label: 'Thứ tự', type: 'string', required: false, nullable: false, defaultValue: SORT_DIRECTIONS.desc, allowlist: Object.values(SORT_DIRECTIONS), hint: 'asc hoặc desc.' },
  { key: QUERY_PARAMS.page, label: 'Trang', type: 'integer', required: false, nullable: false, defaultValue: 1, allowlist: null, hint: 'Từ 1.' },
  { key: QUERY_PARAMS.pageSize, label: 'Số dòng/trang', type: 'integer', required: false, nullable: false, defaultValue: PAGINATION.defaults.pageSize, allowlist: PAGINATION.pageSizeOptions, hint: 'Từ 1 đến 100.' },
]
