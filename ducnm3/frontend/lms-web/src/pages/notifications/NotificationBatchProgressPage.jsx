import { useEffect } from 'react'
import { CheckCircle2, Circle, CircleX, LoaderCircle, Pause, Play, RotateCcw, RefreshCw } from 'lucide-react'
import { useNavigate, useParams } from 'react-router-dom'
import { Button } from '@/components/ui/admin/Button'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { Icon } from '@/components/ui/admin/Icon'
import { LoadingState } from '@/components/ui/admin/LoadingState'
import { PageHeader } from '@/components/ui/admin/PageHeader'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { GET_NOTIFICATION_BATCH_ACTIVITY } from '@/constants/activities/getNotificationBatch'
import { UI_LABELS } from '@/constants/ui'
import { APP_ROUTES, notificationBatchProgressPath } from '@/constants/appRoutes'
import { useNotificationBatchProgress } from '@/hooks/notifications/useNotificationBatchProgress'
import { adminUi } from '@/theme/admin'
import { NotificationBatchProgressForm } from './components/NotificationBatchProgressForm'
import { NotificationBatchProgressManualForm } from './components/NotificationBatchProgressManualForm'

const terminalFailed = new Set(['FAILED', 'PARTIAL_FAILED'])

function outputJson(progress) {
  const data = {
    batchId: progress.activeBatchId,
    currentStep: progress.currentStep,
    snapshot: progress.snapshot,
    delivery: progress.delivery,
    mediaUsage: progress.mediaUsage,
  }
  return progress.error
    ? { data, error: progress.error, meta: { traceId: progress.traceId, query: progress.query } }
    : { data, meta: { traceId: progress.traceId, stepTraceIds: progress.stepTraceIds, query: progress.query, paused: progress.paused }, success: progress.success }
}

function formatDuration(value) {
  if (value == null) return 'Chưa bắt đầu'
  const seconds = Math.floor(value / 1000)
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  return `${hours ? `${hours}h ` : ''}${minutes}m ${seconds % 60}s`
}

function StepIcon({ status }) {
  if (status === 'COMPLETED') return <Icon icon={CheckCircle2} />
  if (terminalFailed.has(status)) return <Icon icon={CircleX} />
  if (status === 'RUNNING' || status === 'PROCESSING') return <Icon icon={LoaderCircle} className="animate-spin" />
  return <Icon icon={Circle} />
}

function StepCard({ code, title, description, data, active }) {
  const status = data?.status ?? 'PENDING'
  const percentage = data?.progressPercent
  const tone = terminalFailed.has(status)
    ? adminUi.badgeDanger
    : status === 'COMPLETED'
      ? adminUi.badgeSuccess
      : active
        ? adminUi.badgeWarning
        : adminUi.badgeMuted
  return (
    <article className={`${active ? adminUi.rail : ''} rounded-lg ${adminUi.card} p-4`}>
      <div className="flex items-start gap-3">
        <span className={`mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-md ${tone}`}>
          <StepIcon status={status} />
        </span>
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div>
              <p className={`text-[11px] font-semibold uppercase tracking-[0.12em] ${adminUi.caption}`}>{code}</p>
              <h3 className={`mt-0.5 text-[15px] font-semibold ${adminUi.title}`}>{title}</h3>
            </div>
            <span className={`rounded-full px-2.5 py-1 text-[11px] font-medium ${tone}`}>{status}</span>
          </div>
          <p className={`mt-2 text-[13px] ${adminUi.body}`}>{description}</p>
          {percentage != null ? (
            <div className="mt-3">
              <div
                className={`h-2 overflow-hidden rounded-full ${adminUi.progressTrack}`}
                role="progressbar"
                aria-label={`${title}: ${percentage}%`}
                aria-valuenow={percentage}
                aria-valuemin="0"
                aria-valuemax="100"
              >
                <div
                  className={`${terminalFailed.has(status) ? adminUi.progressFailed : adminUi.progressFill} h-full rounded-full transition-[width] duration-200 ease-[cubic-bezier(0.23,1,0.32,1)]`}
                  style={{ width: `${percentage}%` }}
                />
              </div>
              <p className={`mt-1 text-right font-mono text-[11px] tabular-nums ${adminUi.caption}`}>{percentage}%</p>
            </div>
          ) : null}
        </div>
      </div>
    </article>
  )
}

function Metric({ label, value, tone }) {
  return (
    <div className={`rounded-md px-2 py-3 text-center ${tone}`}>
      <p className="font-mono text-[18px] font-semibold tabular-nums">{value}</p>
      <p className="mt-1 text-[11px] font-medium">{label}</p>
    </div>
  )
}

function DeliverySummary({ delivery }) {
  if (!delivery) return null
  return (
    <section className={`rounded-lg ${adminUi.card} p-5`}>
      <div>
        <p className={`font-display text-[18px] font-semibold ${adminUi.title}`}>Kết quả gửi notification</p>
        <p className={`mt-1 text-[13px] ${adminUi.body}`}>Counter được cập nhật sau mỗi chunk; không đợi Media Usage hoàn tất.</p>
      </div>
      <div className="mt-4 grid grid-cols-2 gap-2 sm:grid-cols-4">
        <Metric label="Thành công" value={delivery.successCount.toLocaleString()} tone={adminUi.badgeSuccess} />
        <Metric label="Thất bại" value={delivery.failedCount.toLocaleString()} tone={delivery.failedCount ? adminUi.badgeDanger : adminUi.badgeMuted} />
        <Metric label="Còn lại" value={delivery.remainingCount.toLocaleString()} tone={adminUi.badgeMuted} />
        <Metric label="Tổng thời gian" value={formatDuration(delivery.durationMs)} tone={adminUi.badgeMuted} />
      </div>
    </section>
  )
}

function Failures({ progress }) {
  return (
    <section className={`rounded-lg ${adminUi.card} p-5`}>
      <div className="flex flex-wrap items-baseline justify-between gap-3">
        <div>
          <p className={`font-display text-[18px] font-semibold ${adminUi.title}`}>Lỗi theo người nhận</p>
          <p className={`mt-1 text-[13px] ${adminUi.body}`}>Retry tạo batch con chỉ từ item FAILED.</p>
        </div>
        <Button disabled={progress.retryLoading} onClick={progress.onRetry}>
          <Icon icon={RefreshCw} />{progress.retryLoading ? 'Đang retry' : 'Retry lỗi'}
        </Button>
      </div>
      {progress.retryError ? <p className={`mt-4 rounded-md ${adminUi.badgeDanger} px-3 py-2 text-[13px]`}>{progress.retryError.code}: {progress.retryError.message}</p> : null}
      {progress.failuresLoading ? <div className="mt-4"><LoadingState /></div> : null}
      {progress.failuresError ? <p className={`mt-4 rounded-md ${adminUi.badgeDanger} px-3 py-2 text-[13px]`}>{progress.failuresError.code}: {progress.failuresError.message}</p> : null}
      {!progress.failuresLoading && !progress.failuresError && progress.failures.length ? (
        <div className="mt-4 overflow-hidden rounded-md border border-line">
          <table className="w-full text-left text-[13px]">
            <thead className={adminUi.tableHead}><tr><th className="px-3 py-2 font-medium">Student ID</th><th className="px-3 py-2 font-medium">Retry</th><th className="px-3 py-2 font-medium">Lỗi cuối</th></tr></thead>
            <tbody>{progress.failures.map((item) => <tr key={item.studentId} className={adminUi.tableRow}><td className="break-all px-3 py-2 font-mono text-[11px]">{item.studentId}</td><td className="px-3 py-2 font-mono tabular-nums">{item.retryCount}</td><td className="px-3 py-2">{item.errorMessage}</td></tr>)}</tbody>
          </table>
        </div>
      ) : null}
    </section>
  )
}

function BatchProgressView({ progress }) {
  if (!progress.activeBatchId) return <EmptyState title="Chưa chọn batch" description="Dán Batch ID để bắt đầu theo dõi tuần tự từng bước background." />
  const snapshotDescription = progress.snapshot
    ? `${progress.snapshot.snapshotCount.toLocaleString()} recipient đã được snapshot${progress.snapshot.targetCount ? ` / ${progress.snapshot.targetCount.toLocaleString()}` : ''}.`
    : 'Đang chờ Notification Worker tiếp nhận snapshot.'
  const deliveryDescription = progress.delivery
    ? `${progress.delivery.processedCount.toLocaleString()} / ${progress.delivery.totalCount.toLocaleString()} recipient đã xử lý.`
    : 'API delivery chỉ được gọi sau khi snapshot hoàn tất.'
  const mediaDescription = progress.mediaUsage
    ? `${progress.mediaUsage.processedUsageCount.toLocaleString()} Media Usage thành công, ${progress.mediaUsage.failedUsageCount.toLocaleString()} thất bại.`
    : 'API Media Service chỉ được gọi sau khi delivery terminal.'

  return (
    <div className="space-y-4">
      <section className="grid gap-3 xl:grid-cols-3">
        <StepCard code="01" title="Snapshot người nhận" description={snapshotDescription} data={progress.snapshot} active={progress.currentStep === 'snapshot'} />
        <StepCard code="02" title="Gửi notification" description={deliveryDescription} data={progress.delivery} active={progress.currentStep === 'delivery'} />
        <StepCard code="03" title="Đăng ký Media Usage" description={mediaDescription} data={progress.mediaUsage} active={progress.currentStep === 'mediaUsage'} />
      </section>
      <DeliverySummary delivery={progress.delivery} />
      <section className={`rounded-lg ${adminUi.card} p-4`}>
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <p className={`text-[13px] font-medium ${adminUi.title}`}>Polling tuần tự</p>
            <p className={`mt-1 text-[12px] ${adminUi.body}`}>{progress.isTerminal ? 'Toàn bộ pipeline đã terminal; polling tự dừng.' : progress.paused ? 'Polling đang tạm dừng trên trình duyệt.' : `Đang theo dõi bước ${progress.currentStep ?? 'hoàn tất'} mỗi 3 giây.`}</p>
          </div>
          {!progress.isTerminal ? progress.paused ? <Button onClick={progress.resume}><Icon icon={Play} />Tiếp tục</Button> : <Button variant="ghost" onClick={progress.pause}><Icon icon={Pause} />Pause</Button> : null}
        </div>
      </section>
      {progress.delivery?.failedCount > 0 && terminalFailed.has(progress.delivery.status) ? <Failures progress={progress} /> : null}
    </div>
  )
}

export function NotificationBatchProgressPage() {
  const progress = useNotificationBatchProgress()
  const { batchId } = useParams()
  const navigate = useNavigate()
  const { activeBatchId, watch } = progress
  useEffect(() => {
    if (batchId && batchId !== activeBatchId) watch(batchId)
  }, [activeBatchId, batchId, watch])
  const watchFromInput = () => navigate(notificationBatchProgressPath(progress.query.batchId.trim()))
  const reset = () => { progress.reset(); navigate(APP_ROUTES.notificationProgress) }
  const onRetry = async () => {
    const result = await progress.retryFailed(progress.activeBatchId)
    if (result.meta.requestStatus === 'fulfilled') navigate(notificationBatchProgressPath(result.payload.data.id), { replace: true })
  }
  const viewProgress = { ...progress, onRetry }

  return (
    <Workbench
      input={<InputPanel actions={<Button variant="ghost" disabled={progress.loading} onClick={reset}><Icon icon={RotateCcw} />{UI_LABELS.reset}</Button>} guided={<div className="flex h-full min-h-0 flex-col"><div className="shrink-0 px-5 pt-4"><PageHeader eyebrow="Notification · pipeline" title="Chi tiết tiến trình batch" description="Theo dõi tuần tự Snapshot → Delivery → Media Usage theo đúng service sở hữu." /></div><div className="min-h-0 flex-1"><NotificationBatchProgressForm query={progress.query} loading={progress.loading} onChange={progress.setQuery} onSubmit={watchFromInput} /></div></div>} manual={<NotificationBatchProgressManualForm query={progress.query} loading={progress.loading} onChange={progress.setQuery} onSubmit={watchFromInput} />} />}
      output={<OutputPanel json={outputJson(progress)} activity={GET_NOTIFICATION_BATCH_ACTIVITY} run={{ loading: progress.loading, success: progress.success, error: progress.error }}>{progress.error ? <EmptyState title="Không đọc được tiến trình" description={`${progress.error.code}: ${progress.error.message}`} /> : <BatchProgressView progress={viewProgress} />}</OutputPanel>}
    />
  )
}
