import {
  Bell,
  BookOpen,
  CalendarClock,
  GraduationCap,
  Images,
  Library,
  ListTodo,
  Mail,
  Send,
  Timer,
  Activity,
  Upload,
  UploadCloud,
  Download,
  Users,
} from 'lucide-react'
import { NavLink, useLocation, useNavigate } from 'react-router-dom'
import { Dropdown } from '@/components/ui/Dropdown'
import { Icon } from '@/components/ui/Icon'
import { readAdmin } from '@/auth/actorStorage'
import { ENV } from '@/constants/env'
import { ICON } from '@/constants/icons'
import { LAYOUT } from '@/constants/layout'
import {
  SERVICES,
  getServiceById,
  getServiceByPath,
  getMenuByPath,
} from '@/constants/appRoutes'
import { UI_LABELS } from '@/constants/ui'
import { ui } from '@/theme'

const serviceIcons = {
  student: GraduationCap,
  course: BookOpen,
  media: Images,
  notification: Bell,
  scheduler: CalendarClock,
}

const menuIcons = {
  students: Users,
  courses: Library,
  courseExport: Download,
  upload: Upload,
  uploadDirect: UploadCloud,
  messages: Mail,
  send: Send,
  progress: Activity,
  batches: ListTodo,
  jobs: Timer,
}

export function Sidebar() {
  const location = useLocation()
  const navigate = useNavigate()
  const activeService = getServiceByPath(location.pathname)
  const activeMenu = getMenuByPath(location.pathname)
  const admin = readAdmin()

  return (
    <aside
      className={ui.sidebar}
      style={{ width: LAYOUT.sidebarWidthPx }}
    >
      <div className={`${ui.sidebarHeader} px-5 py-5`}>
        <p
          className={`font-display text-[11px] font-medium tracking-[0.22em] uppercase ${ui.eyebrow}`}
        >
          Sổ lớp
        </p>
        <h1
          className={`mt-2 flex items-center gap-2 font-display text-[20px] leading-none font-semibold ${ui.title}`}
        >
          <Icon
            icon={BookOpen}
            size={ICON.size.md}
            className={ui.brand}
          />
          {ENV.appName}
        </h1>
      </div>

      <div className={`${ui.sidebarService} px-3 py-3`}>
        <Dropdown
          label={UI_LABELS.service}
          className="w-full min-w-0"
          value={activeService.id}
          options={SERVICES.map((service) => ({
            value: service.id,
            label: service.label,
          }))}
          onChange={(serviceId) => {
            const service = getServiceById(serviceId)
            navigate(service.menus[0].to)
          }}
        />
      </div>

      <nav className={`${ui.sidebarMenu} flex flex-col gap-1 p-3`}>
        <p
          className={`px-3 pb-1 font-display text-[11px] font-medium tracking-[0.18em] uppercase ${ui.eyebrow}`}
        >
          {UI_LABELS.menu}
        </p>
        {activeService.menus.map((item) => {
          const MenuIcon = menuIcons[item.icon] ?? serviceIcons[activeService.icon]

          return (
            <NavLink
              key={item.to}
              to={item.to}
              className={() =>
                [
                  'flex cursor-pointer items-center gap-2 rounded-md px-3 py-2 text-[14px] font-medium',
                  activeMenu.to === item.to ? ui.navActive : ui.navIdle,
                ].join(' ')
              }
            >
              <Icon icon={MenuIcon} />
              {item.label}
            </NavLink>
          )
        })}
      </nav>

      <div className={`${ui.sidebarFooter} px-5 py-4 text-[12px] ${ui.caption}`}>
        <p className="font-medium text-fg">{admin.displayName}</p>
        <p className="mt-1 break-all font-mono text-[10px]">{admin.id}</p>
      </div>
    </aside>
  )
}
