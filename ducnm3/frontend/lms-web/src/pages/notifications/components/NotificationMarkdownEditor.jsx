import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { ImagePlus, Paperclip, X } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { FieldLabel, FileInput, TextInput } from '@/components/ui/Field'
import { Icon } from '@/components/ui/Icon'
import { ui } from '@/theme'
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

function MediaInsertDialog({
  open,
  disabled,
  uploadLoading,
  uploadError,
  onClose,
  onInsert,
}) {
  const dialogRef = useRef(null)
  const [file, setFile] = useState(null)
  const [usage, setUsage] = useState(MEDIA_USAGE.embed)
  const [label, setLabel] = useState('')

  useEffect(() => {
    if (!open) return undefined
    const dialog = dialogRef.current
    if (!dialog) return undefined
    dialog.showModal()
    return () => dialog.close()
  }, [open])

  const close = () => {
    setFile(null)
    setUsage(MEDIA_USAGE.embed)
    setLabel('')
    onClose()
  }

  const submit = async (event) => {
    preventParentBatchSubmit(event)
    if (!file) return
    const inserted = await onInsert({ file, usage, label })
    if (inserted) close()
  }

  if (!open) return null

  return createPortal(
    <dialog
      ref={dialogRef}
      aria-labelledby="media-insert-title"
      onClick={(event) => {
        if (event.target === event.currentTarget) close()
      }}
      onCancel={(event) => {
        event.preventDefault()
        close()
      }}
      className={ui.modal}
    >
      <form onSubmit={submit} className="flex flex-col">
        <header className={`flex items-start justify-between gap-4 px-5 py-4 ${ui.hairlineB}`}>
          <div>
            <p className={`font-display text-[18px] font-semibold ${ui.title}`} id="media-insert-title">
              Thêm media vào Markdown
            </p>
            <p className={`mt-1 text-[13px] ${ui.body}`}>
              Media được upload trước, rồi URL an toàn được chèn tại vị trí con trỏ.
            </p>
          </div>
          <Button type="button" variant="ghost" size="icon" aria-label="Đóng modal" onClick={close}>
            <Icon icon={X} />
          </Button>
        </header>

        <div className="flex flex-col gap-4 px-5 py-4">
          <div className="flex flex-col gap-1">
            <FieldLabel htmlFor="markdown-media-file" hint="MVP upload ảnh qua Media API.">
              Chọn ảnh
            </FieldLabel>
            <FileInput
              id="markdown-media-file"
              name="markdownMediaFile"
              accept="image/*"
              disabled={disabled || uploadLoading}
              fileName={file?.name}
              onChange={(event) => setFile(event.target.files?.[0] ?? null)}
            />
          </div>

          <fieldset disabled={disabled || uploadLoading}>
            <legend className={`font-display text-[11px] font-medium tracking-[0.18em] uppercase ${ui.eyebrow}`}>
              Cách dùng trong nội dung
            </legend>
            <div className="mt-2 grid grid-cols-2 gap-2">
              <label className={`cursor-pointer rounded-md p-3 ${usage === MEDIA_USAGE.embed ? ui.choiceActive : ui.choiceIdle}`}>
                <input className="sr-only" type="radio" name="mediaUsage" checked={usage === MEDIA_USAGE.embed} onChange={() => setUsage(MEDIA_USAGE.embed)} />
                <span className="flex items-center gap-2 text-[13px] font-medium"><Icon icon={ImagePlus} />Nhúng ảnh</span>
                <span className={`mt-1 block text-[11px] ${ui.body}`}>`![alt](url)`</span>
              </label>
              <label className={`cursor-pointer rounded-md p-3 ${usage === MEDIA_USAGE.attachment ? ui.choiceActive : ui.choiceIdle}`}>
                <input className="sr-only" type="radio" name="mediaUsage" checked={usage === MEDIA_USAGE.attachment} onChange={() => setUsage(MEDIA_USAGE.attachment)} />
                <span className="flex items-center gap-2 text-[13px] font-medium"><Icon icon={Paperclip} />Attachment</span>
                <span className={`mt-1 block text-[11px] ${ui.body}`}>`[text](url)`</span>
              </label>
            </div>
          </fieldset>

          <div className="flex flex-col gap-1">
            <FieldLabel htmlFor="markdown-media-label" hint={usage === MEDIA_USAGE.embed ? 'Mô tả ảnh cho người đọc và screen reader.' : 'Nhãn văn bản của liên kết tải media.'}>
              {usage === MEDIA_USAGE.embed ? 'Alt text' : 'Nhãn attachment'}
            </FieldLabel>
            <TextInput
              id="markdown-media-label"
              name="markdownMediaLabel"
              value={label}
              disabled={disabled || uploadLoading}
              placeholder={usage === MEDIA_USAGE.embed ? 'Mô tả ảnh' : 'Tải tài liệu'}
              onChange={(event) => setLabel(event.target.value)}
            />
          </div>

          {uploadError ? <p className={`rounded-md ${ui.badgeDanger} px-3 py-2 text-[13px]`}>{uploadError.code}: {uploadError.message}</p> : null}
        </div>

        <footer className={`flex justify-end gap-2 px-5 py-3 ${ui.hairlineT}`}>
          <Button type="button" variant="ghost" onClick={close}>Hủy</Button>
          <Button
            type="submit"
            disabled={disabled || uploadLoading || !file}
          >
            {uploadLoading ? 'Đang tải ảnh' : 'Chèn vào nội dung'}
          </Button>
        </footer>
      </form>
    </dialog>
    ,
    document.body,
  )
}

export function NotificationMarkdownEditor({
  value,
  disabled,
  uploadLoading,
  uploadError,
  onChange,
  onUploadImage,
}) {
  const textareaRef = useRef(null)
  const selectionRef = useRef({ start: value.length, end: value.length })
  const [isDialogOpen, setIsDialogOpen] = useState(false)

  const insertMedia = async ({ file, usage, label }) => {
    const media = await onUploadImage(file)
    if (!media?.contentUrl) return false

    const insert = markdownForMedia({ usage, label: label.trim() || file.name, contentUrl: media.contentUrl })
    const { start, end } = selectionRef.current
    const nextValue = `${value.slice(0, start)}${insert}${value.slice(end)}`
    const nextCursor = start + insert.length
    onChange(nextValue)
    requestAnimationFrame(() => {
      textareaRef.current?.focus()
      textareaRef.current?.setSelectionRange(nextCursor, nextCursor)
    })
    return true
  }

  return (
    <section className={`rounded-lg ${ui.card} p-4`}>
      <div className="flex items-center justify-between gap-3">
        <div>
          <p className={`font-display text-[16px] font-semibold ${ui.title}`}>Nội dung Markdown</p>
          <p className={`mt-1 text-[12px] ${ui.body}`}>Chèn ảnh hoặc attachment đúng URL của Media Service.</p>
        </div>
        <Button size="icon" variant="ghost" aria-label="Thêm ảnh hoặc attachment" disabled={disabled} onClick={() => setIsDialogOpen(true)}>
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
        className={`${ui.control} mt-4 h-48 resize-y py-2`}
      />
      <MediaInsertDialog
        open={isDialogOpen}
        disabled={disabled}
        uploadLoading={uploadLoading}
        uploadError={uploadError}
        onClose={() => setIsDialogOpen(false)}
        onInsert={insertMedia}
      />
    </section>
  )
}
