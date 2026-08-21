import { STUDENT_STATUS, STUDENT_STATUS_LABELS } from '@/constants/studentStatus'
import { adminUi } from '@/theme/admin'

const toneByStatus = {
  [STUDENT_STATUS.active]: adminUi.badgeSuccess,
  [STUDENT_STATUS.inactive]: adminUi.badgeWarning,
  [STUDENT_STATUS.blocked]: adminUi.badgeDanger,
}

export function StatusBadge({ status }) {
  return (
    <span
      className={[
        'inline-flex rounded-full px-2 py-0.5 text-[12px] font-medium',
        toneByStatus[status] ?? adminUi.badgeMuted,
      ].join(' ')}
    >
      {STUDENT_STATUS_LABELS[status] ?? status}
    </span>
  )
}
