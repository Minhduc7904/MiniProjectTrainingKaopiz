import { useLocation } from 'react-router-dom'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { PageHeader } from '@/components/ui/admin/PageHeader'
import { getActivityById } from '@/constants/activities'
import { getMenuByPath, getServiceByPath } from '@/constants/appRoutes'

export function PlaceholderPage() {
  const { pathname } = useLocation()
  const service = getServiceByPath(pathname)
  const menu = getMenuByPath(pathname)

  return (
    <Workbench
      input={
        <InputPanel
          guided={
            <div className="h-full overflow-y-auto px-5 py-4">
              <PageHeader
                eyebrow={service.label}
                title={menu.label}
                description="Service này chưa gắn form Input."
              />
            </div>
          }
          manual={
            <div className="h-full overflow-y-auto px-5 py-4">
              <EmptyState
                title="Chưa có mẫu thủ công"
                description="API của menu này chưa khai báo field Input."
              />
            </div>
          }
        />
      }
      output={
        <OutputPanel json={null} activity={getActivityById(menu.activityId)}>
          <EmptyState
            title="Chưa có Output"
            description={`${service.label} / ${menu.label} sẽ hiện JSON và bảng khi API sẵn sàng.`}
          />
        </OutputPanel>
      }
    />
  )
}
