import { File } from 'lucide-react'
import { Icon } from '@/components/ui/admin/Icon'
import { MediaImagePreview } from '@/pages/media/components/MediaImagePreview'
import { getMediaTypeIcon, getMediaTypeLabel } from '@/components/media/mediaPresentation'
import { MEDIA_STATUS_LABELS } from '@/constants/media'
import { adminUi } from '@/theme/admin'

function statusTone(status) {
  if (status === 'READY') return adminUi.badgeSuccess
  if (status === 'PENDING') return adminUi.badgeWarning
  if (status === 'FAILED') return adminUi.badgeDanger
  return adminUi.badgeMuted
}

function MediaThumb({ media }) {
  const MediaIcon = getMediaTypeIcon(media.mediaType)

  if (media.mediaType === 'IMAGE' && media.status === 'READY') {
    return <MediaImagePreview contentUrl={media.thumbnailUrl || media.contentUrl} alt="" />
  }

  return (
    <div className={`flex aspect-video items-center justify-center rounded-md ${adminUi.choiceIdle}`}>
      <Icon icon={MediaIcon || File} size={28} className={adminUi.caption} />
    </div>
  )
}

export function MediaLibraryExplorerList({ items, selectedId, viewMode, onSelect }) {
  if (viewMode === 'list') {
    return (
      <div className="space-y-2 px-4 pb-4">
        {items.map((media) => {
          const MediaIcon = getMediaTypeIcon(media.mediaType)
          const selected = media.id === selectedId

          return (
            <button
              key={media.id}
              type="button"
              aria-pressed={selected}
              onClick={() => onSelect(media.id)}
              className={`flex w-full cursor-pointer items-center gap-3 rounded-md p-3 text-left ${selected ? adminUi.choiceActive : adminUi.choiceIdle}`}
            >
              <span className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-md ${adminUi.badgeMuted}`}>
                <Icon icon={MediaIcon} size={18} />
              </span>
              <span className="min-w-0 flex-1">
                <span className={`block truncate text-[13px] font-medium ${adminUi.title}`}>
                  {media.originalFileName || 'media'}
                </span>
                <span className={`mt-1 block text-[11px] ${adminUi.caption}`}>
                  {getMediaTypeLabel(media.mediaType)}
                </span>
              </span>
              <span className={`shrink-0 rounded-full px-2 py-1 text-[10px] ${statusTone(media.status)}`}>
                {MEDIA_STATUS_LABELS[media.status] || media.status}
              </span>
            </button>
          )
        })}
      </div>
    )
  }

  return (
    <div className="grid grid-cols-2 gap-3 px-4 pb-4 sm:grid-cols-3">
      {items.map((media) => {
        const selected = media.id === selectedId

        return (
          <button
            key={media.id}
            type="button"
            aria-pressed={selected}
            onClick={() => onSelect(media.id)}
            className={`group cursor-pointer overflow-hidden rounded-md p-2 text-left ${selected ? adminUi.choiceActive : adminUi.choiceIdle}`}
          >
            <MediaThumb media={media} />
            <span className={`mt-2 block truncate text-[12px] font-medium ${adminUi.title}`}>
              {media.originalFileName || 'media'}
            </span>
            <span className={`mt-1 inline-flex rounded-full px-1.5 py-0.5 text-[10px] ${statusTone(media.status)}`}>
              {MEDIA_STATUS_LABELS[media.status] || media.status}
            </span>
          </button>
        )
      })}
    </div>
  )
}
