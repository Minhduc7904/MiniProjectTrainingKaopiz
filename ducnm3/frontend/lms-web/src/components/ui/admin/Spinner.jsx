import { LoaderCircle } from 'lucide-react'
import { Icon } from '@/components/ui/admin/Icon'
import { ICON } from '@/constants/icons'
import { adminUi } from '@/theme/admin'

export function Spinner({ size = ICON.size.sm }) {
  return <Icon icon={LoaderCircle} size={size} className={adminUi.spinner} />
}
