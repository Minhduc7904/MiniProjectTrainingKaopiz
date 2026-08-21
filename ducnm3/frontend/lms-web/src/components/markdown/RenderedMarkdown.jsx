import { adminUi } from '@/theme/admin'

export function RenderedMarkdown({ html, emptyLabel = 'Chưa có nội dung.' }) {
  if (!html) return <p className={`text-[14px] ${adminUi.body}`}>{emptyLabel}</p>
  return <div className={adminUi.markdown} dangerouslySetInnerHTML={{ __html: html }} />
}
