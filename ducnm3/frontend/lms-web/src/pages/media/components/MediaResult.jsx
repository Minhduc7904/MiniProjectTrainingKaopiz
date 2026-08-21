import { MEDIA_COPY } from '@/constants/mediaCopy'
import { isImageMedia } from '@/hooks/media/mediaThumbnail'
import { adminUi } from '@/theme/admin'
import { MediaImagePreview } from './MediaImagePreview'
import { MediaThumbnailStatus } from './MediaThumbnailStatus'

function Row({ label, value }) {
  return (
    <div className={`grid grid-cols-[140px_1fr] gap-3 px-4 py-3 ${adminUi.tableRow}`}>
      <dt className={`font-mono text-[12px] ${adminUi.caption}`}>{label}</dt>
      <dd className={`break-all text-[14px] ${adminUi.tableCellStrong}`}>
        {value == null || value === '' ? '—' : String(value)}
      </dd>
    </div>
  )
}

export function MediaResult({ media, location, showDraft = false }) {
  return (
    <div className="space-y-4">
      {isImageMedia(media) ? <MediaImagePreview contentUrl={media.contentUrl} alt={media.id} /> : null}
      <div className={`overflow-hidden rounded-lg ${adminUi.card}`}>
        <p className={`px-4 py-3 text-[12px] font-medium tracking-wide uppercase ${adminUi.tableHead}`}>
          {MEDIA_COPY.result}
        </p>
        <dl>
          <Row label="id" value={media.id} />
          <Row label="mediaType" value={media.mediaType} />
          <Row label="contentType" value={media.contentType} />
          <Row label="sizeBytes" value={media.sizeBytes} />
          <Row label="status" value={media.status} />
          {showDraft ? <Row label="isDraft" value={media.isDraft} /> : null}
          {showDraft ? <Row label="draftedAt" value={media.draftedAt} /> : null}
          <Row label={MEDIA_COPY.contentUrl} value={media.contentUrl} />
          <Row label={MEDIA_COPY.thumbnailStatus} value={media.thumbnailStatus} />
          <Row label={MEDIA_COPY.location} value={location} />
        </dl>
      </div>
      <MediaThumbnailStatus media={media} />
    </div>
  )
}
