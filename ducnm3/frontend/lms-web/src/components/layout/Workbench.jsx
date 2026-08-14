import { ui } from '@/theme'

export function Workbench({ input, output }) {
  return (
    <div className={ui.workbench}>
      <section className={`${ui.panel} ${ui.panelSplit}`}>
        <div className="flex min-h-0 min-w-0 flex-1 flex-col">{input}</div>
      </section>
      <section className={ui.panel}>
        <div className="flex min-h-0 min-w-0 flex-1 flex-col">{output}</div>
      </section>
    </div>
  )
}
