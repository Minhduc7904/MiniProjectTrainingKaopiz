import { RefreshCw, RotateCcw } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { EmptyState } from '@/components/ui/EmptyState'
import { Icon } from '@/components/ui/Icon'
import { PageHeader } from '@/components/ui/PageHeader'
import { Pagination } from '@/components/ui/Pagination'
import { TableSkeleton } from '@/components/ui/Skeleton'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { GET_MEDIA_JOBS_ACTIVITY } from '@/constants/activities/getMediaJobs'
import { MEDIA_JOB_COPY } from '@/constants/mediaJobs'
import { UI_LABELS } from '@/constants/ui'
import { useMediaJobsList } from '@/hooks/media/useMediaJobsList'
import { ui } from '@/theme'
import { MediaJobsFilters } from './components/MediaJobsFilters'
import { MediaJobsManualForm } from './components/MediaJobsManualForm'
import { MediaJobsTable } from './components/MediaJobsTable'

export function MediaJobsPage() {
  const list = useMediaJobsList()
  const output = list.error ? { data: null, error: list.error, meta: { traceId: list.traceId, query: list.query } } : { data: list.data, meta: { traceId: list.traceId, pagination: list.pagination, query: list.query }, success: list.success }
  return <Workbench input={<InputPanel actions={<div className="flex gap-2"><Button variant="ghost" disabled={list.loading} onClick={() => list.load(list.query)}><Icon icon={RefreshCw} />Làm mới</Button><Button variant="ghost" disabled={list.loading} onClick={list.reset}><Icon icon={RotateCcw} />{UI_LABELS.reset}</Button></div>} guided={<div className="flex h-full min-h-0 flex-col"><div className="min-h-0 flex-1 overflow-y-auto px-5 py-4"><PageHeader eyebrow="Media · operations" title={MEDIA_JOB_COPY.title} description={MEDIA_JOB_COPY.description} /><div className="mt-5"><MediaJobsFilters query={list.query} loading={list.loading} onChange={list.load} /></div></div><div className={`shrink-0 px-5 py-3 ${ui.hairlineT}`}><Pagination page={list.query.page} pageSize={list.query.pageSize} totalItems={list.pagination.totalItems} totalPages={list.pagination.totalPages} loading={list.loading} itemLabel={MEDIA_JOB_COPY.item} onPageChange={(page) => list.load({ ...list.query, page })} onPageSizeChange={(pageSize) => list.load({ ...list.query, page: 1, pageSize })} /></div></div>} manual={<MediaJobsManualForm query={list.query} loading={list.loading} onChange={list.setQuery} onSubmit={list.load} />} />} output={<OutputPanel json={output} activity={GET_MEDIA_JOBS_ACTIVITY} run={{ loading: list.loading, success: list.success, error: list.error }}>{list.error ? <EmptyState title="Không đọc được Media job" description={`${list.error.code}: ${list.error.message}`} /> : null}{!list.error && list.loading && !list.data.length ? <TableSkeleton /> : null}{!list.error && !list.loading && list.success && !list.data.length ? <EmptyState title={MEDIA_JOB_COPY.empty} description={MEDIA_JOB_COPY.emptyHint} /> : null}{!list.error && list.data.length ? <MediaJobsTable rows={list.data} /> : null}</OutputPanel>} />
}
