import {
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
} from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Dropdown } from '@/components/ui/Dropdown'
import { Icon } from '@/components/ui/Icon'
import { PAGINATION } from '@/constants/pagination'
import { UI_LABELS } from '@/constants/ui'
import { ui } from '@/theme'

export function Pagination({
  page,
  totalPages,
  totalItems,
  pageSize,
  pageSizeOptions = PAGINATION.pageSizeOptions,
  loading = false,
  itemLabel = UI_LABELS.items,
  onPageChange,
  onPageSizeChange,
}) {
  const currentPage = page || PAGINATION.defaults.page
  const lastPage = totalPages || 0
  const size = pageSize || PAGINATION.defaults.pageSize
  const atStart = loading || currentPage <= 1
  const atEnd = loading || lastPage === 0 || currentPage >= lastPage

  return (
    <div className="flex flex-col gap-3">
      <p className={`text-[13px] tabular-nums ${ui.body}`}>
        {UI_LABELS.page} {currentPage}/{lastPage || 1} · {totalItems || 0}{' '}
        {itemLabel}
      </p>
      <Dropdown
        label={UI_LABELS.pageSize}
        placement="top"
        className="w-full min-w-0"
        value={size}
        disabled={loading}
        options={pageSizeOptions.map((option) => ({
          value: option,
          label: String(option),
        }))}
        onChange={(nextSize) => onPageSizeChange(Number(nextSize))}
      />
      <div className="flex items-center gap-1">
        <Button
          variant="ghost"
          size="icon"
          disabled={atStart}
          aria-label={UI_LABELS.firstPage}
          onClick={() => onPageChange(1)}
        >
          <Icon icon={ChevronsLeft} />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          disabled={atStart}
          aria-label={UI_LABELS.previous}
          onClick={() => onPageChange(currentPage - 1)}
        >
          <Icon icon={ChevronLeft} />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          disabled={atEnd}
          aria-label={UI_LABELS.next}
          onClick={() => onPageChange(currentPage + 1)}
        >
          <Icon icon={ChevronRight} />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          disabled={atEnd}
          aria-label={UI_LABELS.lastPage}
          onClick={() => onPageChange(lastPage)}
        >
          <Icon icon={ChevronsRight} />
        </Button>
      </div>
    </div>
  )
}
