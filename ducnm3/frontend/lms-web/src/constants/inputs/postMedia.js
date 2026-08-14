import {
  ACTOR_TYPES,
  MEDIA_LIMITS_MIB,
  MEDIA_TYPES,
  POST_MEDIA_FIELDS,
} from '@/constants/media'
import { MEDIA_COPY } from '@/constants/mediaCopy'

export const POST_MEDIA_DEFAULT_QUERY = {
  [POST_MEDIA_FIELDS.mediaType]: MEDIA_TYPES.image,
  [POST_MEDIA_FIELDS.uploadedByType]: ACTOR_TYPES.student,
  [POST_MEDIA_FIELDS.uploadedBy]: '',
}

export const POST_MEDIA_INPUT_FIELDS = [
  {
    key: POST_MEDIA_FIELDS.file,
    label: MEDIA_COPY.file,
    type: 'file',
    required: true,
    nullable: false,
    defaultValue: null,
    allowlist: null,
    hint: 'Không rỗng, có tên và phần mở rộng. MIME phải khớp mediaType.',
  },
  {
    key: POST_MEDIA_FIELDS.mediaType,
    label: MEDIA_COPY.mediaType,
    type: 'string',
    required: true,
    nullable: false,
    defaultValue: MEDIA_TYPES.image,
    allowlist: Object.values(MEDIA_TYPES),
    hint: `Không phân biệt hoa/thường. Giới hạn MiB: IMAGE ${MEDIA_LIMITS_MIB.IMAGE}, VIDEO ${MEDIA_LIMITS_MIB.VIDEO}, DOCUMENT ${MEDIA_LIMITS_MIB.DOCUMENT}, AUDIO ${MEDIA_LIMITS_MIB.AUDIO}, OTHER ${MEDIA_LIMITS_MIB.OTHER}.`,
  },
  {
    key: POST_MEDIA_FIELDS.uploadedByType,
    label: MEDIA_COPY.uploadedByType,
    type: 'string',
    required: true,
    nullable: false,
    defaultValue: ACTOR_TYPES.student,
    allowlist: Object.values(ACTOR_TYPES),
    hint: 'Hiện chỉ STUDENT. Sai type → 400 INVALID_ACTOR_TYPE.',
  },
  {
    key: POST_MEDIA_FIELDS.uploadedBy,
    label: MEDIA_COPY.uploadedBy,
    type: 'uuid',
    required: true,
    nullable: false,
    defaultValue: null,
    allowlist: null,
    hint: 'UUID Học viên tồn tại trên Student Service. Không có → 404 ACTOR_NOT_FOUND.',
  },
]
