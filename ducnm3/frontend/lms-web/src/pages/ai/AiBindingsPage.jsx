import { useState } from 'react'
import { RotateCcw } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { ConfirmModal } from '@/components/ui/admin/ConfirmModal'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { Icon } from '@/components/ui/admin/Icon'
import { LoadingState } from '@/components/ui/admin/LoadingState'
import { PageHeader } from '@/components/ui/admin/PageHeader'
import { TableSkeleton } from '@/components/ui/admin/Skeleton'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { AI_COPY, AI_PAGES } from '@/constants/ai'
import { UI_LABELS } from '@/constants/ui'
import { useAiBindings } from '@/hooks/ai/useAiBindings'
import { toAiOutputJson } from './aiOutput'
import { AiBindingForm } from './components/AiBindingForm'
import { AiBindingsTable } from './components/AiBindingsTable'

export function AiBindingsPage() {
  const {
    data,
    loading,
    success,
    error,
    traceId,
    bindableServices,
    models,
    mutation,
    load,
    save,
    remove,
  } = useAiBindings()
  const [editing, setEditing] = useState(null)
  const [deleting, setDeleting] = useState(null)
  const copy = AI_PAGES.bindings

  const handleSubmit = async (input) => {
    const saved = await save(input)
    if (saved) {
      setEditing(null)
    }
    return saved
  }

  const handleDelete = async () => {
    const removed = await remove(deleting.serviceName)
    if (removed) {
      if (editing?.serviceName === deleting.serviceName) {
        setEditing(null)
      }
      setDeleting(null)
    }
  }

  return (
    <>
      <Workbench
        input={
          <InputPanel
            actions={
              <Button variant="ghost" disabled={loading} onClick={load}>
                <Icon icon={RotateCcw} />
                {UI_LABELS.reset}
              </Button>
            }
            guided={
              <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">
                <PageHeader eyebrow={AI_PAGES.eyebrow} title={copy.title} description={copy.description} />
                <AiBindingForm
                  key={editing?.serviceName ?? 'new'}
                  binding={editing}
                  services={bindableServices}
                  models={models}
                  saving={mutation.loading}
                  onSubmit={handleSubmit}
                  onCancel={() => setEditing(null)}
                />
              </div>
            }
          />
        }
        output={
          <OutputPanel
            json={toAiOutputJson({ data, pagination: undefined, traceId, error, success, query: undefined })}
            run={{ loading, success, error }}
          >
            {error ? <EmptyState title="Không tải được danh sách gán" description={`${error.code}: ${error.message}`} /> : null}
            {!error && loading && data.length === 0 ? <TableSkeleton /> : null}
            {!error && success && !loading && data.length === 0 ? (
              <EmptyState title={copy.emptyTitle} description={copy.emptyDescription} />
            ) : null}
            {!error && data.length > 0 ? (
              <AiBindingsTable rows={data} busy={mutation.loading} onEdit={setEditing} onDelete={setDeleting} />
            ) : null}
            {!error && loading && data.length > 0 ? (
              <div className="mt-3">
                <LoadingState />
              </div>
            ) : null}
          </OutputPanel>
        }
      />
      <ConfirmModal
        open={Boolean(deleting)}
        mode="delete"
        title={`${AI_PAGES.deleteBinding.title} ${deleting?.serviceName ?? ''}`.trim()}
        description={AI_PAGES.deleteBinding.description}
        confirmLabel={AI_COPY.remove}
        busy={mutation.loading}
        onConfirm={handleDelete}
        onClose={() => setDeleting(null)}
      />
    </>
  )
}
