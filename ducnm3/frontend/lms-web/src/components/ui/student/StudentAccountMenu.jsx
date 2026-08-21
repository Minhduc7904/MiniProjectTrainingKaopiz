import { ChevronDown, LogOut, UserRound } from 'lucide-react'
import { Link } from 'react-router-dom'
import { studentUi } from '@/theme/student'

function initials(displayName) {
  return displayName
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase()
}

export function StudentAccountMenu({ displayName, email, profileTo, logoutTo }) {
  return (
    <details className="group relative">
      <summary className={studentUi.accountTrigger}>
        <span aria-hidden="true" className={studentUi.accountAvatar}>{initials(displayName)}</span>
        <span className="min-w-0 flex-1">
          <span className={`block ${studentUi.accountName}`}>{displayName}</span>
          <span className={`block ${studentUi.accountRole}`}>{email}</span>
        </span>
        <ChevronDown aria-hidden="true" className={studentUi.accountChevron} size={16} />
      </summary>
      <nav aria-label="Tài khoản Student" className={studentUi.accountMenu}>
        <Link className={studentUi.accountMenuLink} to={profileTo}>
          <UserRound aria-hidden="true" className={studentUi.accountMenuIcon} size={18} />
          Xem thông tin người dùng
        </Link>
        <Link className={studentUi.accountMenuLink} to={logoutTo}>
          <LogOut aria-hidden="true" className={studentUi.accountLogoutIcon} size={18} />
          Đăng xuất
        </Link>
      </nav>
    </details>
  )
}
