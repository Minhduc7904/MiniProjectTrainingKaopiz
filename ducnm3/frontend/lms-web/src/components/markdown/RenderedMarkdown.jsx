import { ui } from '@/theme'

export function RenderedMarkdown({ html, emptyLabel = 'Chưa có nội dung.' }) {
  if (!html) return <p className={`text-[14px] ${ui.body}`}>{emptyLabel}</p>
  return <div className={ui.markdown} dangerouslySetInnerHTML={{ __html: html }} />
}
