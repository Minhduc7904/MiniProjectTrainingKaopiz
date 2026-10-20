import { AI_HEALTH_HINTS, AI_HEALTH_LABELS, AI_HEALTH_STATUS, AI_COPY } from '@/constants/ai'
import { adminUi } from '@/theme/admin'

const toneByStatus = {
  [AI_HEALTH_STATUS.online]: adminUi.badgeSuccess,
  [AI_HEALTH_STATUS.authFailed]: adminUi.badgeDanger,
  [AI_HEALTH_STATUS.unreachable]: adminUi.badgeDanger,
  [AI_HEALTH_STATUS.error]: adminUi.badgeDanger,
  [AI_HEALTH_STATUS.disabled]: adminUi.badgeMuted,
}

// Trạng thái kết nối của một AI provider; `check` null nghĩa là chưa có kết quả, `loading` là đang thử.
export function AiHealthBadge({ check, loading = false }) {
  const status = check?.healthStatus
  const label = loading ? AI_COPY.checking : status ? (AI_HEALTH_LABELS[status] ?? status) : 'Chưa kiểm tra'
  const tone = loading || !status ? adminUi.badgeWarning : (toneByStatus[status] ?? adminUi.badgeMuted)
  const title = status ? (AI_HEALTH_HINTS[status] ?? check.message) : undefined
  return (
    <span className="inline-flex flex-col items-start gap-0.5">
      <span
        title={title}
        aria-live="polite"
        className={['inline-flex rounded-full px-2 py-0.5 text-[12px] font-medium', tone].join(' ')}
      >
        {label}
      </span>
      {!loading && check?.latencyMs != null ? (
        <span className={`font-mono text-[11px] ${adminUi.caption}`}>
          {check.httpStatus ? `HTTP ${check.httpStatus} · ` : ''}
          {check.latencyMs} ms
        </span>
      ) : null}
    </span>
  )
}
