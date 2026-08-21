import { ApiField } from '@/components/ui/admin/ApiField'
import { Button } from '@/components/ui/admin/Button'
import { TextInput } from '@/components/ui/admin/Field'
import { GET_NOTIFICATION_BATCH_INPUT_FIELDS } from '@/constants/inputs/getNotificationBatch'
import { adminUi } from '@/theme/admin'

export function NotificationBatchProgressManualForm({ query, loading, onChange, onSubmit }) { return <form className="flex h-full min-h-0 flex-col" onSubmit={(event) => { event.preventDefault(); onSubmit() }}><div className="min-h-0 flex-1 overflow-y-auto px-5 py-4"><p className={`mb-3 text-[13px] ${adminUi.body}`}>GET không có request body; Batch ID là path parameter.</p><ApiField field={GET_NOTIFICATION_BATCH_INPUT_FIELDS[0]}><TextInput id="manual-batch-id" name="batchId" value={query.batchId ?? ''} disabled={loading} onChange={(event) => onChange(event.target.value)} /></ApiField></div><div className={`shrink-0 px-5 py-3 ${adminUi.hairlineT}`}><Button type="submit" disabled={!query.batchId.trim()}>Gọi API</Button></div></form> }
