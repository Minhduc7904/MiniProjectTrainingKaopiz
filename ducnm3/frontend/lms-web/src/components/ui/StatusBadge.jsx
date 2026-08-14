import { STUDENT_STATUS, STUDENT_STATUS_LABELS } from '@/constants/studentStatus'
import { ui } from '@/theme'

const toneByStatus = {
  [STUDENT_STATUS.active]: ui.badgeSuccess,
  [STUDENT_STATUS.inactive]: ui.badgeWarning,
  [STUDENT_STATUS.blocked]: ui.badgeDanger,
}

export function StatusBadge({ status }) {
  return (
    <span
      className={[
        'inline-flex rounded-full px-2 py-0.5 text-[12px] font-medium',
        toneByStatus[status] ?? ui.badgeMuted,
      ].join(' ')}
    >
      {STUDENT_STATUS_LABELS[status] ?? status}
    </span>
  )
}
