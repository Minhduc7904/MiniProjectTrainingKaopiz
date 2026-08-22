import { useEffect, useState } from 'react'
import { File, FileText, Image, ImagePlus, Music2, Pencil, Trash2, Video } from 'lucide-react'
import { RenderedMarkdown } from '@/components/markdown/RenderedMarkdown'
import { MediaImagePreview } from '@/pages/media/components/MediaImagePreview'
import { Button } from '@/components/ui/admin/Button'
import { Icon } from '@/components/ui/admin/Icon'
import { Tabs } from '@/components/ui/admin/Tabs'
import { adminUi } from '@/theme/admin'

const CONTENT_TAB = 'content'
const DOCUMENTS_TAB = 'documents'

function mediaIcon(mediaType) {
  return ({ IMAGE: Image, VIDEO: Video, AUDIO: Music2, DOCUMENT: FileText }[mediaType] ?? File)
}

export function LessonDetailTabs({
  lesson,
  lessonNumber,
  mediaError,
  mediaSaving,
  onDelete,
  onEdit,
  onOpenLibrary,
  onOpenUpload,
  onPreviewMedia,
  onRemoveMedia,
}) {
  const [activeTab, setActiveTab] = useState(CONTENT_TAB)
  const attachments = lesson.attachments ?? []

  useEffect(() => {
    setActiveTab(CONTENT_TAB)
  }, [lesson.id])

  return (
    <>
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className={`text-[11px] font-medium tracking-[0.18em] uppercase ${adminUi.eyebrow}`}>Lesson {String(lessonNumber).padStart(2, '0')}</p>
          <h3 className={`mt-1 font-display text-[20px] font-semibold ${adminUi.title}`}>{lesson.title}</h3>
        </div>
        <div className="flex shrink-0 items-center gap-2">
          <Button type="button" size="sm" variant="ghost" onClick={() => onEdit(lesson.id)}><Icon icon={Pencil} size={15} />Sửa</Button>
          <Button type="button" size="sm" variant="danger" onClick={() => onDelete(lesson)}><Icon icon={Trash2} size={15} />Xóa lesson</Button>
        </div>
      </div>

      <div className={`mt-5 ${adminUi.hairlineB}`}>
        <Tabs
          value={activeTab}
          onChange={setActiveTab}
          tabs={[
            { id: CONTENT_TAB, label: 'Nội dung', icon: FileText },
            { id: DOCUMENTS_TAB, label: `Tài liệu (${attachments.length})`, icon: ImagePlus },
          ]}
        />
      </div>

      {activeTab === CONTENT_TAB ? (
        <RenderedMarkdown className={`mt-5 ${adminUi.markdown}`} html={lesson.contentHtml} emptyLabel="Lesson chưa có nội dung." />
      ) : (
        <section className="mt-5">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <p className={`text-[11px] font-medium tracking-[0.18em] uppercase ${adminUi.eyebrow}`}>Tài liệu lesson</p>
              <p className={`mt-1 text-[12px] ${adminUi.caption}`}>Chọn media có sẵn hoặc upload trực tiếp trước khi gắn vào lesson.</p>
            </div>
            <div className="flex flex-wrap gap-2">
              <Button type="button" size="sm" variant="ghost" disabled={mediaSaving} onClick={onOpenLibrary}><Icon icon={ImagePlus} size={15} />Chọn từ thư viện</Button>
              <Button type="button" size="sm" disabled={mediaSaving} onClick={onOpenUpload}><Icon icon={Image} size={15} />Upload tài liệu</Button>
            </div>
          </div>

          {attachments.length ? <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
            {attachments.map((media) => {
              const MediaIcon = mediaIcon(media.mediaType)
              const previewUrl = media.thumbnailUrl || (media.mediaType === 'IMAGE' ? media.contentUrl : null)
              return <article key={media.usageId} className={`group relative overflow-hidden rounded-md ${adminUi.choiceIdle}`}>
                <button type="button" className="w-full cursor-pointer text-left" aria-label={`Xem trước ${media.originalFileName}`} onClick={() => onPreviewMedia(media)}>
                  <div className={`aspect-video ${adminUi.mediaStage}`}>
                    {previewUrl ? <MediaImagePreview contentUrl={previewUrl} alt={media.originalFileName} /> : <div className={`flex h-full flex-col items-center justify-center gap-2 ${adminUi.caption}`}><Icon icon={MediaIcon} size={30} /><span className="text-[11px]">{media.mediaType}</span></div>}
                  </div>
                  <div className="min-w-0 p-3"><p className={`truncate text-[12px] font-medium ${adminUi.title}`}>{media.originalFileName}</p><p className={`mt-1 text-[11px] ${adminUi.caption}`}>{media.mediaType} · {media.contentType}</p></div>
                </button>
                <button type="button" aria-label={`Gỡ ${media.originalFileName}`} disabled={mediaSaving} onClick={(event) => { event.stopPropagation(); onRemoveMedia(media.usageId) }} className={`absolute right-2 top-2 cursor-pointer rounded-full p-1.5 opacity-0 transition-opacity duration-150 group-hover:opacity-100 focus:opacity-100 disabled:cursor-not-allowed ${adminUi.mediaAction}`}><Icon icon={Trash2} size={14} /></button>
              </article>
            })}
          </div> : <p className={`mt-4 p-4 text-[13px] ${adminUi.emptyDropzone}`}>Chưa có tài liệu đính kèm. Chọn từ thư viện hoặc upload file mới để bắt đầu.</p>}
          {mediaError ? <p className={`mt-3 rounded-md px-3 py-2 text-[12px] ${adminUi.badgeDanger}`}>{mediaError}</p> : null}
        </section>
      )}
    </>
  )
}
