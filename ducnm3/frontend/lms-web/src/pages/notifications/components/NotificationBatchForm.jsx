import { Button } from '@/components/ui/admin/Button'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { FieldLabel, TextInput } from '@/components/ui/admin/Field'
import { NOTIFICATION_BATCH_FIELDS } from '@/constants/notification'
import { adminUi } from '@/theme/admin'
import { NotificationMarkdownEditor } from './NotificationMarkdownEditor'

export function NotificationBatchForm({
  query,
  loading,
  onChange,
  onSubmit,
}) {
  return (
    <form className="flex h-full min-h-0 flex-col" onSubmit={(event) => { event.preventDefault(); onSubmit() }}>
      <div className="flex min-h-0 flex-1 flex-col gap-4 overflow-y-auto px-5 py-4">
        <div className="flex flex-col gap-1"><FieldLabel htmlFor="batch-title">Tiêu đề</FieldLabel><TextInput id="batch-title" name={NOTIFICATION_BATCH_FIELDS.title} value={query.title ?? ''} disabled={loading} placeholder="Thông báo khóa học" onChange={(event) => onChange({ ...query, title: event.target.value })} /></div>
        <NotificationMarkdownEditor
          value={query[NOTIFICATION_BATCH_FIELDS.bodyMarkdown] ?? ''}
          disabled={loading}
          onChange={(bodyMarkdown) => onChange({ ...query, bodyMarkdown })}
        />
        <Dropdown label="Phạm vi gửi" className="w-full" value={query.targetScope ?? ''} disabled={loading} options={[{ value: 'ALL_STUDENTS', label: 'Tất cả học viên đang hoạt động' }]} onChange={(targetScope) => onChange({ ...query, targetScope })} />
        <div className="flex flex-col gap-1"><FieldLabel htmlFor="batch-size">Kích thước chunk</FieldLabel><TextInput id="batch-size" name={NOTIFICATION_BATCH_FIELDS.batchSize} type="number" value={query.batchSize ?? ''} disabled={loading} onChange={(event) => onChange({ ...query, batchSize: Number(event.target.value) })} /></div>
        <div className="flex flex-col gap-1"><FieldLabel htmlFor="batch-requested-count" hint="Để trống để gửi toàn bộ học viên đang hoạt động.">Số người nhận</FieldLabel><TextInput id="batch-requested-count" name={NOTIFICATION_BATCH_FIELDS.requestedCount} type="number" min="1" max="100000" value={query.requestedCount ?? ''} disabled={loading} placeholder="Tất cả" onChange={(event) => onChange({ ...query, requestedCount: event.target.value === '' ? null : Number(event.target.value) })} /></div>
      </div>
      <div className={`shrink-0 px-5 py-3 ${adminUi.hairlineT}`}><Button type="submit" disabled={loading}>Tạo batch gửi</Button></div>
    </form>
  )
}
