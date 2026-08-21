import { Dropdown } from '@/components/ui/admin/Dropdown'
import { MEDIA_JOB_COPY, MEDIA_JOB_STATUS, MEDIA_JOB_TYPE } from '@/constants/mediaJobs'
import { FieldLabel, TextInput } from '@/components/ui/admin/Field'

const selectOptions = (values) => [
  { value: '', label: 'Tất cả' },
  ...Object.values(values).map((value) => ({ value, label: value })),
]

export function MediaJobsFilters({ query, loading, onChange }) {
  const update = (field, value) => onChange({ ...query, [field]: value, page: 1 })
  return (
    <div className="flex flex-col gap-3">
      <Dropdown label={MEDIA_JOB_COPY.jobType} value={query.jobType} disabled={loading} options={selectOptions(MEDIA_JOB_TYPE)} onChange={(value) => update('jobType', value)} />
      <Dropdown label={MEDIA_JOB_COPY.status} value={query.status} disabled={loading} options={selectOptions(MEDIA_JOB_STATUS)} onChange={(value) => update('status', value)} />
      <div className="flex flex-col gap-1"><FieldLabel htmlFor="media-job-correlation-id">{MEDIA_JOB_COPY.correlationId}</FieldLabel><TextInput id="media-job-correlation-id" value={query.correlationId} disabled={loading} placeholder="UUID" onChange={(event) => update('correlationId', event.target.value)} /></div>
    </div>
  )
}
