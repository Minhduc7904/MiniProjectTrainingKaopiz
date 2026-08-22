import {
  Activity,
  Bell,
  BookOpen,
  ChevronRight,
  Database,
  GraduationCap,
  Images,
  RefreshCw,
  ServerCog,
  Timer,
  TriangleAlert,
  Users,
} from 'lucide-react'
import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/admin/Button'
import { Icon } from '@/components/ui/admin/Icon'
import { readAdmin } from '@/auth/actorStorage'
import { APP_ROUTES } from '@/constants/appRoutes'
import { DASHBOARD_HEALTH_SERVICES, DASHBOARD_METRICS } from '@/constants/dashboard'
import { useAdminDashboard } from '@/hooks/dashboard/useAdminDashboard'
import { adminUi } from '@/theme/admin'

const metricIcons = { students: Users, media: Images, courses: BookOpen, lessons: GraduationCap }
const serviceIcons = { student: GraduationCap, course: BookOpen, media: Images, notification: Bell, scheduler: Timer }
const managementServices = [
  { id: 'student', label: 'Student', description: 'Danh sách học viên', to: APP_ROUTES.students },
  { id: 'course', label: 'Course', description: 'Danh sách khóa học', to: APP_ROUTES.courses },
  { id: 'media', label: 'Media', description: 'Upload và quản lý media', to: APP_ROUTES.mediaUpload },
  { id: 'notification', label: 'Notification', description: 'Quản lý notification batch', to: APP_ROUTES.notificationBatches },
  { id: 'scheduler', label: 'Scheduler', description: 'Màn hình quản lý đang được phát triển', disabled: true },
]

function metricValue(metricId, request) {
  if (metricId === 'students') return request.data?.totalStudents
  if (metricId === 'media') return request.data?.totalMedia
  if (metricId === 'courses') return request.data?.totalCourses
  return request.data?.totalLessons
}

function MetricCard({ metric, request }) {
  const MetricIcon = metricIcons[metric.id]
  const value = metricValue(metric.id, request)
  return (
    <article className={`rounded-lg p-5 ${adminUi.dashboardMetric}`}>
      <div className="flex items-start justify-between gap-4">
        <div><p className={`text-[11px] font-medium tracking-[0.16em] uppercase ${adminUi.eyebrow}`}>{metric.label}</p><p className={`mt-3 font-display text-[30px] leading-none font-semibold tabular-nums ${adminUi.title}`}>{request.loading ? '—' : value?.toLocaleString('vi-VN') ?? '—'}</p></div>
        <Icon icon={MetricIcon} size={24} className={adminUi.brand} />
      </div>
      <p className={`mt-4 text-[12px] ${request.error ? adminUi.dangerText : adminUi.caption}`}>{request.error?.message ?? metric.description}</p>
    </article>
  )
}

function dependencyEntries(data) {
  return Object.entries(data ?? {}).filter(([key, value]) => key !== 'service' && key !== 'status' && value && typeof value === 'object' && 'status' in value)
}

function HealthCard({ service, request, onRefresh }) {
  const healthy = request.data?.status?.toLowerCase() === 'healthy'
  const statusClass = request.loading ? adminUi.dashboardHealthPending : healthy ? adminUi.dashboardHealthHealthy : adminUi.dashboardHealthFailed
  return (
    <article className={`rounded-lg p-5 ${adminUi.card}`}>
      <div className="flex items-start justify-between gap-3"><div><h3 className={`text-[15px] font-semibold ${adminUi.title}`}>{service.label}</h3><p className={`mt-1 text-[12px] ${adminUi.caption}`}>{service.route}</p></div><span className={`rounded-full px-2.5 py-1 text-[11px] font-medium ${statusClass}`}>{request.loading ? 'Đang kiểm tra' : request.data?.status ?? request.error?.code ?? 'Không khả dụng'}</span></div>
      <div className={`mt-4 grid grid-cols-2 gap-3 border-y py-3 ${adminUi.hairline}`}><div><p className={`text-[11px] ${adminUi.caption}`}>HTTP</p><p className={`mt-1 font-mono text-[13px] ${adminUi.title}`}>{request.httpStatus ?? '—'}</p></div><div><p className={`text-[11px] ${adminUi.caption}`}>Độ trễ</p><p className={`mt-1 font-mono text-[13px] ${adminUi.title}`}>{request.latencyMs == null ? '—' : `${request.latencyMs} ms`}</p></div></div>
      {dependencyEntries(request.data).length ? <div className="mt-3 space-y-1.5">{dependencyEntries(request.data).map(([key, value]) => <p key={key} className={`flex items-center justify-between gap-2 text-[12px] ${adminUi.body}`}><span className="capitalize">{key}</span><span className={value.status?.toLowerCase() === 'healthy' ? adminUi.accentText : adminUi.dangerText}>{value.status}</span></p>)}</div> : null}
      <p className={`mt-3 min-h-8 text-[11px] ${request.error ? adminUi.dangerText : adminUi.caption}`}>{request.error?.message ?? (request.checkedAt ? `Cập nhật ${new Date(request.checkedAt).toLocaleTimeString('vi-VN')}` : 'Chưa có kết quả kiểm tra.')}</p>
      <div className="mt-3 flex items-center justify-between gap-2"><span className={`truncate font-mono text-[10px] ${adminUi.caption}`}>{request.traceId ? `trace ${request.traceId}` : ''}</span><Button size="sm" variant="ghost" disabled={request.loading} onClick={() => onRefresh(service.id)}><Icon icon={RefreshCw} size={14} />Kiểm tra lại</Button></div>
    </article>
  )
}

export function AdminDashboardPage() {
  const admin = readAdmin()
  const { metrics, health, refresh, refreshHealth } = useAdminDashboard()
  return (
    <main className={adminUi.dashboardPage}>
      <header className={adminUi.dashboardHeader}><div className="mx-auto flex max-w-[1280px] items-center justify-between gap-4 px-5 py-4 sm:px-8"><div><p className={`text-[11px] font-medium tracking-[0.2em] uppercase ${adminUi.eyebrow}`}>Sổ lớp</p><h1 className={`mt-1 font-display text-[22px] font-semibold ${adminUi.title}`}>Dashboard quản trị</h1></div><div className="text-right"><p className={`text-[13px] font-medium ${adminUi.title}`}>{admin.displayName}</p><p className={`mt-1 font-mono text-[10px] ${adminUi.caption}`}>{admin.type} · {admin.id}</p></div></div></header>
      <div className="mx-auto max-w-[1280px] px-5 py-8 sm:px-8">
        <section className={`rounded-lg p-6 sm:p-7 ${adminUi.dashboardHero}`}><div className="flex flex-col justify-between gap-5 sm:flex-row sm:items-end"><div><p className={`text-[11px] font-medium tracking-[0.2em] uppercase ${adminUi.eyebrow}`}>Tổng quan vận hành</p><h2 className={`mt-2 font-display text-[28px] leading-tight font-semibold ${adminUi.title}`}>Nắm tình trạng lớp học trong một lần xem</h2><p className={`mt-2 max-w-[62ch] text-[14px] ${adminUi.body}`}>Theo dõi dữ liệu sở hữu bởi từng service, trạng thái kết nối và đi thẳng đến khu vực quản lý.</p></div><Button disabled={Object.values(health).some((request) => request.loading)} onClick={refresh}><Icon icon={RefreshCw} />Kiểm tra lại toàn bộ</Button></div></section>
        <section className="mt-8"><div className="flex items-center gap-2"><Icon icon={Database} className={adminUi.brand} /><h2 className={`font-display text-[19px] font-semibold ${adminUi.title}`}>Dữ liệu hệ thống</h2></div><div className="mt-4 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{DASHBOARD_METRICS.map((metric) => <MetricCard key={metric.id} metric={metric} request={metrics[metric.id]} />)}</div></section>
        <section className="mt-10"><div className="flex items-center gap-2"><Icon icon={Activity} className={adminUi.brand} /><div><h2 className={`font-display text-[19px] font-semibold ${adminUi.title}`}>Tình trạng service</h2><p className={`mt-1 text-[12px] ${adminUi.caption}`}>Mỗi card gọi health endpoint riêng và hiển thị dependency trả về.</p></div></div><div className="mt-4 grid gap-4 md:grid-cols-2 xl:grid-cols-3">{DASHBOARD_HEALTH_SERVICES.map((service) => <HealthCard key={service.id} service={service} request={health[service.id]} onRefresh={refreshHealth} />)}</div></section>
        <section className="mt-10 pb-10"><div className="flex items-center gap-2"><Icon icon={ServerCog} className={adminUi.brand} /><div><h2 className={`font-display text-[19px] font-semibold ${adminUi.title}`}>Quản lý service</h2><p className={`mt-1 text-[12px] ${adminUi.caption}`}>Chọn service để tiếp tục thao tác quản trị.</p></div></div><div className="mt-4 grid gap-4 md:grid-cols-2 xl:grid-cols-5">{managementServices.map((service) => { const ServiceIcon = serviceIcons[service.id]; const content = <><Icon icon={ServiceIcon} size={22} className={service.disabled ? adminUi.caption : adminUi.brand} /><div className="min-w-0 flex-1"><h3 className={`text-[14px] font-semibold ${adminUi.title}`}>{service.label}</h3><p className={`mt-1 text-[12px] ${adminUi.body}`}>{service.description}</p></div>{service.disabled ? <TriangleAlert size={17} className={adminUi.caption} /> : <ChevronRight size={18} className={adminUi.caption} />}</>; return service.disabled ? <div key={service.id} aria-disabled="true" className={`flex cursor-not-allowed items-start gap-3 rounded-lg p-4 opacity-65 ${adminUi.dashboardManagement}`}>{content}</div> : <Link key={service.id} to={service.to} className={`flex cursor-pointer items-start gap-3 rounded-lg p-4 transition-colors duration-150 ${adminUi.dashboardManagement}`}>{content}</Link> })}</div></section>
      </div>
    </main>
  )
}
