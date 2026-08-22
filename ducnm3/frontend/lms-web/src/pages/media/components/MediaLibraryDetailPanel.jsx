import { Download, FileText, ShieldAlert } from 'lucide-react'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { Icon } from '@/components/ui/admin/Icon'
import { MediaImagePreview } from '@/pages/media/components/MediaImagePreview'
import { getMediaPreviewKind, MEDIA_PREVIEW_KIND } from '@/components/media/mediaPreviewKind'
import { getMediaTypeIcon, getMediaTypeLabel } from '@/components/media/mediaPresentation'
import { resolveMediaContentUrl } from '@/components/media/mediaContentUrl'
import {
  getMediaLibraryThumbnail,
  getThumbnailStatusLabel,
} from '@/components/media/mediaLibraryThumbnail'
import { MEDIA_STATUS_LABELS } from '@/constants/media'
import { adminUi } from '@/theme/admin'

function formatDate(value) {
  return value
    ? new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))
    : '—'
}

function formatBytes(value) {
  if (value == null) return '—'
  if (value < 1024) return `${value} B`
  if (value < 1024 * 1024) return `${(value / 1024).toFixed(1)} KB`
  return `${(value / (1024 * 1024)).toFixed(1)} MB`
}

function statusTone(status) {
  if (status === 'READY') return adminUi.badgeSuccess
  if (status === 'PENDING') return adminUi.badgeWarning
  if (status === 'FAILED') return adminUi.badgeDanger
  return adminUi.badgeMuted
}

function MetadataList({ rows }) {
  return (
    <dl className={`overflow-hidden rounded-lg ${adminUi.card}`}>
      {rows.map(([label, value], index) => (
        <div key={label} className={`flex items-start justify-between gap-4 px-4 py-3 ${index ? adminUi.hairlineT : ''}`}>
          <dt className={`shrink-0 text-[12px] ${adminUi.caption}`}>{label}</dt>
          <dd className={`min-w-0 break-all text-right text-[12px] ${label.endsWith('ID') ? `font-mono ${adminUi.body}` : adminUi.title}`}>{value}</dd>
        </div>
      ))}
    </dl>
  )
}

function MediaPreview({ media, fileName }) {
  if (media.status !== 'READY') {
    return (
      <div className={`flex min-h-64 flex-col items-center justify-center rounded-lg p-6 text-center ${adminUi.choiceIdle}`}>
        <Icon icon={ShieldAlert} size={36} className={adminUi.caption} />
        <p className={`mt-3 text-[14px] font-medium ${adminUi.title}`}>Chưa thể xem nội dung</p>
        <p className={`mt-1 text-[12px] ${adminUi.body}`}>Tệp đang ở trạng thái {MEDIA_STATUS_LABELS[media.status] || media.status}.</p>
      </div>
    )
  }

  const previewKind = getMediaPreviewKind(media)
  const contentUrl = resolveMediaContentUrl(media.contentUrl)

  if (previewKind === MEDIA_PREVIEW_KIND.image) {
    return <MediaImagePreview contentUrl={media.contentUrl} alt={fileName} />
  }

  if (previewKind === MEDIA_PREVIEW_KIND.video) {
    return <video className={`max-h-[52vh] min-h-48 w-full rounded-lg ${adminUi.choiceIdle}`} controls preload="metadata"><source src={contentUrl} type={media.contentType} />Trình duyệt không hỗ trợ phát video.</video>
  }

  if (previewKind === MEDIA_PREVIEW_KIND.audio) {
    return <div className={`rounded-lg p-5 ${adminUi.choiceIdle}`}><audio className="w-full" controls preload="metadata"><source src={contentUrl} type={media.contentType} />Trình duyệt không hỗ trợ phát audio.</audio></div>
  }

  if (previewKind === MEDIA_PREVIEW_KIND.pdf) {
    return <iframe title={`Xem trước ${fileName}`} src={contentUrl} className={`h-[52vh] min-h-72 w-full rounded-lg ${adminUi.choiceIdle}`} />
  }

  return (
    <div className={`flex min-h-64 flex-col items-center justify-center rounded-lg p-6 text-center ${adminUi.choiceIdle}`}>
      <Icon icon={FileText} size={40} className={adminUi.caption} />
      <p className={`mt-3 text-[14px] font-medium ${adminUi.title}`}>Không hỗ trợ xem trước loại tệp này</p>
      <a className={`mt-4 inline-flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-2 text-[13px] font-medium ${adminUi.buttonGhost}`} href={contentUrl} download={fileName}>
        <Icon icon={Download} size={16} />Tải file
      </a>
    </div>
  )
}

export function MediaLibraryDetailPanel({ media }) {
  if (!media) {
    return (
      <aside className={`min-h-0 overflow-y-auto p-5 ${adminUi.panel}`}>
        <EmptyState title="Chọn một media" description="Chọn tệp từ danh sách bên trái để xem trước và kiểm tra thông tin." />
      </aside>
    )
  }

  const MediaIcon = getMediaTypeIcon(media.mediaType)
  const fileName = media.originalFileName || 'media'
  const thumbnail = getMediaLibraryThumbnail(media)

  return (
    <aside className={`min-h-0 overflow-y-auto ${adminUi.panel}`}>
      <header className={`px-5 py-4 ${adminUi.hairlineB}`}>
        <div className="flex items-start gap-3">
          <span className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-md ${adminUi.badgeMuted}`}><Icon icon={MediaIcon} size={20} /></span>
          <div className="min-w-0">
            <h1 className={`truncate font-display text-[18px] font-semibold ${adminUi.title}`}>{fileName}</h1>
            <p className={`mt-1 text-[12px] ${adminUi.body}`}>{getMediaTypeLabel(media.mediaType)}{media.contentType ? ` · ${media.contentType}` : ''}</p>
          </div>
        </div>
        <span className={`mt-3 inline-flex rounded-full px-2 py-1 text-[11px] ${statusTone(media.status)}`}>
          {MEDIA_STATUS_LABELS[media.status] || media.status}
        </span>
      </header>

      <div className="space-y-5 p-5">
        <MediaPreview media={media} fileName={fileName} />
        <section>
          <h2 className={`mb-2 text-[13px] font-semibold ${adminUi.title}`}>Thông tin file gốc</h2>
          <MetadataList rows={[
            ['Media ID', media.id],
            ['Loại media', getMediaTypeLabel(media.mediaType)],
            ['MIME type', media.contentType || '—'],
            ['Dung lượng', formatBytes(media.sizeBytes)],
            ['Khởi tạo', formatDate(media.createdAtUtc)],
            ['Hoàn tất', formatDate(media.completedAtUtc)],
          ]} />
        </section>
        <section>
          <h2 className={`mb-2 text-[13px] font-semibold ${adminUi.title}`}>Thông tin thumbnail</h2>
          <MetadataList rows={thumbnail ? [
            ['Thumbnail ID', thumbnail.id],
            ['Trạng thái', getThumbnailStatusLabel(thumbnail.status)],
            ['MIME type', thumbnail.contentType || '—'],
            ['Dung lượng', formatBytes(thumbnail.sizeBytes)],
            ['Khởi tạo', formatDate(thumbnail.createdAtUtc)],
            ['Hoàn tất', formatDate(thumbnail.completedAtUtc)],
          ] : [
            ['Trạng thái', getThumbnailStatusLabel('NOT_REQUIRED')],
          ]} />
        </section>
        <a className={`inline-flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-2 text-[13px] font-medium ${adminUi.buttonGhost}`} href={resolveMediaContentUrl(media.contentUrl)} download={fileName}>
          <Icon icon={Download} size={16} />Tải file gốc
        </a>
      </div>
    </aside>
  )
}
