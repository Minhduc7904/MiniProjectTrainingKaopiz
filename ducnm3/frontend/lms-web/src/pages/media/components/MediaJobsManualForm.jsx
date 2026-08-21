import { ApiField } from '@/components/ui/ApiField'
import { Button } from '@/components/ui/Button'
import { TextInput } from '@/components/ui/Field'
import { GET_MEDIA_JOBS_INPUT_FIELDS } from '@/constants/inputs/getMediaJobs'
import { UI_LABELS } from '@/constants/ui'
import { ui } from '@/theme'

function nextQuery(query, field, raw) {
  const next = { ...query }
  if (raw.trim() === '') return field.nullable ? { ...next, [field.key]: '' } : next
  next[field.key] = field.type === 'integer' && Number.isFinite(Number(raw)) ? Number(raw) : raw
  return next
}

export function MediaJobsManualForm({ query, loading, onChange, onSubmit }) {
  return <form className="flex h-full min-h-0 flex-col" onSubmit={(event) => { event.preventDefault(); onSubmit(query) }}>
    <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">
      <p className={`mb-4 text-[13px] ${ui.body}`}>{UI_LABELS.queryOnly}</p>
      <div className="flex flex-col gap-3">{GET_MEDIA_JOBS_INPUT_FIELDS.map((field) => <ApiField key={field.key} field={field}><TextInput id={`manual-${field.key}`} name={field.key} value={query[field.key] == null ? '' : String(query[field.key])} disabled={loading} placeholder={field.allowlist?.join(' | ') ?? String(field.defaultValue ?? '')} onChange={(event) => onChange(nextQuery(query, field, event.target.value))} /></ApiField>)}</div>
    </div>
    <div className={`shrink-0 px-5 py-3 ${ui.hairlineT}`}><Button type="submit" disabled={loading}>{UI_LABELS.callApi}</Button></div>
  </form>
}
