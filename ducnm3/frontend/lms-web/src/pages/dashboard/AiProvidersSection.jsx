import { Bot, RefreshCw } from 'lucide-react'
import { Link } from 'react-router-dom'
import { AiHealthBadge } from '@/components/ai/AiHealthBadge'
import { Button } from '@/components/ui/admin/Button'
import { Icon } from '@/components/ui/admin/Icon'
import { AI_PROVIDER_TYPE_LABELS, AI_STATUS_LABELS } from '@/constants/ai'
import { APP_ROUTES } from '@/constants/appRoutes'
import { adminUi } from '@/theme/admin'

function ProviderCard({ provider, loading }) {
  return (
    <article className={`rounded-lg p-5 ${adminUi.card}`}>
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h3 className={`truncate text-[15px] font-semibold ${adminUi.title}`}>{provider.name}</h3>
          <p className={`mt-1 text-[12px] ${adminUi.caption}`}>
            {AI_PROVIDER_TYPE_LABELS[provider.providerType] ?? provider.providerType}
          </p>
        </div>
        <AiHealthBadge check={provider} loading={loading} />
      </div>
      <p className={`mt-3 truncate font-mono text-[11px] ${adminUi.caption}`} title={provider.baseUrl}>{provider.baseUrl}</p>
      <p className={`mt-3 text-[12px] ${adminUi.body}`}>
        Cấu hình: {AI_STATUS_LABELS[provider.providerStatus] ?? provider.providerStatus}
      </p>
    </article>
  )
}

// Danh sách AI provider kèm trạng thái kết nối; dữ liệu do một request thử kết nối trả về cùng lúc.
export function AiProvidersSection({ request, onRefresh }) {
  const providers = request.data ?? []
  return (
    <section className="mt-10">
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-center gap-2">
          <Icon icon={Bot} className={adminUi.brand} />
          <div>
            <h2 className={`font-display text-[19px] font-semibold ${adminUi.title}`}>AI provider</h2>
            <p className={`mt-1 text-[12px] ${adminUi.caption}`}>Mỗi lần mở dashboard server gọi thử từng provider bằng API key đã lưu.</p>
          </div>
        </div>
        <Button size="sm" variant="ghost" disabled={request.loading} onClick={onRefresh}>
          <Icon icon={RefreshCw} size={14} />
          Kiểm tra lại
        </Button>
      </div>
      {request.error ? (
        <p role="alert" className={`mt-4 text-[13px] ${adminUi.dangerText}`}>
          Không tải được danh sách AI: {request.error.message ?? request.error.code}
        </p>
      ) : null}
      {!request.error && !request.loading && request.success && providers.length === 0 ? (
        <p className={`mt-4 text-[13px] ${adminUi.body}`}>
          Chưa có AI provider nào. <Link className={adminUi.brand} to={APP_ROUTES.aiProviders}>Tạo provider đầu tiên</Link>
        </p>
      ) : null}
      {providers.length > 0 ? (
        <div className="mt-4 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {providers.map((provider) => <ProviderCard key={provider.providerId} provider={provider} loading={request.loading} />)}
        </div>
      ) : null}
    </section>
  )
}
