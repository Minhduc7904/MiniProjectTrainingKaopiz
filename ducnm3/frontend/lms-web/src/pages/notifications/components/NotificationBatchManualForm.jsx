import { ApiField } from '@/components/ui/admin/ApiField'
import { Button } from '@/components/ui/admin/Button'
import { TextInput } from '@/components/ui/admin/Field'
import { POST_NOTIFICATION_BATCH_INPUT_FIELDS } from '@/constants/inputs/postNotificationBatch'
import { adminUi } from '@/theme/admin'

export function NotificationBatchManualForm({ query, loading, onChange, onSubmit }) {
  const fieldValue = (field, value) => field.type === 'integer'
    ? (value === '' && field.nullable ? null : Number(value))
    : value
  return <form className="flex h-full min-h-0 flex-col" onSubmit={(event) => { event.preventDefault(); onSubmit() }}><div className="min-h-0 flex-1 overflow-y-auto px-5 py-4"><p className={`mb-3 text-[13px] ${adminUi.body}`}>Payload JSON. Scope chỉ nhận <code>ALL_STUDENTS</code>; API trả 202 ngay sau durable handoff.</p><div className="flex flex-col gap-3">{POST_NOTIFICATION_BATCH_INPUT_FIELDS.map((field) => <ApiField key={field.key} field={field}>{field.key === 'bodyMarkdown' ? <textarea id={`manual-${field.key}`} name={field.key} value={query[field.key] ?? ''} disabled={loading} onChange={(event) => onChange({ ...query, [field.key]: event.target.value })} className={`${adminUi.control} h-32 resize-y py-2`} /> : <TextInput id={`manual-${field.key}`} name={field.key} type={field.type === 'integer' ? 'number' : 'text'} value={query[field.key] ?? ''} disabled={loading} placeholder={field.allowlist?.join(' | ') ?? String(field.defaultValue ?? '')} onChange={(event) => onChange({ ...query, [field.key]: fieldValue(field, event.target.value) })} />}</ApiField>)}</div></div><div className={`shrink-0 px-5 py-3 ${adminUi.hairlineT}`}><Button type="submit" disabled={loading}>Gọi API</Button></div></form>
}
