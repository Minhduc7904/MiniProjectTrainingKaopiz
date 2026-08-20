import { CircleAlert, LoaderCircle, RefreshCw } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Icon } from '@/components/ui/Icon'
import { MEDIA_COPY } from '@/constants/mediaCopy'
import { useMediaThumbnail } from '@/hooks/media/mediaThumbnail'
import { ui } from '@/theme'
import { MediaImagePreview } from './MediaImagePreview'

export function MediaThumbnailStatus({ media }) {
  const { thumbnail, error, retrying, retry } = useMediaThumbnail(media)
  const active = thumbnail.status === 'QUEUED' || thumbnail.status === 'PROCESSING'
  const failed = thumbnail.status === 'FAILED'

  if (!thumbnail.status || thumbnail.status === 'NOT_REQUIRED') return null

  return (
    <section className={`overflow-hidden rounded-lg ${ui.card}`} aria-live="polite">
      <div className={`flex items-center justify-between gap-3 px-4 py-3 ${ui.tableHead}`}>
        <p className="text-[12px] font-medium tracking-wide uppercase">{MEDIA_COPY.thumbnail}</p>
        <span className={`text-[12px] font-medium ${failed ? ui.dangerText : ui.accentText}`}>
          {thumbnail.status}
        </span>
      </div>
      <div className="space-y-3 p-4">
        {active ? (
          <div className={`flex items-center gap-2 text-[13px] ${ui.body}`}>
            <Icon icon={LoaderCircle} className="animate-spin" />
            {MEDIA_COPY.thumbnailProcessing}
          </div>
        ) : null}
        {thumbnail.thumbnailContentUrl ? (
          <MediaImagePreview contentUrl={thumbnail.thumbnailContentUrl} alt={MEDIA_COPY.thumbnail} />
        ) : null}
        {failed ? (
          <div className={`flex flex-wrap items-center justify-between gap-3 rounded-md p-3 ${ui.badgeDanger}`}>
            <span className="flex items-center gap-2 text-[13px]">
              <Icon icon={CircleAlert} />
              {thumbnail.lastError || MEDIA_COPY.thumbnailFailed}
            </span>
            <Button disabled={retrying} variant="ghost" onClick={retry}>
              <Icon icon={RefreshCw} className={retrying ? 'animate-spin' : undefined} />
              {MEDIA_COPY.retry}
            </Button>
          </div>
        ) : null}
        {error ? <p className={`text-[13px] ${ui.dangerText}`}>{error.message}</p> : null}
      </div>
    </section>
  )
}
