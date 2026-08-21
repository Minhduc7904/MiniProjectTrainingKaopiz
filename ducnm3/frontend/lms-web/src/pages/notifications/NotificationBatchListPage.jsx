import { Eye, RefreshCw, RotateCcw } from 'lucide-react'
import { Link, useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui/admin/Button'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { Icon } from '@/components/ui/admin/Icon'
import { PageHeader } from '@/components/ui/admin/PageHeader'
import { Pagination } from '@/components/ui/admin/Pagination'
import { TableSkeleton } from '@/components/ui/admin/Skeleton'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { notificationBatchProgressPath } from '@/constants/appRoutes'
import { GET_NOTIFICATION_BATCHES_ACTIVITY } from '@/constants/activities/getNotificationBatches'
import {
  canRetryNotificationBatch,
  NOTIFICATION_BATCH_TERMINAL_STATUSES,
} from '@/constants/notification'
import { useNotificationBatchList } from '@/hooks/notifications/useNotificationBatchList'
import { adminUi } from '@/theme/admin'

const statuses = [
  '',
  'PENDING',
  'SNAPSHOTTING',
  'SNAPSHOT_READY',
  'PROCESSING',
  'COMPLETED',
  'PARTIAL_FAILED',
  'FAILED',
]

function durationMs(batch, clock) {
  if (!batch.startedAtUtc) return null
  if (batch.completedAtUtc) return batch.durationMs
  return Math.max(batch.durationMs ?? 0, clock - Date.parse(batch.startedAtUtc))
}

function formatDuration(value) {
  if (value == null) return 'Chưa bắt đầu'
  const seconds = Math.floor(value / 1000)
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${hours ? `${hours}h ` : ''}${minutes}m ${seconds % 60}s`
}

function BatchActions({ batch, retrying, onRetry }) {
  return (
    <div className="flex items-center gap-2">
      <Link
        to={notificationBatchProgressPath(batch.id)}
        className={`inline-flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-2 ${adminUi.buttonGhost}`}
      >
        <Icon icon={Eye} />
        Xem
      </Link>
      {canRetryNotificationBatch(batch) ? (
        <Button
          variant="ghost"
          disabled={retrying}
          onClick={() => onRetry(batch.id)}
        >
          <Icon icon={RefreshCw} />
          {retrying ? 'Đang retry' : 'Retry lỗi'}
        </Button>
      ) : null}
    </div>
  )
}

function BatchTable({ batches, clock, retryingBatchId, onRetry }) {
  return (
    <div className="overflow-x-auto rounded-lg border border-line">
      <table className="w-full min-w-[980px] text-left text-[13px]">
        <thead className={adminUi.tableHead}>
          <tr>
            <th className="px-3 py-3">Tiêu đề</th>
            <th className="px-3 py-3">Trạng thái</th>
            <th className="px-3 py-3">Số lượng</th>
            <th className="px-3 py-3">Thành công / lỗi</th>
            <th className="px-3 py-3">Tổng thời gian</th>
            <th className="px-3 py-3">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          {batches.map((batch) => (
            <tr key={batch.id} className={adminUi.tableRow}>
              <td className="max-w-64 px-3 py-3">
                <p className={`truncate font-medium ${adminUi.title}`}>{batch.title}</p>
                <p className={`mt-1 font-mono text-[10px] ${adminUi.caption}`}>{batch.id}</p>
              </td>
              <td className="px-3 py-3">
                <span className={`rounded-full px-2 py-1 text-[11px] ${NOTIFICATION_BATCH_TERMINAL_STATUSES.has(batch.status) ? adminUi.badgeMuted : adminUi.badgeWarning}`}>
                  {batch.status}
                </span>
              </td>
              <td className="px-3 py-3 font-mono tabular-nums">
                {batch.processedCount.toLocaleString()} / {batch.totalCount.toLocaleString()}
                <p className={`mt-1 text-[11px] ${adminUi.caption}`}>
                  Yêu cầu: {batch.requestedCount?.toLocaleString() ?? 'Tất cả'}
                </p>
              </td>
              <td className="px-3 py-3 font-mono tabular-nums">
                {batch.successCount.toLocaleString()} / {batch.failedCount.toLocaleString()}
              </td>
              <td className="px-3 py-3 font-mono tabular-nums">
                {formatDuration(durationMs(batch, clock))}
              </td>
              <td className="px-3 py-3">
                <BatchActions
                  batch={batch}
                  retrying={retryingBatchId === batch.id}
                  onRetry={onRetry}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export function NotificationBatchListPage() {
  const list = useNotificationBatchList()
  const navigate = useNavigate()
  const output = list.error
    ? { data: null, error: list.error, meta: { traceId: list.traceId, query: list.query } }
    : { data: list.data, meta: { traceId: list.traceId, pagination: list.pagination, query: list.query } }

  const retryFailed = async (batchId) => {
    const result = await list.retryFailed(batchId)
    if (result.meta.requestStatus === 'fulfilled') {
      navigate(notificationBatchProgressPath(result.payload.data.id))
    }
  }

  return (
    <Workbench
      input={
        <InputPanel
          actions={
            <Button variant="ghost" disabled={list.loading} onClick={list.reset}>
              <Icon icon={RotateCcw} />
              Đặt lại
            </Button>
          }
          guided={
            <div className="flex h-full min-h-0 flex-col">
              <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">
                <PageHeader
                  eyebrow="Notification · operations"
                  title="Quản lý batch job"
                  description="Theo dõi batch và retry trực tiếp riêng các recipient thất bại."
                />
                <div className="mt-5">
                  <Dropdown
                    label="Trạng thái"
                    value={list.query.status}
                    disabled={list.loading}
                    options={statuses.map((status) => ({ value: status, label: status || 'Tất cả' }))}
                    onChange={(status) => list.load({ ...list.query, page: 1, status })}
                  />
                </div>
              </div>
              <div className={`shrink-0 px-5 py-3 ${adminUi.hairlineT}`}>
                <Pagination
                  page={list.query.page}
                  pageSize={list.query.pageSize}
                  totalItems={list.pagination.totalItems}
                  totalPages={list.pagination.totalPages}
                  loading={list.loading}
                  itemLabel="batch"
                  onPageChange={(page) => list.load({ ...list.query, page })}
                  onPageSizeChange={(pageSize) => list.load({ ...list.query, page: 1, pageSize })}
                />
              </div>
            </div>
          }
          manual={<div className="p-5"><p className={adminUi.body}>GET /notification/api/notification-batches</p></div>}
        />
      }
      output={
        <OutputPanel
          json={output}
          activity={GET_NOTIFICATION_BATCHES_ACTIVITY}
          run={{ loading: list.loading, success: list.success, error: list.error }}
        >
          {list.error ? <EmptyState title="Không đọc được batch" description={`${list.error.code}: ${list.error.message}`} /> : null}
          {list.retryError ? <p className={`mb-3 rounded-md px-3 py-2 text-[13px] ${adminUi.badgeDanger}`}>{list.retryError.code}: {list.retryError.message}</p> : null}
          {!list.error && list.loading && !list.data.length ? <TableSkeleton /> : null}
          {!list.error && !list.loading && !list.data.length ? <EmptyState title="Chưa có batch" description="Tạo batch gửi đầu tiên để bắt đầu theo dõi." /> : null}
          {list.data.length ? (
            <BatchTable
              batches={list.data}
              clock={list.clock}
              retryingBatchId={list.retryingBatchId}
              onRetry={retryFailed}
            />
          ) : null}
        </OutputPanel>
      }
    />
  )
}
