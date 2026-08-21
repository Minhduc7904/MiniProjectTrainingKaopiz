import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { Check, FileUp, Grid2X2, Images, List, LoaderCircle, X } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { FileInput } from '@/components/ui/admin/Field'
import { Icon } from '@/components/ui/admin/Icon'
import { Tabs } from '@/components/ui/admin/Tabs'
import { MEDIA_TYPES, MEDIA_TYPE_LABELS } from '@/constants/media'
import { useDirectMediaUpload } from '@/hooks/media/useDirectMediaUpload'
import { useMediaLibrary } from '@/hooks/media/useMediaLibrary'
import { adminUi } from '@/theme/admin'
import { MediaImagePreview } from '@/pages/media/components/MediaImagePreview'
import { getMediaTypeIcon, getMediaTypeLabel } from '@/components/media/mediaPresentation'

const LIBRARY_TABS = [
  { id: '', label: 'Tất cả', icon: Images },
  ...Object.values(MEDIA_TYPES).map((id) => ({ id, label: MEDIA_TYPE_LABELS[id] })),
]

const EMPTY_SELECTION = []

function formatUploadDate(value) {
  if (!value) return '—'
  return new Intl.DateTimeFormat('vi-VN', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function MediaLibraryModal({
  open,
  selectionMode = 'single',
  initialSelection = EMPTY_SELECTION,
  showUpload = true,
  onClose,
  onConfirm,
}) {
  const dialogRef = useRef(null)
  const [tab, setTab] = useState('library')
  const [mediaType, setMediaType] = useState('')
  const [selected, setSelected] = useState(initialSelection)
  const [file, setFile] = useState(null)
  const [fileKey, setFileKey] = useState(0)
  const [viewMode, setViewMode] = useState('grid')
  const library = useMediaLibrary(mediaType, open && tab === 'library')
  const upload = useDirectMediaUpload()

  useEffect(() => {
    if (!open) return undefined
    const dialog = dialogRef.current
    if (!dialog) return undefined
    dialog.showModal()
    return () => dialog.close()
  }, [open])

  useEffect(() => {
    if (open) setSelected(initialSelection)
  }, [initialSelection, open])

  const close = () => {
    upload.reset()
    setFile(null)
    setTab('library')
    onClose()
  }

  const toggle = (media) => {
    if (selectionMode === 'single') {
      setSelected([media])
      return
    }
    setSelected((current) => current.some((item) => item.id === media.id)
      ? current.filter((item) => item.id !== media.id)
      : [...current, media])
  }

  const submitUpload = async (event) => {
    event.preventDefault()
    const completed = await upload.submit(file)
    if (completed) {
      setFile(null)
      setFileKey((value) => value + 1)
      setTab('library')
    }
  }

  if (!open) return null

  return createPortal(
    <dialog
      ref={dialogRef}
      className={`${adminUi.modal} flex max-h-[calc(100vh-2rem)] w-[min(960px,calc(100vw-2rem))] max-w-none flex-col overflow-hidden`}
      aria-labelledby="media-library-title"
      onClick={(event) => { if (event.target === event.currentTarget) close() }}
      onCancel={(event) => { event.preventDefault(); close() }}
    >
      <header className={`flex items-start justify-between gap-4 px-5 py-4 ${adminUi.hairlineB}`}>
        <div>
          <p id="media-library-title" className={`font-display text-[18px] font-semibold ${adminUi.title}`}>Thư viện media</p>
          <p className={`mt-1 text-[13px] ${adminUi.body}`}>Chọn media đã tải lên hoặc tải media mới.</p>
        </div>
        <Button type="button" variant="ghost" size="icon" aria-label="Đóng" onClick={close}><Icon icon={X} /></Button>
      </header>

      <div className={`px-5 pt-3 ${adminUi.hairlineB}`}>
        <Tabs
          tabs={showUpload
            ? [{ id: 'library', label: 'Thư viện', icon: Images }, { id: 'upload', label: 'Upload', icon: FileUp }]
            : [{ id: 'library', label: 'Thư viện', icon: Images }]}
          value={tab}
          onChange={setTab}
        />
      </div>

      {tab === 'library' ? (
        <div className="flex min-h-0 flex-1 flex-col md:flex-row">
          <aside className={`shrink-0 border-b p-3 md:w-44 md:border-b-0 md:border-r ${adminUi.hairlineB}`}>
            <p className={`px-2 pb-2 text-[11px] font-medium tracking-[0.18em] uppercase ${adminUi.eyebrow}`}>Loại media</p>
            <div className="flex gap-1 overflow-x-auto md:flex-col">
              {LIBRARY_TABS.map((item) => (
                <button
                  key={item.id || 'all'}
                  type="button"
                  className={`whitespace-nowrap rounded-md px-3 py-2 text-left text-[13px] ${mediaType === item.id ? adminUi.navActive : adminUi.navIdle}`}
                  onClick={() => setMediaType(item.id)}
                >{item.label}</button>
              ))}
            </div>
          </aside>
          <section className="min-h-0 min-w-0 flex-1 overflow-y-auto p-4">
            <div className="mb-4 flex items-center justify-between gap-3">
              <p className={`text-[12px] ${adminUi.body}`}>{library.data.length} media đã tải</p>
              <div className={`flex items-center gap-1 rounded-md p-1 ${adminUi.choiceIdle}`} role="group" aria-label="Kiểu hiển thị">
                <Button
                  type="button"
                  variant={viewMode === 'grid' ? 'primary' : 'ghost'}
                  size="icon"
                  aria-label="Hiển thị dạng lưới"
                  onClick={() => setViewMode('grid')}
                ><Icon icon={Grid2X2} /></Button>
                <Button
                  type="button"
                  variant={viewMode === 'list' ? 'primary' : 'ghost'}
                  size="icon"
                  aria-label="Hiển thị dạng danh sách"
                  onClick={() => setViewMode('list')}
                ><Icon icon={List} /></Button>
              </div>
            </div>
            {library.error ? <p className={`rounded-md px-3 py-2 text-[13px] ${adminUi.badgeDanger}`}>{library.error.message}</p> : null}
            {library.loading && !library.data.length ? <div className={`flex h-48 items-center justify-center ${adminUi.body}`}><Icon icon={LoaderCircle} className="animate-spin" /></div> : null}
            {!library.loading && !library.data.length && !library.error ? <p className={`flex h-48 items-center justify-center text-[13px] ${adminUi.body}`}>Chưa có media phù hợp.</p> : null}
            {viewMode === 'grid' ? <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
              {library.data.map((media) => {
                const active = selected.some((item) => item.id === media.id)
                    const readyThumbnail = media.thumbnailUrl && media.thumbnailStatus === 'READY'
                const fallbackImage = media.mediaType === 'IMAGE' && media.contentUrl
                return (
                  <button
                    key={media.id}
                    type="button"
                    className={`relative overflow-hidden rounded-md border text-left ${active ? 'border-accent ring-2 ring-accent/30' : 'border-line'} bg-surface`}
                    onClick={() => toggle(media)}
                  >
                    <div className="bg-surface-muted">
                      {readyThumbnail ? <MediaImagePreview contentUrl={media.thumbnailUrl} alt="" /> : fallbackImage ? <MediaImagePreview contentUrl={media.contentUrl} alt="" /> : <div className={`flex h-full flex-col items-center justify-center gap-2 ${adminUi.caption}`}><Icon icon={getMediaTypeIcon(media.mediaType)} size={28} /><span className="text-[11px]">{getMediaTypeLabel(media.mediaType)}</span></div>}
                    </div>
                    {active ? <span className="absolute right-2 top-2 rounded-full bg-accent p-1 text-white"><Icon icon={Check} size={14} /></span> : null}
                    <div className="p-2"><p className={`truncate text-[12px] font-medium ${adminUi.title}`}>{media.originalFileName}</p><p className={`mt-1 text-[11px] ${adminUi.caption}`}>{getMediaTypeLabel(media.mediaType)} · {media.status} · {media.isDraft ? 'Draft' : 'Đã dùng'}</p><p className={`mt-1 truncate text-[10px] ${adminUi.caption}`}>{formatUploadDate(media.completedAtUtc || media.createdAtUtc)}</p></div>
                  </button>
                )
              })}
            </div> : (
              <div className={`overflow-hidden rounded-md ${adminUi.card}`}>
                <div className={`grid grid-cols-[minmax(0,2fr)_100px_120px_100px_170px_48px] gap-3 px-3 py-2 text-[10px] font-medium tracking-[0.14em] uppercase ${adminUi.tableHead}`}>
                  <span>Tên file</span><span>Loại</span><span>Trạng thái</span><span>Draft</span><span>Upload lúc</span><span />
                </div>
                {library.data.map((media) => {
                  const active = selected.some((item) => item.id === media.id)
                  const readyThumbnail = media.thumbnailUrl && media.thumbnailStatus === 'READY'
                  const fallbackImage = media.mediaType === 'IMAGE' && media.contentUrl
                  return (
                    <button key={media.id} type="button" className={`grid w-full grid-cols-[minmax(0,2fr)_100px_120px_100px_170px_48px] items-center gap-3 px-3 py-2 text-left ${adminUi.tableRow} ${active ? 'bg-accent-soft' : ''}`} onClick={() => toggle(media)}>
                      <span className="flex min-w-0 items-center gap-3"><span className="h-10 w-12 shrink-0 overflow-hidden rounded bg-surface-muted">{readyThumbnail ? <MediaImagePreview contentUrl={media.thumbnailUrl} alt="" /> : fallbackImage ? <MediaImagePreview contentUrl={media.contentUrl} alt="" /> : <span className={`flex h-full items-center justify-center ${adminUi.caption}`}><Icon icon={getMediaTypeIcon(media.mediaType)} size={18} /></span>}</span><span className={`truncate text-[12px] font-medium ${adminUi.title}`}>{media.originalFileName}</span></span>
                      <span className={`text-[11px] ${adminUi.body}`}>{getMediaTypeLabel(media.mediaType)}</span>
                      <span className={`text-[11px] ${adminUi.body}`}>{media.status}</span>
                      <span className={`text-[11px] ${adminUi.body}`}>{media.isDraft ? 'Draft' : 'Đã dùng'}</span>
                      <span className={`text-[11px] ${adminUi.body}`}>{formatUploadDate(media.completedAtUtc || media.createdAtUtc)}</span>
                      <span className="flex justify-end">{active ? <span className="rounded-full bg-accent p-1 text-white"><Icon icon={Check} size={14} /></span> : null}</span>
                    </button>
                  )
                })}
              </div>
            )}
            {library.hasMore ? <div className="mt-4 flex justify-center"><Button variant="ghost" disabled={library.loading} onClick={library.loadMore}>{library.loading ? 'Đang tải...' : 'Tải thêm'}</Button></div> : null}
          </section>
        </div>
      ) : showUpload ? (
        <form className="min-h-[420px] p-5" onSubmit={submitUpload}>
          <div className={`rounded-md p-4 ${adminUi.choiceIdle}`}>
            <p className={`font-display text-[14px] font-semibold ${adminUi.title}`}>Upload direct</p>
            <p className={`mt-1 text-[12px] ${adminUi.body}`}>Backend tự nhận diện loại media và chọn bucket phù hợp.</p>
            <div className="mt-4"><FileInput key={`library-upload-${fileKey}`} id="library-upload-file" name="libraryUploadFile" fileName={file?.name} disabled={upload.loading} onChange={(event) => setFile(event.target.files?.[0] ?? null)} /></div>
            {upload.error ? <p className={`mt-3 rounded-md px-3 py-2 text-[13px] ${adminUi.badgeDanger}`}>{upload.error.message}</p> : null}
            {upload.loading ? <p className={`mt-3 flex items-center gap-2 text-[13px] ${adminUi.body}`}><Icon icon={LoaderCircle} className="animate-spin" />Đang upload...</p> : null}
          </div>
          <div className="mt-4 flex justify-end"><Button type="submit" disabled={!file || upload.loading}><Icon icon={FileUp} />Upload vào thư viện</Button></div>
        </form>
      ) : null}

      <footer className={`flex justify-end gap-2 px-5 py-3 ${adminUi.hairlineT}`}>
        <Button type="button" variant="ghost" onClick={close}>Hủy</Button>
        <Button type="button" disabled={!selected.length} onClick={() => { onConfirm(selected); close() }}>Chọn {selected.length ? `(${selected.length})` : ''}</Button>
      </footer>
    </dialog>,
    document.body,
  )
}
