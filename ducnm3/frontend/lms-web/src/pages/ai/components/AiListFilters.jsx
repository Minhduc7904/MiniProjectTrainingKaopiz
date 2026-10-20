import { Search } from 'lucide-react'
import { Button } from '@/components/ui/admin/Button'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { FieldLabel, TextInput } from '@/components/ui/admin/Field'
import { Icon } from '@/components/ui/admin/Icon'
import { AI_COPY, AI_STATUS, AI_STATUS_LABELS } from '@/constants/ai'
import { QUERY_PARAMS } from '@/constants/queryParams'
import { UI_LABELS } from '@/constants/ui'

const STATUS_OPTIONS = [
  { value: '', label: UI_LABELS.all },
  { value: AI_STATUS.active, label: AI_STATUS_LABELS[AI_STATUS.active] },
  { value: AI_STATUS.inactive, label: AI_STATUS_LABELS[AI_STATUS.inactive] },
]

// extraFilters: các Dropdown riêng của từng trang (vd lọc theo provider), đặt cạnh ô trạng thái.
export function AiListFilters({
  idPrefix,
  query,
  loading,
  searchHint,
  searchPlaceholder,
  extraFilters = null,
  onChange,
  onSubmit,
}) {
  return (
    <form
      className="flex flex-col gap-3"
      onSubmit={(event) => {
        event.preventDefault()
        onSubmit({ ...query, page: 1 })
      }}
    >
      <FieldLabel htmlFor={`${idPrefix}-search`} hint={searchHint}>
        {AI_COPY.search}
      </FieldLabel>
      <div className="flex flex-col gap-2 sm:flex-row">
        <TextInput
          id={`${idPrefix}-search`}
          name={QUERY_PARAMS.search}
          value={query.search ?? ''}
          disabled={loading}
          placeholder={searchPlaceholder}
          onChange={(event) => onChange({ ...query, search: event.target.value || undefined, page: 1 })}
        />
        <Button type="submit" disabled={loading} aria-label="Tìm kiếm">
          <Icon icon={Search} />
          Tìm
        </Button>
      </div>
      <div className="flex flex-col gap-3 sm:flex-row">
        <Dropdown
          id={`${idPrefix}-status`}
          label={AI_COPY.status}
          value={query.status ?? ''}
          options={STATUS_OPTIONS}
          disabled={loading}
          onChange={(status) => onSubmit({ ...query, status: status || undefined, page: 1 })}
        />
        {extraFilters}
      </div>
    </form>
  )
}
