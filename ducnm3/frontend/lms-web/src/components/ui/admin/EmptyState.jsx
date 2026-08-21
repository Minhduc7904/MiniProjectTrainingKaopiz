import { ClipboardList } from 'lucide-react'
import { Icon } from '@/components/ui/admin/Icon'
import { ICON } from '@/constants/icons'
import { adminUi } from '@/theme/admin'

export function EmptyState({ title, description }) {
  return (
    <div className={`${adminUi.rail} rounded-lg ${adminUi.card} px-6 py-10 text-center`}>
      <Icon icon={ClipboardList} size={ICON.size.lg} className={adminUi.emptyIcon} />
      <p className={`mt-3 font-display text-[18px] font-semibold ${adminUi.title}`}>
        {title}
      </p>
      {description ? (
        <p className={`mt-2 text-[14px] ${adminUi.body}`}>{description}</p>
      ) : null}
    </div>
  )
}
