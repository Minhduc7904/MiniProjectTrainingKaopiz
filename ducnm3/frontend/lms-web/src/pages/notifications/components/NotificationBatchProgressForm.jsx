import { Button } from '@/components/ui/Button'
import { FieldLabel, TextInput } from '@/components/ui/Field'
import { ui } from '@/theme'

export function NotificationBatchProgressForm({ query, loading, onChange, onSubmit }) {
  return <form className="flex h-full min-h-0 flex-col" onSubmit={(event) => { event.preventDefault(); onSubmit() }}><div className="min-h-0 flex-1 overflow-y-auto px-5 py-4"><div className="flex flex-col gap-1"><FieldLabel htmlFor="batch-id" hint="Dán ID trong response 202 hoặc Location để theo dõi.">Batch ID</FieldLabel><TextInput id="batch-id" name="batchId" value={query.batchId ?? ''} disabled={loading} placeholder="UUID" onChange={(event) => onChange(event.target.value)} /></div><p className={`mt-5 rounded-md ${ui.badgeWarning} px-3 py-2 text-[13px]`}>Tự gọi lại mỗi 3 giây khi batch chưa hoàn tất. Pause chỉ dừng polling trên trình duyệt, không dừng Worker.</p></div><div className={`shrink-0 px-5 py-3 ${ui.hairlineT}`}><Button type="submit" disabled={!query.batchId.trim()}>Xem tiến trình</Button></div></form>
}
