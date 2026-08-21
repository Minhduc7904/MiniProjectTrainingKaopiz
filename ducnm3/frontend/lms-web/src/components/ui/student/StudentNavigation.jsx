import { NavLink } from 'react-router-dom'
import { BookOpen, House } from 'lucide-react'
import { APP_ROUTES } from '@/constants/appRoutes'
import { studentUi } from '@/theme/student'

const navigationItems = [
  { to: APP_ROUTES.studentHome, label: 'Home', icon: House },
  { to: APP_ROUTES.studentCourses, label: 'Khóa học', icon: BookOpen },
]

export function StudentNavigation() {
  return (
    <nav aria-label="Điều hướng Student" className={studentUi.navigation}>
      {navigationItems.map(({ to, label, icon: Icon }) => (
        <NavLink key={to} to={to} className={({ isActive }) => `${studentUi.navigationLink} ${isActive ? studentUi.navigationLinkActive : ''}`}>
          <Icon aria-hidden="true" size={16} strokeWidth={2} />
          <span>{label}</span>
        </NavLink>
      ))}
    </nav>
  )
}
