import { UI_LABELS } from '@/constants/ui'
import { adminUi } from '@/theme/admin'

function formatDefault(value) {
  if (value == null || value === '') {
    return UI_LABELS.noFilter
  }

  return String(value)
}

export function ApiField({ field, children }) {
  return (
    <div className={`rounded-md ${adminUi.card} p-3`}>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <p className={`font-mono text-[13px] font-medium ${adminUi.title}`}>{field.key}</p>
        <span
          className={[
            'inline-flex rounded-full px-2 py-0.5 text-[11px] font-medium',
            field.nullable ? adminUi.badgeMuted : adminUi.badgeWarning,
          ].join(' ')}
        >
          {field.nullable ? UI_LABELS.nullable : UI_LABELS.notNull}
        </span>
      </div>
      <p className={`mt-1 text-[12px] ${adminUi.body}`}>
        {field.label} · {field.type} ·{' '}
        {field.required ? UI_LABELS.requiredField : UI_LABELS.optional}
      </p>
      <p className={`mt-1 text-[12px] ${adminUi.caption}`}>
        {UI_LABELS.defaultValue}: {formatDefault(field.defaultValue)}
      </p>
      {field.allowlist ? (
        <p className={`mt-1 font-mono text-[11px] ${adminUi.caption}`}>
          {UI_LABELS.allowlist}: {field.allowlist.join(' | ')}
        </p>
      ) : null}
      {field.hint ? (
        <p className={`mt-1 text-[12px] ${adminUi.body}`}>{field.hint}</p>
      ) : null}
      <div className="mt-2">{children}</div>
    </div>
  )
}
