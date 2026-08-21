import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { ImagePlus, Paperclip, X } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { FieldLabel, TextInput } from '@/components/ui/admin/Field'
import { MediaLibraryModal } from '@/components/media/MediaLibraryModal'
import { Icon } from '@/components/ui/admin/Icon'
import { adminUi } from '@/theme/admin'
import { preventParentBatchSubmit } from './notificationMarkdownSubmission'

const MEDIA_USAGE = {
  embed: 'EMBED',
  attachment: 'ATTACHMENT',
}

function markdownForMedia({ usage, label, contentUrl }) {
  const safeLabel = label.replaceAll(']', '')
  return usage === MEDIA_USAGE.embed
    ? `![${safeLabel}](${contentUrl})`
    : `[${safeLabel}](${contentUrl})`
}

function MediaInsertDialog({ open, disabled, onClose, onInsert }) {
  const dialogRef = useRef(null)
  const [usage, setUsage] = useState(MEDIA_USAGE.embed)
  const [label, setLabel] = useState('')
  const [selectedMedia, setSelectedMedia] = useState(null)
  const [isLibraryOpen, setIsLibraryOpen] = useState(false)

  useEffect(() => {
    if (!open) return undefined
    const dialog = dialogRef.current
    if (!dialog) return undefined
    dialog.showModal()
    return () => dialog.close()
  }, [open])

  const close = () => {
    setUsage(MEDIA_USAGE.embed)
    setLabel('')
    setSelectedMedia(null)
    setIsLibraryOpen(false)
    onClose()
  }

  const selectMedia = (mediaItems) => {
    const media = mediaItems[0]
    if (!media) return
    setSelectedMedia(media)
    setLabel((current) => current.trim() || media.originalFileName || '')
    setIsLibraryOpen(false)
  }

  const submit = (event) => {
    preventParentBatchSubmit(event)
    if (!selectedMedia?.contentUrl) return
    onInsert({
      usage,
      label: label.trim() || selectedMedia.originalFileName || selectedMedia.id,
      media: selectedMedia,
    })
    close()
  }

  if (!open) return null

  return createPortal(
    <>
      <dialog
        ref={dialogRef}
        aria-labelledby="media-insert-title"
        onClick={(event) => { if (event.target === event.currentTarget) close() }}
        onCancel={(event) => { event.preventDefault(); close() }}
        className={adminUi.modal}
      >
        <form onSubmit={submit} className="flex flex-col">
          <header className={`flex items-start justify-between gap-4 px-5 py-4 ${adminUi.hairlineB}`}>
            <div>
              <p className={`font-display text-[18px] font-semibold ${adminUi.title}`} id="media-insert-title">Thêm media vào Markdown</p>
              <p className={`mt-1 text-[13px] ${adminUi.body}`}>Chọn media trong thư viện, sau đó chọn cách hiển thị trong nội dung.</p>
            </div>
            <Button type="button" variant="ghost" size="icon" aria-label="Đóng modal" onClick={close}><Icon icon={X} /></Button>
          </header>

          <div className="flex flex-col gap-4 px-5 py-4">
            <div className={`flex items-center justify-between gap-3 rounded-md p-3 ${adminUi.choiceIdle}`}>
              <div className="min-w-0">
                <p className={`text-[13px] font-medium ${adminUi.title}`}>{selectedMedia?.originalFileName || 'Chưa chọn ảnh'}</p>
                <p className={`mt-1 text-[12px] ${adminUi.body}`}>{selectedMedia ? 'Đã chọn từ thư viện' : 'Mở thư viện để chọn một media'}</p>
              </div>
              <Button type="button" variant="ghost" onClick={() => setIsLibraryOpen(true)}>Chọn ảnh</Button>
            </div>

            <fieldset disabled={disabled}>
              <legend className={`font-display text-[11px] font-medium tracking-[0.18em] uppercase ${adminUi.eyebrow}`}>Cách dùng trong nội dung</legend>
              <div className="mt-2 grid grid-cols-2 gap-2">
                <label className={`cursor-pointer rounded-md p-3 ${usage === MEDIA_USAGE.embed ? adminUi.choiceActive : adminUi.choiceIdle}`}>
                  <input className="sr-only" type="radio" name="mediaUsage" checked={usage === MEDIA_USAGE.embed} onChange={() => setUsage(MEDIA_USAGE.embed)} />
                  <span className="flex items-center gap-2 text-[13px] font-medium"><Icon icon={ImagePlus} />Nhúng ảnh</span>
                  <span className={`mt-1 block text-[11px] ${adminUi.body}`}>`![alt](url)`</span>
                </label>
                <label className={`cursor-pointer rounded-md p-3 ${usage === MEDIA_USAGE.attachment ? adminUi.choiceActive : adminUi.choiceIdle}`}>
                  <input className="sr-only" type="radio" name="mediaUsage" checked={usage === MEDIA_USAGE.attachment} onChange={() => setUsage(MEDIA_USAGE.attachment)} />
                  <span className="flex items-center gap-2 text-[13px] font-medium"><Icon icon={Paperclip} />Attachment</span>
                  <span className={`mt-1 block text-[11px] ${adminUi.body}`}>`[text](url)`</span>
                </label>
              </div>
            </fieldset>

            <div className="flex flex-col gap-1">
              <FieldLabel htmlFor="markdown-media-label">{usage === MEDIA_USAGE.embed ? 'Alt text' : 'Nhãn attachment'}</FieldLabel>
              <TextInput id="markdown-media-label" name="markdownMediaLabel" value={label} disabled={disabled} placeholder={selectedMedia?.originalFileName || 'Tên file'} onChange={(event) => setLabel(event.target.value)} />
            </div>
          </div>

          <footer className={`flex justify-end gap-2 px-5 py-3 ${adminUi.hairlineT}`}>
            <Button type="button" variant="ghost" onClick={close}>Hủy</Button>
            <Button type="submit" disabled={disabled || !selectedMedia}>{usage === MEDIA_USAGE.embed ? 'Chèn ảnh' : 'Chèn attachment'}</Button>
          </footer>
        </form>
      </dialog>
      <MediaLibraryModal
        open={isLibraryOpen}
        selectionMode="single"
        onClose={() => setIsLibraryOpen(false)}
        onConfirm={selectMedia}
      />
    </>,
    document.body,
  )
}

export function NotificationMarkdownEditor({
  value,
  disabled,
  onChange,
}) {
  const textareaRef = useRef(null)
  const selectionRef = useRef({ start: value.length, end: value.length })
  const [isDialogOpen, setIsDialogOpen] = useState(false)

  const insertMedia = ({ usage, label, media }) => {
    if (!media?.contentUrl) return
    const insert = markdownForMedia({ usage, label, contentUrl: media.contentUrl })
    const { start, end } = selectionRef.current
    const nextValue = `${value.slice(0, start)}${insert}${value.slice(end)}`
    onChange(nextValue)
    const nextCursor = start + insert.length
    requestAnimationFrame(() => {
      textareaRef.current?.focus()
      textareaRef.current?.setSelectionRange(nextCursor, nextCursor)
    })
  }

  return (
    <section className={`rounded-lg ${adminUi.card} p-4`}>
      <div className="flex items-center justify-between gap-3">
        <div>
          <p className={`font-display text-[16px] font-semibold ${adminUi.title}`}>Nội dung Markdown</p>
          <p className={`mt-1 text-[12px] ${adminUi.body}`}>Chèn ảnh hoặc attachment đúng URL của Media Service.</p>
        </div>
        <Button size="icon" variant="ghost" aria-label="Thêm media vào Markdown" disabled={disabled} onClick={() => setIsDialogOpen(true)}>
          <Icon icon={ImagePlus} />
        </Button>
      </div>
      <textarea
        ref={textareaRef}
        id="batch-body"
        name="bodyMarkdown"
        value={value}
        disabled={disabled}
        placeholder="Nội dung gửi cho học viên..."
        onSelect={(event) => { selectionRef.current = { start: event.currentTarget.selectionStart, end: event.currentTarget.selectionEnd } }}
        onChange={(event) => {
          selectionRef.current = { start: event.target.selectionStart, end: event.target.selectionEnd }
          onChange(event.target.value)
        }}
        className={`${adminUi.control} mt-4 h-48 resize-y py-2`}
      />
      <MediaInsertDialog
        open={isDialogOpen}
        disabled={disabled}
        onClose={() => setIsDialogOpen(false)}
        onInsert={insertMedia}
      />
    </section>
  )
}
