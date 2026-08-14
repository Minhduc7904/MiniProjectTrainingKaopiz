import { RotateCcw } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { EmptyState } from '@/components/ui/EmptyState'
import { Icon } from '@/components/ui/Icon'
import { LoadingState } from '@/components/ui/LoadingState'
import { PageHeader } from '@/components/ui/PageHeader'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { POST_NOTIFICATION_BATCH_ACTIVITY } from '@/constants/activities/postNotificationBatch'
import { UI_LABELS } from '@/constants/ui'
import { useNotificationBatchCreate } from '@/hooks/notifications/useNotificationBatchCreate'
import { useMediaUpload } from '@/hooks/media/useMediaUpload'
import { MEDIA_TYPES } from '@/constants/media'
import { NotificationBatchForm } from './components/NotificationBatchForm'
import { NotificationBatchManualForm } from './components/NotificationBatchManualForm'
import { NotificationBatchResult } from './components/NotificationBatchResult'

function toOutputJson({ data, error, success, query, traceId, location }) { return error ? { data: null, error, meta: { traceId, query } } : { data, meta: { traceId, query, location }, success } }

export function NotificationBatchCreatePage() {
  const { data, query, loading, success, error, traceId, location, setQuery, submit, reset } = useNotificationBatchCreate()
  const mediaUpload = useMediaUpload()
  const uploadImage = async (file, mediaQuery) => {
    const result = await mediaUpload.submit({
      file,
      mediaType: MEDIA_TYPES.image,
      uploadedByType: mediaQuery.uploadedByType,
      uploadedBy: String(mediaQuery.uploadedBy ?? '').trim(),
    })
    return result.meta.requestStatus === 'fulfilled' ? result.payload.data : null
  }
  return <Workbench input={<InputPanel actions={<Button variant="ghost" disabled={loading} onClick={reset}><Icon icon={RotateCcw} />{UI_LABELS.reset}</Button>} guided={<div className="flex h-full min-h-0 flex-col"><div className="shrink-0 px-5 pt-4"><PageHeader eyebrow="Notification · batch" title="Gửi thông báo hàng loạt" description="Tạo batch cho tất cả học viên đang hoạt động. Worker xử lý nền theo từng chunk." /></div><div className="min-h-0 flex-1"><NotificationBatchForm query={query} loading={loading} mediaUpload={mediaUpload} onChange={setQuery} onSubmit={() => submit(query)} onUploadImage={uploadImage} /></div></div>} manual={<NotificationBatchManualForm query={query} loading={loading} onChange={setQuery} onSubmit={() => submit(query)} />} />} output={<OutputPanel json={toOutputJson({ data, error, success, query, traceId, location })} activity={POST_NOTIFICATION_BATCH_ACTIVITY} run={{ loading, success, error }}>{error ? <EmptyState title="Không tạo được batch" description={`${error.code}: ${error.message}`} /> : null}{!error && loading ? <LoadingState /> : null}{!error && !loading && !data ? <EmptyState title="Sẵn sàng tạo batch" description="Nhập nội dung, tạo batch, rồi chuyển sang trang tiến trình để quan sát Worker." /> : null}{!error && data ? <NotificationBatchResult batch={data} /> : null}</OutputPanel>} />
}
