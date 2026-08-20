import { useState } from 'react'
import { RotateCcw } from 'lucide-react'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { Button } from '@/components/ui/Button'
import { EmptyState } from '@/components/ui/EmptyState'
import { Icon } from '@/components/ui/Icon'
import { PageHeader } from '@/components/ui/PageHeader'
import { POST_MEDIA_DIRECT_ACTIVITY } from '@/constants/activities/postMediaDirect'
import { MEDIA_COPY } from '@/constants/mediaCopy'
import { UI_LABELS } from '@/constants/ui'
import { useDirectMediaUpload } from '@/hooks/media/useDirectMediaUpload'
import { ui } from '@/theme'
import { DirectUploadForm } from './components/DirectUploadForm'
import { DirectUploadProgress } from './components/DirectUploadProgress'
import { MediaResult } from './components/MediaResult'

function toOutputJson({ data, error, success, query, traceId }) {
  return error
    ? { data: null, error, meta: { traceId, query } }
    : { data, meta: { traceId, query }, success }
}

export function MediaDirectUploadPage() {
  const upload = useDirectMediaUpload()
  const [file, setFile] = useState(null)
  const [fileKey, setFileKey] = useState(0)

  const resetAll = () => {
    upload.reset()
    setFile(null)
    setFileKey((value) => value + 1)
  }
  const formProps = {
    query: upload.query,
    file,
    fileKey,
    loading: upload.loading,
    onQueryChange: upload.setQuery,
    onFileChange: setFile,
    onSubmit: () => upload.submit(file),
  }

  return (
    <Workbench
      className={ui.workbenchResponsive}
      input={
        <InputPanel
          actions={
            <Button variant="ghost" onClick={resetAll}>
              <Icon icon={RotateCcw} />
              {UI_LABELS.reset}
            </Button>
          }
          guided={
            <div className="flex h-full min-h-0 flex-col">
              <div className="shrink-0 px-5 pt-4">
                <PageHeader
                  eyebrow="Media"
                  title={MEDIA_COPY.directUpload}
                  description={MEDIA_COPY.directUploadDescription}
                />
              </div>
              <div className="shrink-0 px-5 pt-3">
                <DirectUploadProgress
                  phase={upload.phase}
                  failedPhase={upload.failedPhase}
                  progress={upload.progress}
                />
              </div>
              <div className="min-h-0 flex-1">
                <DirectUploadForm {...formProps} />
              </div>
            </div>
          }
          manual={
            <div className="flex h-full min-h-0 flex-col">
              <div className="shrink-0 px-5 pt-4">
                <DirectUploadProgress
                  phase={upload.phase}
                  failedPhase={upload.failedPhase}
                  progress={upload.progress}
                />
              </div>
              <div className="min-h-0 flex-1">
                <DirectUploadForm {...formProps} manual />
              </div>
            </div>
          }
        />
      }
      output={
        <OutputPanel
          json={toOutputJson(upload)}
          activity={POST_MEDIA_DIRECT_ACTIVITY}
          run={{ loading: upload.loading, success: upload.success, error: upload.error }}
        >
          {upload.error ? (
            <EmptyState title={MEDIA_COPY.failed} description={`${upload.error.code}: ${upload.error.message}`} />
          ) : null}
          {!upload.error && !upload.data ? (
            upload.loading ? (
              <DirectUploadProgress
                phase={upload.phase}
                failedPhase={upload.failedPhase}
                progress={upload.progress}
              />
            ) : <EmptyState title={MEDIA_COPY.empty} description={MEDIA_COPY.directEmptyHint} />
          ) : null}
          {!upload.error && upload.data ? <MediaResult media={upload.data} showDraft /> : null}
        </OutputPanel>
      }
    />
  )
}
