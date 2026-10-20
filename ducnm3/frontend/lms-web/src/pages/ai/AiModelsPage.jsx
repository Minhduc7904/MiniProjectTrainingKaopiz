import { useState } from 'react'
import { RotateCcw } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { ConfirmModal } from '@/components/ui/admin/ConfirmModal'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { Icon } from '@/components/ui/admin/Icon'
import { LoadingState } from '@/components/ui/admin/LoadingState'
import { PageHeader } from '@/components/ui/admin/PageHeader'
import { Pagination } from '@/components/ui/admin/Pagination'
import { TableSkeleton } from '@/components/ui/admin/Skeleton'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { AI_COPY, AI_PAGES } from '@/constants/ai'
import { UI_LABELS } from '@/constants/ui'
import { useAiModels } from '@/hooks/ai/useAiModels'
import { adminUi } from '@/theme/admin'
import { toAiOutputJson } from './aiOutput'
import { AiListFilters } from './components/AiListFilters'
import { AiModelForm } from './components/AiModelForm'
import { AiModelsTable } from './components/AiModelsTable'

export function AiModelsPage() {
  const {
    data,
    pagination,
    query,
    loading,
    success,
    error,
    traceId,
    providers,
    mutation,
    setQuery,
    load,
    reset,
    save,
    remove,
  } = useAiModels()
  const [editing, setEditing] = useState(null)
  const [deleting, setDeleting] = useState(null)
  const copy = AI_PAGES.models
  const providerFilterOptions = [
    { value: '', label: UI_LABELS.all },
    ...providers.map((provider) => ({ value: provider.id, label: provider.name })),
  ]

  const handleSubmit = async (payload) => {
    const saved = await save({ id: editing?.id, payload })
    if (saved) {
      setEditing(null)
    }
    return saved
  }

  const handleDelete = async () => {
    const removed = await remove(deleting.id)
    if (removed) {
      if (editing?.id === deleting.id) {
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
              <Button variant="ghost" disabled={loading} onClick={reset}>
                <Icon icon={RotateCcw} />
                {UI_LABELS.reset}
              </Button>
            }
            guided={
              <div className="flex h-full min-h-0 flex-col">
                <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">
                  <PageHeader eyebrow={AI_PAGES.eyebrow} title={copy.title} description={copy.description} />
                  <AiListFilters
                    idPrefix="ai-model"
                    query={query}
                    loading={loading}
                    searchHint={copy.searchHint}
                    searchPlaceholder={copy.searchPlaceholder}
                    extraFilters={
                      <Dropdown
                        id="ai-model-provider-filter"
                        label={AI_COPY.provider}
                        value={query.providerId ?? ''}
                        options={providerFilterOptions}
                        disabled={loading}
                        onChange={(providerId) => load({ ...query, providerId: providerId || undefined, page: 1 })}
                      />
                    }
                    onChange={setQuery}
                    onSubmit={load}
                  />
                  <AiModelForm
                    key={editing?.id ?? 'new'}
                    model={editing}
                    providers={providers}
                    saving={mutation.loading}
                    onSubmit={handleSubmit}
                    onCancel={() => setEditing(null)}
                  />
                </div>
                <div className={`shrink-0 px-5 py-3 ${adminUi.hairlineT}`}>
                  <Pagination
                    page={query.page}
                    totalPages={pagination.totalPages}
                    totalItems={pagination.totalItems}
                    pageSize={query.pageSize}
                    loading={loading}
                    itemLabel={AI_COPY.modelItem}
                    onPageChange={(page) => load({ ...query, page })}
                    onPageSizeChange={(pageSize) => load({ ...query, page: 1, pageSize })}
                  />
                </div>
              </div>
            }
          />
        }
        output={
          <OutputPanel
            json={toAiOutputJson({ data, pagination, traceId, error, success, query })}
            run={{ loading, success, error }}
          >
            {error ? <EmptyState title="Không tải được model" description={`${error.code}: ${error.message}`} /> : null}
            {!error && loading && data.length === 0 ? <TableSkeleton /> : null}
            {!error && success && !loading && data.length === 0 ? (
              <EmptyState title={copy.emptyTitle} description={copy.emptyDescription} />
            ) : null}
            {!error && data.length > 0 ? (
              <AiModelsTable rows={data} busy={mutation.loading} onEdit={setEditing} onDelete={setDeleting} />
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
        title={`${AI_PAGES.deleteModel.title} ${deleting?.displayName ?? ''}`.trim()}
        description={AI_PAGES.deleteModel.description}
        confirmLabel={AI_COPY.remove}
        busy={mutation.loading}
        onConfirm={handleDelete}
        onClose={() => setDeleting(null)}
      />
    </>
  )
}
