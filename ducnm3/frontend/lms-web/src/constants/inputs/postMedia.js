import { POST_MEDIA_FIELDS } from '@/constants/media'
import { MEDIA_COPY } from '@/constants/mediaCopy'

export const POST_MEDIA_DEFAULT_QUERY = {}

export const POST_MEDIA_INPUT_FIELDS = [
  {
    key: POST_MEDIA_FIELDS.file,
    label: MEDIA_COPY.file,
    type: 'file',
    required: true,
    nullable: false,
    defaultValue: null,
    allowlist: null,
    hint: 'BE sẽ phân loại media theo MIME và chọn bucket phù hợp.',
  },
]
