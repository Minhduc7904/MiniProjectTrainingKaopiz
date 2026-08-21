import { useState } from 'react'
import { Braces, Table2, Workflow } from 'lucide-react'
import { ActivityDiagram } from '@/components/ui/admin/ActivityDiagram'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { JsonView } from '@/components/ui/admin/JsonView'
import { Tabs } from '@/components/ui/admin/Tabs'
import { OUTPUT_TAB_LABELS, OUTPUT_TABS } from '@/constants/outputTabs'
import { UI_LABELS } from '@/constants/ui'
import { adminUi } from '@/theme/admin'

const TABS = [
  { id: OUTPUT_TABS.json, label: OUTPUT_TAB_LABELS[OUTPUT_TABS.json], icon: Braces },
  { id: OUTPUT_TABS.view, label: OUTPUT_TAB_LABELS[OUTPUT_TABS.view], icon: Table2 },
  { id: OUTPUT_TABS.uml, label: OUTPUT_TAB_LABELS[OUTPUT_TABS.uml], icon: Workflow },
]

export function OutputPanel({ json, activity, run, children }) {
  const [tab, setTab] = useState(OUTPUT_TABS.view)

  return (
    <>
      <header className={`flex shrink-0 items-center justify-between px-3 ${adminUi.hairlineB}`}>
        <p
          className={`px-2 py-3 font-display text-[11px] font-medium tracking-[0.18em] uppercase ${adminUi.eyebrow}`}
        >
          {UI_LABELS.output}
        </p>
        <Tabs tabs={TABS} value={tab} onChange={setTab} />
      </header>
      <div className="min-h-0 flex-1 overflow-hidden">
        {tab === OUTPUT_TABS.json ? <JsonView value={json} /> : null}
        {tab === OUTPUT_TABS.view ? (
          <div className="h-full overflow-auto px-5 py-4">{children}</div>
        ) : null}
        {tab === OUTPUT_TABS.uml ? (
          activity ? (
            <ActivityDiagram diagram={activity} run={run} />
          ) : (
            <div className="h-full overflow-auto px-5 py-4">
              <EmptyState
                title="Chưa có UML"
                description="Menu này chưa gắn Activity Diagram của API."
              />
            </div>
          )
        ) : null}
      </div>
    </>
  )
}
