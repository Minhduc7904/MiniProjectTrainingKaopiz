import { adminUi } from '@/theme/admin'

export function Workbench({ input, output, className = adminUi.workbench }) {
  return (
    <div className={className}>
      <section className={`${adminUi.panel} ${adminUi.panelSplit}`}>
        <div className="flex min-h-0 min-w-0 flex-1 flex-col">{input}</div>
      </section>
      <section className={adminUi.panel}>
        <div className="flex min-h-0 min-w-0 flex-1 flex-col">{output}</div>
      </section>
    </div>
  )
}
