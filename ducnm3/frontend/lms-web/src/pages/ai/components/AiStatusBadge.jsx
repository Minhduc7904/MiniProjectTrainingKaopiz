import { AI_STATUS, AI_STATUS_LABELS } from '@/constants/ai'
import { adminUi } from '@/theme/admin'

const toneByStatus = {
  [AI_STATUS.active]: adminUi.badgeSuccess,
  [AI_STATUS.inactive]: adminUi.badgeWarning,
}

export function AiStatusBadge({ status }) {
  return (
    <span
      className={[
        'inline-flex rounded-full px-2 py-0.5 text-[12px] font-medium',
        toneByStatus[status] ?? adminUi.badgeMuted,
      ].join(' ')}
    >
      {AI_STATUS_LABELS[status] ?? status}
    </span>
  )
}
