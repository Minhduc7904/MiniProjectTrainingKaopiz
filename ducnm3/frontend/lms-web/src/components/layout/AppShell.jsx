import { Sidebar } from '@/components/layout/Sidebar'
import { LAYOUT } from '@/constants/layout'
import { ui } from '@/theme'
import { Outlet } from 'react-router-dom'

export function AppShell() {
  return (
    <div className={`h-full ${ui.page}`}>
      <Sidebar />
      <main
        className="h-full min-w-0"
        style={{ marginLeft: LAYOUT.sidebarWidthPx }}
      >
        <Outlet />
      </main>
    </div>
  )
}
