import { ApiField } from '@/components/ui/ApiField'
import { Button } from '@/components/ui/Button'
import { TextInput } from '@/components/ui/Field'
import { GET_STUDENTS_INPUT_FIELDS } from '@/constants/inputs/getStudents'
import { UI_LABELS } from '@/constants/ui'
import { ui } from '@/theme'

function fieldValue(query, field) {
  const value = query[field.key]
  return value == null ? '' : String(value)
}

function nextQuery(query, field, raw) {
  const next = { ...query }

  if (raw.trim() === '') {
    if (field.nullable) {
      delete next[field.key]
      return next
    }

    next[field.key] = ''
    return next
  }

  if (field.type === 'integer') {
    const parsed = Number(raw)
    next[field.key] = Number.isFinite(parsed) ? parsed : raw
    return next
  }

  next[field.key] = raw
  return next
}

export function StudentsManualForm({ query, loading, onChange, onSubmit }) {
  return (
    <form
      className="flex h-full min-h-0 flex-col"
      onSubmit={(event) => {
        event.preventDefault()
        onSubmit(query)
      }}
    >
      <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">
        <p className={`mb-3 text-[13px] ${ui.body}`}>{UI_LABELS.queryOnly}</p>
        <p className={`mb-4 text-[12px] ${ui.caption}`}>{UI_LABELS.omitDefault}</p>
        <div className="flex flex-col gap-3">
          {GET_STUDENTS_INPUT_FIELDS.map((field) => (
            <ApiField key={field.key} field={field}>
              <TextInput
                id={`manual-${field.key}`}
                name={field.key}
                value={fieldValue(query, field)}
                disabled={loading}
                placeholder={
                  field.allowlist
                    ? field.allowlist.join(' | ')
                    : String(field.defaultValue ?? '')
                }
                onChange={(event) =>
                  onChange(nextQuery(query, field, event.target.value))
                }
              />
            </ApiField>
          ))}
        </div>
      </div>
      <div className={`shrink-0 px-5 py-3 ${ui.hairlineT}`}>
        <Button type="submit" disabled={loading}>
          {UI_LABELS.callApi}
        </Button>
      </div>
    </form>
  )
}
