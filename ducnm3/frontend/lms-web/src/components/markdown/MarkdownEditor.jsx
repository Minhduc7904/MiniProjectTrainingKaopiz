import { useRef, useState } from 'react'
import { ImagePlus } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { Icon } from '@/components/ui/admin/Icon'
import { MediaLibraryModal } from '@/components/media/MediaLibraryModal'
import { adminUi } from '@/theme/admin'

export function MarkdownEditor({ value, onChange, disabled = false, placeholder = 'Nội dung Markdown...', id = 'markdown-content' }) {
  const textareaRef = useRef(null)
  const selectionRef = useRef({ start: value.length, end: value.length })
  const [libraryOpen, setLibraryOpen] = useState(false)

  const insertMedia = (items) => {
    const media = items[0]
    if (!media?.contentUrl) return
    const alt = (media.originalFileName || media.id).replaceAll(']', '')
    const insert = `![${alt}](${media.contentUrl})`
    const { start, end } = selectionRef.current
    const nextValue = `${value.slice(0, start)}${insert}${value.slice(end)}`
    onChange(nextValue)
    const cursor = start + insert.length
    requestAnimationFrame(() => {
      textareaRef.current?.focus()
      textareaRef.current?.setSelectionRange(cursor, cursor)
    })
  }

  return (
    <div className={`rounded-lg p-4 ${adminUi.card}`}>
      <div className="flex items-center justify-between gap-3"><div><p className={`font-display text-[16px] font-semibold ${adminUi.title}`}>Nội dung Markdown</p><p className={`mt-1 text-[12px] ${adminUi.body}`}>Chèn ảnh từ Media Service bằng content URL.</p></div><Button type="button" size="icon" variant="ghost" aria-label="Chèn media" disabled={disabled} onClick={() => setLibraryOpen(true)}><Icon icon={ImagePlus} /></Button></div>
      <textarea ref={textareaRef} id={id} value={value} disabled={disabled} placeholder={placeholder} onSelect={(event) => { selectionRef.current = { start: event.currentTarget.selectionStart, end: event.currentTarget.selectionEnd } }} onChange={(event) => { selectionRef.current = { start: event.target.selectionStart, end: event.target.selectionEnd }; onChange(event.target.value) }} className={`${adminUi.control} mt-4 min-h-48 resize-y py-2`} />
  <MediaLibraryModal open={libraryOpen} selectionMode="single" onClose={() => setLibraryOpen(false)} onConfirm={(items) => { insertMedia(items); setLibraryOpen(false) }} />
    </div>
  )
}
