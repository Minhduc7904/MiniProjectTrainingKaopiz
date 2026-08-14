import { useState } from 'react'
import { Keyboard, ListFilter } from 'lucide-react'
import { Tabs } from '@/components/ui/Tabs'
import { INPUT_TAB_LABELS, INPUT_TABS } from '@/constants/inputTabs'
import { UI_LABELS } from '@/constants/ui'
import { ui } from '@/theme'

const TABS = [
  { id: INPUT_TABS.guided, label: INPUT_TAB_LABELS[INPUT_TABS.guided], icon: ListFilter },
  { id: INPUT_TABS.manual, label: INPUT_TAB_LABELS[INPUT_TABS.manual], icon: Keyboard },
]

export function InputPanel({ guided, manual, actions }) {
  const [tab, setTab] = useState(INPUT_TABS.guided)

  return (
    <>
      <header className={`flex shrink-0 items-center justify-between gap-2 px-3 ${ui.hairlineB}`}>
        <p
          className={`px-2 py-3 font-display text-[11px] font-medium tracking-[0.18em] uppercase ${ui.eyebrow}`}
        >
          {UI_LABELS.input}
        </p>
        <div className="flex items-center gap-2">
          {actions}
          <Tabs tabs={TABS} value={tab} onChange={setTab} />
        </div>
      </header>
      <div className="flex min-h-0 flex-1 flex-col overflow-hidden">
        <div
          className={
            tab === INPUT_TABS.guided
              ? 'flex min-h-0 flex-1 flex-col overflow-hidden'
              : 'hidden'
          }
        >
          {guided}
        </div>
        <div
          className={
            tab === INPUT_TABS.manual
              ? 'flex min-h-0 flex-1 flex-col overflow-hidden'
              : 'hidden'
          }
        >
          {manual}
        </div>
      </div>
    </>
  )
}
