import { PAGINATION } from '@/constants/pagination'
import { QUERY_PARAMS, SORT_DIRECTIONS, STUDENT_SORT_BY } from '@/constants/queryParams'
import { STUDENT_STATUS } from '@/constants/studentStatus'
import { STUDENT_COPY } from '@/constants/studentCopy'
import { UI_LABELS } from '@/constants/ui'

export const GET_STUDENTS_DEFAULT_QUERY = {
  sortBy: STUDENT_SORT_BY.createdAt,
  sortDirection: SORT_DIRECTIONS.desc,
  page: PAGINATION.defaults.page,
  pageSize: PAGINATION.defaults.pageSize,
}

export const GET_STUDENTS_INPUT_FIELDS = [
  {
    key: QUERY_PARAMS.search,
    label: STUDENT_COPY.search,
    type: 'string',
    required: false,
    nullable: true,
    defaultValue: null,
    allowlist: null,
    hint: 'Tìm một phần tên hiển thị hoặc email; bỏ trống để không lọc.',
  },
  {
    key: QUERY_PARAMS.status,
    label: STUDENT_COPY.status,
    type: 'string',
    required: false,
    nullable: true,
    defaultValue: null,
    allowlist: Object.values(STUDENT_STATUS),
    hint: 'Chuỗi rỗng được xem như không truyền. Không lọc khi null.',
  },
  {
    key: QUERY_PARAMS.sortBy,
    label: STUDENT_COPY.sortBy,
    type: 'string',
    required: false,
    nullable: false,
    defaultValue: STUDENT_SORT_BY.createdAt,
    allowlist: Object.values(STUDENT_SORT_BY),
    hint: 'Bỏ trống thì server dùng mặc định. Gửi rỗng hoặc giá trị lạ sẽ 400.',
  },
  {
    key: QUERY_PARAMS.sortDirection,
    label: STUDENT_COPY.direction,
    type: 'string',
    required: false,
    nullable: false,
    defaultValue: SORT_DIRECTIONS.desc,
    allowlist: Object.values(SORT_DIRECTIONS),
    hint: 'asc hoặc desc. Không phân biệt hoa/thường.',
  },
  {
    key: QUERY_PARAMS.page,
    label: UI_LABELS.page,
    type: 'integer',
    required: false,
    nullable: false,
    defaultValue: PAGINATION.defaults.page,
    allowlist: null,
    hint: `Bắt đầu từ ${PAGINATION.limits.pageMin}. Không null.`,
  },
  {
    key: QUERY_PARAMS.pageSize,
    label: UI_LABELS.pageSize,
    type: 'integer',
    required: false,
    nullable: false,
    defaultValue: PAGINATION.defaults.pageSize,
    allowlist: PAGINATION.pageSizeOptions,
    hint: `Từ ${PAGINATION.limits.pageSizeMin} đến ${PAGINATION.limits.pageSizeMax}. Không null.`,
  },
]
