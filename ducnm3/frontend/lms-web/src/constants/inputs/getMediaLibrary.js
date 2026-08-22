import { MEDIA_STATUSES, MEDIA_TYPES } from '@/constants/media'
import { QUERY_PARAMS } from '@/constants/queryParams'

export const GET_MEDIA_LIBRARY_DEFAULT_QUERY = {
  mediaType: '',
  status: '',
}

export const GET_MEDIA_LIBRARY_INPUT_FIELDS = [
  {
    key: QUERY_PARAMS.mediaType,
    label: 'Loại media',
    type: 'string',
    required: false,
    nullable: true,
    defaultValue: null,
    allowlist: Object.values(MEDIA_TYPES),
    hint: 'Lọc IMAGE, VIDEO, DOCUMENT, AUDIO hoặc OTHER.',
  },
  {
    key: QUERY_PARAMS.status,
    label: 'Trạng thái',
    type: 'string',
    required: false,
    nullable: true,
    defaultValue: null,
    allowlist: Object.values(MEDIA_STATUSES),
    hint: 'Lọc PENDING, READY hoặc FAILED.',
  },
]
