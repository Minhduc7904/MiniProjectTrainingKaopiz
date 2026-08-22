import { File } from 'lucide-react'
import { Icon } from '@/components/ui/admin/Icon'
import { MediaImagePreview } from '@/pages/media/components/MediaImagePreview'
import {
  getMediaLibraryThumbnail,
  getThumbnailStatusLabel,
  hasReadyMediaLibraryThumbnail,
} from '@/components/media/mediaLibraryThumbnail'
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
  const thumbnail = getMediaLibraryThumbnail(media)

  if (hasReadyMediaLibraryThumbnail(media)) {
    return <MediaImagePreview contentUrl={thumbnail.contentUrl} alt="" />
  }

  return (
    <div className={`flex aspect-video flex-col items-center justify-center gap-1 rounded-md p-2 text-center ${adminUi.choiceIdle}`}>
      <Icon icon={MediaIcon || File} size={24} className={adminUi.caption} />
      {thumbnail ? <span className={`text-[10px] ${adminUi.caption}`}>{getThumbnailStatusLabel(thumbnail.status)}</span> : null}
    </div>
  )
}

export function MediaLibraryExplorerList({ items, selectedId, viewMode, onSelect }) {
  if (viewMode === 'list') {
    return (
      <div className="space-y-2 px-4 pb-4">
        {items.map((media) => {
          const selected = media.id === selectedId

          return (
            <button
              key={media.id}
              type="button"
              aria-pressed={selected}
              onClick={() => onSelect(media.id)}
              className={`flex w-full cursor-pointer items-center gap-3 rounded-md p-3 text-left ${selected ? adminUi.choiceActive : adminUi.choiceIdle}`}
            >
              <span className="block w-16 shrink-0"><MediaThumb media={media} /></span>
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
