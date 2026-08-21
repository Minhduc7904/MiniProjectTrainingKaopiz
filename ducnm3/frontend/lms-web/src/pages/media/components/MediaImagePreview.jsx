import { useEffect, useState } from 'react'
import { getMediaContentRequest } from '@/api/mediaApi'
import { ImageOff, LoaderCircle } from 'lucide-react'
import { Icon } from '@/components/ui/admin/Icon'
import { MEDIA_COPY } from '@/constants/mediaCopy'
import { adminUi } from '@/theme/admin'

export function MediaImagePreview({ contentUrl, alt }) {
  const [state, setState] = useState('empty')
  const [imageUrl, setImageUrl] = useState(null)

  useEffect(() => {
    if (!contentUrl) {
      setState('empty')
      setImageUrl(null)
      return undefined
    }

    const controller = new AbortController()
    let objectUrl = null
    setState('loading')
    setImageUrl(null)

    getMediaContentRequest(contentUrl, { signal: controller.signal })
      .then((content) => {
        if (controller.signal.aborted) return
        objectUrl = URL.createObjectURL(content)
        setImageUrl(objectUrl)
      })
      .catch(() => {
        if (!controller.signal.aborted) setState('error')
      })

    return () => {
      controller.abort()
      if (objectUrl) URL.revokeObjectURL(objectUrl)
    }
  }, [contentUrl])

  if (!contentUrl) return null

  return (
    <div className={`relative aspect-video overflow-hidden rounded-lg ${adminUi.card}`}>
      {state === 'loading' ? (
        <div className={`absolute inset-0 flex items-center justify-center ${adminUi.body}`}>
          <Icon icon={LoaderCircle} className="animate-spin" />
          <span className="ml-2 text-[13px]">{MEDIA_COPY.loadingImage}</span>
        </div>
      ) : null}
      {state === 'error' ? (
        <div className={`absolute inset-0 flex flex-col items-center justify-center gap-2 ${adminUi.caption}`}>
          <Icon icon={ImageOff} />
          <span className="text-[13px]">{MEDIA_COPY.imageUnavailable}</span>
        </div>
      ) : null}
      <img
        src={imageUrl ?? undefined}
        alt={alt}
        className={`h-full w-full object-contain ${state === 'ready' ? 'opacity-100' : 'opacity-0'}`}
        onLoad={() => setState('ready')}
        onError={() => setState('error')}
      />
    </div>
  )
}
