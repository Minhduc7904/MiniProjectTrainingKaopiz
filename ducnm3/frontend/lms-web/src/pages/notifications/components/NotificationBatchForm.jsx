import { Button } from '@/components/ui/Button'
import { Dropdown } from '@/components/ui/Dropdown'
import { FieldLabel, TextInput } from '@/components/ui/Field'
import { NOTIFICATION_BATCH_FIELDS } from '@/constants/notification'
import { ui } from '@/theme'

export function NotificationBatchForm({ query, loading, onChange, onSubmit }) {
  return (
    <form className="flex h-full min-h-0 flex-col" onSubmit={(event) => { event.preventDefault(); onSubmit() }}>
      <div className="flex min-h-0 flex-1 flex-col gap-4 overflow-y-auto px-5 py-4">
        <div className="flex flex-col gap-1"><FieldLabel htmlFor="batch-title">Tiêu đề</FieldLabel><TextInput id="batch-title" name={NOTIFICATION_BATCH_FIELDS.title} value={query.title ?? ''} disabled={loading} placeholder="Thông báo khóa học" onChange={(event) => onChange({ ...query, title: event.target.value })} /></div>
        <div className="flex flex-col gap-1"><FieldLabel htmlFor="batch-body">Nội dung Markdown</FieldLabel><textarea id="batch-body" name={NOTIFICATION_BATCH_FIELDS.bodyMarkdown} value={query.bodyMarkdown ?? ''} disabled={loading} placeholder="Nội dung gửi cho học viên..." onChange={(event) => onChange({ ...query, bodyMarkdown: event.target.value })} className={`${ui.control} h-36 resize-y py-2`} /></div>
        <Dropdown label="Phạm vi gửi" className="w-full" value={query.targetScope ?? ''} disabled={loading} options={[{ value: 'ALL_STUDENTS', label: 'Tất cả học viên đang hoạt động' }]} onChange={(targetScope) => onChange({ ...query, targetScope })} />
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2"><div className="flex flex-col gap-1"><FieldLabel htmlFor="batch-size">Kích thước chunk</FieldLabel><TextInput id="batch-size" name={NOTIFICATION_BATCH_FIELDS.batchSize} type="number" value={query.batchSize ?? ''} disabled={loading} onChange={(event) => onChange({ ...query, batchSize: Number(event.target.value) })} /></div><div className="flex flex-col gap-1"><FieldLabel htmlFor="batch-created-by" hint="UUID được tạo sẵn cho lần gửi này.">Người tạo</FieldLabel><TextInput id="batch-created-by" name={NOTIFICATION_BATCH_FIELDS.createdBy} value={query.createdBy ?? ''} disabled={loading} onChange={(event) => onChange({ ...query, createdBy: event.target.value })} /></div></div>
      </div>
      <div className={`shrink-0 px-5 py-3 ${ui.hairlineT}`}><Button type="submit" disabled={loading}>Tạo batch gửi</Button></div>
    </form>
  )
}
