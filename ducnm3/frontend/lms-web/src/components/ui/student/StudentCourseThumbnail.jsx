import { useEffect, useState } from 'react'
import { ImageOff, LoaderCircle } from 'lucide-react'
import { fetchStudentMediaContentRequest } from '@/api/studentLearningApi'
import { studentUi } from '@/theme/student'

export function StudentCourseThumbnail({ alt, contentUrl, className = '' }) {
  const [state, setState] = useState('empty')
  const [imageUrl, setImageUrl] = useState(null)

  useEffect(() => {
    if (!contentUrl) { setState('empty'); setImageUrl(null); return undefined }
    const controller = new AbortController()
    let objectUrl = null
    setState('loading')
    setImageUrl(null)
    fetchStudentMediaContentRequest(contentUrl, { signal: controller.signal })
      .then((content) => {
        if (controller.signal.aborted) return
        objectUrl = URL.createObjectURL(content)
        setImageUrl(objectUrl)
      })
      .catch(() => { if (!controller.signal.aborted) setState('error') })
    return () => { controller.abort(); if (objectUrl) URL.revokeObjectURL(objectUrl) }
  }, [contentUrl])

  if (!contentUrl) return <div aria-label="Khóa học chưa có thumbnail" className={`${studentUi.courseThumbnailFallback} ${className}`}><ImageOff aria-hidden="true" size={26} /></div>

  return (
    <div className={`${studentUi.courseThumbnail} ${className}`}>
      {state === 'loading' ? <div className={studentUi.courseThumbnailOverlay}><LoaderCircle aria-label="Đang tải thumbnail" className="animate-spin" size={22} /></div> : null}
      {state === 'error' ? <div className={studentUi.courseThumbnailOverlay}><ImageOff aria-label="Không tải được thumbnail" size={22} /></div> : null}
      <img src={imageUrl ?? undefined} alt={alt} className={state === 'ready' ? studentUi.courseThumbnailImage : 'hidden'} onLoad={() => setState('ready')} onError={() => setState('error')} />
    </div>
  )
}
