import { Link } from 'react-router-dom'
import { ArrowRight, CheckCircle2 } from 'lucide-react'
import { Icon } from '@/components/ui/Icon'
import { notificationBatchProgressPath } from '@/constants/appRoutes'
import { ui } from '@/theme'

export function NotificationBatchResult({ batch }) {
  return <section className={`${ui.rail} rounded-lg ${ui.card} p-5`}><div className="flex items-start gap-3"><Icon icon={CheckCircle2} className="mt-0.5 text-success" /><div><p className={`font-display text-[18px] font-semibold ${ui.title}`}>Batch đã được tiếp nhận</p><p className={`mt-1 text-[14px] ${ui.body}`}>Worker sẽ snapshot rồi gửi nền; request HTTP không chờ quá trình này.</p></div></div><dl className="mt-5 grid gap-3 text-[13px]"><div><dt className={ui.caption}>Batch ID</dt><dd className={`mt-1 break-all font-mono ${ui.title}`}>{batch.id}</dd></div><div className="flex justify-between gap-4"><dt className={ui.caption}>Trạng thái ban đầu</dt><dd className={`font-medium ${ui.title}`}>{batch.status}</dd></div></dl><Link to={notificationBatchProgressPath(batch.id)} className={`mt-5 inline-flex cursor-pointer items-center gap-1.5 rounded-md px-3.5 py-2 text-[14px] font-medium ${ui.buttonPrimary}`}><Icon icon={ArrowRight} />Theo dõi tiến trình</Link></section>
}
