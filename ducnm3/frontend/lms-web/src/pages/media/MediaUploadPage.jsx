import { useState } from 'react'
import { RotateCcw } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { Icon } from '@/components/ui/admin/Icon'
import { LoadingState } from '@/components/ui/admin/LoadingState'
import { PageHeader } from '@/components/ui/admin/PageHeader'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { POST_MEDIA_ACTIVITY } from '@/constants/activities/postMedia'
import { HTTP_STATUS } from '@/constants/http'
import { MEDIA_COPY } from '@/constants/mediaCopy'
import { UI_LABELS } from '@/constants/ui'
import { useMediaUpload } from '@/hooks/media/useMediaUpload'
import { MediaManualForm } from './components/MediaManualForm'
import { MediaResult } from './components/MediaResult'
import { MediaUploadForm } from './components/MediaUploadForm'

function toOutputJson({ data, error, success, query, traceId, location }) {
  if (error) {
    return {
      data: null,
      error,
      meta: { traceId, query },
    }
  }

  return {
    data,
    meta: { traceId, query, location },
    success,
  }
}

export function MediaUploadPage() {
  const {
    data,
    query,
    loading,
    success,
    error,
    traceId,
    location,
    setQuery,
    submit,
    reset,
  } = useMediaUpload()
  const [file, setFile] = useState(null)
  const [fileKey, setFileKey] = useState(0)

  const send = () => {
    submit({
      file,
    })
  }

  return (
    <Workbench
      input={
        <InputPanel
          actions={
            <Button
              variant="ghost"
              disabled={loading}
              onClick={() => {
                reset()
                setFile(null)
                setFileKey((current) => current + 1)
              }}
            >
              <Icon icon={RotateCcw} />
              {UI_LABELS.reset}
            </Button>
          }
          guided={
            <div className="flex h-full min-h-0 flex-col">
              <div className="shrink-0 px-5 pt-4">
                <PageHeader
                  eyebrow="Media"
                  title={MEDIA_COPY.upload}
                  description={`${POST_MEDIA_ACTIVITY.method} ${POST_MEDIA_ACTIVITY.path}. ${HTTP_STATUS.created} = media gốc READY; thumbnail có thể QUEUED.`}
                />
              </div>
              <div className="min-h-0 flex-1">
                <MediaUploadForm
                  file={file}
                  fileKey={fileKey}
                  loading={loading}
                  onFileChange={setFile}
                  onSubmit={send}
                />
              </div>
            </div>
          }
          manual={
            <MediaManualForm
              query={query}
              file={file}
              fileKey={fileKey}
              loading={loading}
              onQueryChange={setQuery}
              onFileChange={setFile}
              onSubmit={send}
            />
          }
        />
      }
      output={
        <OutputPanel
          json={toOutputJson({
            data,
            error,
            success,
            query,
            traceId,
            location,
          })}
          activity={POST_MEDIA_ACTIVITY}
          run={{ loading, success, error }}
        >
          {error ? (
            <EmptyState
              title={MEDIA_COPY.failed}
              description={`${error.code}: ${error.message}`}
            />
          ) : null}
          {!error && loading ? <LoadingState /> : null}
          {!error && !loading && !data ? (
            <EmptyState
              title={MEDIA_COPY.empty}
              description={MEDIA_COPY.emptyHint}
            />
          ) : null}
          {!error && data ? <MediaResult media={data} location={location} /> : null}
        </OutputPanel>
      }
    />
  )
}
