import { MEDIA_COPY } from '@/constants/mediaCopy'
import { ui } from '@/theme'

function Row({ label, value }) {
  return (
    <div className={`grid grid-cols-[140px_1fr] gap-3 px-4 py-3 ${ui.tableRow}`}>
      <dt className={`font-mono text-[12px] ${ui.caption}`}>{label}</dt>
      <dd className={`break-all text-[14px] ${ui.tableCellStrong}`}>
        {value == null || value === '' ? '—' : String(value)}
      </dd>
    </div>
  )
}

export function MediaResult({ media, location }) {
  return (
    <div className={`overflow-hidden rounded-lg ${ui.card}`}>
      <p className={`px-4 py-3 text-[12px] font-medium tracking-wide uppercase ${ui.tableHead}`}>
        {MEDIA_COPY.result}
      </p>
      <dl>
        <Row label="id" value={media.id} />
        <Row label="mediaType" value={media.mediaType} />
        <Row label="contentType" value={media.contentType} />
        <Row label="sizeBytes" value={media.sizeBytes} />
        <Row label="status" value={media.status} />
        <Row label={MEDIA_COPY.contentUrl} value={media.contentUrl} />
        <Row label={MEDIA_COPY.thumbnailStatus} value={media.thumbnailStatus} />
        <Row label="thumbnailMediaId" value={media.thumbnailMediaId} />
        <Row label="thumbnailJobId" value={media.thumbnailJobId} />
        <Row label="thumbnailStatusUrl" value={media.thumbnailStatusUrl} />
        <Row label={MEDIA_COPY.location} value={location} />
      </dl>
    </div>
  )
}
