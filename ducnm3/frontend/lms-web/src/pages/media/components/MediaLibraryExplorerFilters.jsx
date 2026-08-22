import { FileText, Image, Music2, Video } from 'lucide-react'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { Icon } from '@/components/ui/admin/Icon'
import {
  MEDIA_STATUSES,
  MEDIA_STATUS_LABELS,
  MEDIA_TYPES,
} from '@/constants/media'
import { adminUi } from '@/theme/admin'

const TYPE_OPTIONS = [
  { value: '', label: 'Tất cả' },
  { value: MEDIA_TYPES.image, label: 'Ảnh', icon: Image },
  { value: MEDIA_TYPES.video, label: 'Video', icon: Video },
  { value: MEDIA_TYPES.document, label: 'Tài liệu', icon: FileText },
  { value: MEDIA_TYPES.audio, label: 'Audio', icon: Music2 },
]

const STATUS_OPTIONS = [
  { value: '', label: 'Tất cả trạng thái' },
  ...Object.values(MEDIA_STATUSES).map((status) => ({
    value: status,
    label: MEDIA_STATUS_LABELS[status],
  })),
]

export function MediaLibraryExplorerFilters({ query, disabled, onChange }) {
  return (
    <div className="px-4 py-4">
      <p className={`text-[11px] font-semibold uppercase tracking-[0.12em] ${adminUi.eyebrow}`}>
        Loại media
      </p>
      <div className="mt-2 grid grid-cols-2 gap-2">
        {TYPE_OPTIONS.map((option) => {
          const active = query.mediaType === option.value

          return (
            <button
              key={option.value || 'all'}
              type="button"
              disabled={disabled}
              onClick={() => onChange({ mediaType: option.value })}
              className={`flex cursor-pointer items-center gap-2 rounded-md px-3 py-2 text-left text-[13px] disabled:cursor-not-allowed ${active ? adminUi.choiceActive : adminUi.choiceIdle}`}
            >
              {option.icon ? <Icon icon={option.icon} size={15} /> : null}
              <span>{option.label}</span>
            </button>
          )
        })}
      </div>

      <div className={`mt-4 pt-4 ${adminUi.hairlineT}`}>
        <Dropdown
          id="media-library-status"
          label="Trạng thái"
          value={query.status}
          options={STATUS_OPTIONS}
          disabled={disabled}
          onChange={(status) => onChange({ status })}
        />
      </div>
    </div>
  )
}
