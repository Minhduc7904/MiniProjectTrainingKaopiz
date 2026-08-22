import { Download, FileText, ImageOff, LoaderCircle } from 'lucide-react'
import { useEffect, useState } from 'react'
import { getMediaTypeIcon, getMediaTypeLabel } from '@/components/media/mediaPresentation'
import { getMediaPreviewKind, MEDIA_PREVIEW_KIND } from '@/components/media/mediaPreviewKind'
import { resolveMediaContentUrl } from '@/components/media/mediaContentUrl'
import { fetchStudentMediaContentRequest } from '@/api/studentLearningApi'
import { studentUi } from '@/theme/student'

function StudentImage({ alt, contentUrl }) {
  const [state, setState] = useState('loading')
  const [imageUrl, setImageUrl] = useState(null)

  useEffect(() => {
    const controller = new AbortController()
    let objectUrl = null
    setState('loading')
    setImageUrl(null)
    fetchStudentMediaContentRequest(contentUrl, { signal: controller.signal })
      .then((content) => { if (!controller.signal.aborted) { objectUrl = URL.createObjectURL(content); setImageUrl(objectUrl) } })
      .catch(() => { if (!controller.signal.aborted) setState('error') })
    return () => { controller.abort(); if (objectUrl) URL.revokeObjectURL(objectUrl) }
  }, [contentUrl])

  if (state === 'error') return <div className={`grid min-h-64 place-items-center rounded-3xl ${studentUi.cardMuted}`}><span className={`flex flex-col items-center gap-2 ${studentUi.caption}`}><ImageOff size={30} />Không tải được ảnh</span></div>
  return <div className={`relative overflow-hidden rounded-3xl ${studentUi.cardMuted}`}>
    {state === 'loading' ? <span className={`absolute inset-0 grid place-items-center ${studentUi.caption}`}><LoaderCircle className="animate-spin" size={24} /></span> : null}
    <img src={imageUrl ?? undefined} alt={alt} className={`mx-auto max-h-[calc(100svh-15rem)] w-full object-contain ${state === 'ready' ? 'opacity-100' : 'opacity-0'}`} onLoad={() => setState('ready')} onError={() => setState('error')} />
  </div>
}

export function StudentLessonMediaPreview({ media }) {
  const kind = getMediaPreviewKind(media)
  const contentUrl = resolveMediaContentUrl(media.contentUrl)
  const MediaIcon = getMediaTypeIcon(media.mediaType)
  const fileName = media.originalFileName || 'Media bài học'

  return <section className="mx-auto w-full max-w-4xl">
    <header className="mb-5"><h2 className={studentUi.sectionTitle}>{fileName}</h2><p className={`mt-1 flex items-center gap-2 ${studentUi.caption}`}><MediaIcon size={15} />{getMediaTypeLabel(media.mediaType)} · {media.contentType}</p></header>
    {kind === MEDIA_PREVIEW_KIND.image ? <StudentImage alt={fileName} contentUrl={media.contentUrl} /> : null}
    {kind === MEDIA_PREVIEW_KIND.video ? <video className="max-h-[calc(100svh-15rem)] w-full rounded-3xl bg-student-ink" controls preload="metadata"><source src={contentUrl} type={media.contentType} />Trình duyệt không hỗ trợ phát video.</video> : null}
    {kind === MEDIA_PREVIEW_KIND.audio ? <div className={`rounded-3xl p-5 ${studentUi.card}`}><audio className="w-full" controls preload="metadata"><source src={contentUrl} type={media.contentType} />Trình duyệt không hỗ trợ phát audio.</audio></div> : null}
    {kind === MEDIA_PREVIEW_KIND.pdf ? <iframe title={`Xem trước ${fileName}`} src={contentUrl} className={`h-[calc(100svh-15rem)] min-h-80 w-full rounded-3xl ${studentUi.card}`} /> : null}
    {kind === MEDIA_PREVIEW_KIND.download ? <div className={`flex min-h-64 flex-col items-center justify-center rounded-3xl p-6 text-center ${studentUi.card}`}><FileText size={40} className={studentUi.sectionIcon} /><p className={`mt-4 font-student-display text-lg font-bold text-student-ink`}>Không hỗ trợ xem trước loại tệp này</p><a className={`mt-5 inline-flex min-h-11 cursor-pointer items-center gap-2 rounded-2xl px-4 ${studentUi.buttonPrimary}`} href={contentUrl} download={fileName}><Download size={16} />Tải file</a></div> : null}
  </section>
}
