import { useEffect, useRef } from 'react'
import { createPortal } from 'react-dom'
import { Download, FileText, X } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Icon } from '@/components/ui/Icon'
import { MediaImagePreview } from '@/pages/media/components/MediaImagePreview'
import { getMediaTypeIcon, getMediaTypeLabel } from '@/components/media/mediaPresentation'
import { getMediaPreviewKind, MEDIA_PREVIEW_KIND } from '@/components/media/mediaPreviewKind'
import { resolveMediaContentUrl } from '@/components/media/mediaContentUrl'
import { ui } from '@/theme'

export function MediaPreviewModal({ open, media, onClose }) {
  const dialogRef = useRef(null)
  const previewKind = getMediaPreviewKind(media)

  useEffect(() => {
    if (!open) return undefined
    const dialog = dialogRef.current
    if (!dialog) return undefined
    dialog.showModal()
    return () => dialog.close()
  }, [open])

  if (!open || !media) return null

  const close = () => onClose()
  const MediaIcon = getMediaTypeIcon(media.mediaType)
  const fileName = media.originalFileName || 'media'
  const contentUrl = resolveMediaContentUrl(media.contentUrl)

  return createPortal(
    <dialog
      ref={dialogRef}
      className={`${ui.modal} flex max-h-[calc(100vh-2rem)] w-[min(960px,calc(100vw-2rem))] max-w-none flex-col overflow-hidden`}
      aria-labelledby="media-preview-title"
      onClick={(event) => { if (event.target === event.currentTarget) close() }}
      onCancel={(event) => { event.preventDefault(); close() }}
    >
      <header className={`flex items-start justify-between gap-4 px-5 py-4 ${ui.hairlineB}`}>
        <div className="min-w-0">
          <p id="media-preview-title" className={`truncate font-display text-[18px] font-semibold ${ui.title}`}>{fileName}</p>
          <p className={`mt-1 flex items-center gap-1.5 text-[12px] ${ui.body}`}><Icon icon={MediaIcon} size={14} />{getMediaTypeLabel(media.mediaType)}{media.contentType ? ` · ${media.contentType}` : ''}</p>
        </div>
        <Button type="button" variant="ghost" size="icon" aria-label="Đóng xem trước media" onClick={close}><Icon icon={X} /></Button>
      </header>

      <section className="min-h-0 flex-1 overflow-auto p-5">
        {previewKind === MEDIA_PREVIEW_KIND.image ? <MediaImagePreview contentUrl={media.contentUrl} alt={fileName} /> : null}
        {previewKind === MEDIA_PREVIEW_KIND.video ? <video className={`max-h-[68vh] w-full ${ui.mediaStage}`} controls preload="metadata"><source src={contentUrl} type={media.contentType} />Trình duyệt không hỗ trợ phát video.</video> : null}
        {previewKind === MEDIA_PREVIEW_KIND.audio ? <div className={`rounded-md p-5 ${ui.choiceIdle}`}><audio className="w-full" controls preload="metadata"><source src={contentUrl} type={media.contentType} />Trình duyệt không hỗ trợ phát audio.</audio></div> : null}
        {previewKind === MEDIA_PREVIEW_KIND.pdf ? <iframe title={`Xem trước ${fileName}`} src={contentUrl} className={`h-[68vh] min-h-[360px] w-full rounded-md ${ui.choiceIdle}`} /> : null}
        {previewKind === MEDIA_PREVIEW_KIND.download ? <div className={`flex min-h-64 flex-col items-center justify-center rounded-md p-6 text-center ${ui.choiceIdle}`}><Icon icon={FileText} size={44} className={ui.caption} /><p className={`mt-4 text-[14px] font-medium ${ui.title}`}>Không hỗ trợ xem trước loại tệp này</p><p className={`mt-1 text-[12px] ${ui.body}`}>{media.contentType || 'Không xác định MIME type'}</p><a className="mt-5" href={contentUrl} download={fileName}><Button><Icon icon={Download} size={16} />Tải file</Button></a></div> : null}
      </section>
    </dialog>,
    document.body,
  )
}
