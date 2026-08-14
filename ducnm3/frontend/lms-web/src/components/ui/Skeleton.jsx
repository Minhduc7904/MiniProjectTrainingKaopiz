import { ui } from '@/theme'

export function Skeleton({ className = '' }) {
  return (
    <span
      aria-hidden="true"
      className={['skeleton-pulse block rounded-md', ui.skeleton, className].join(
        ' ',
      )}
    />
  )
}

export function TableSkeleton({ rows = 6, columns = 4 }) {
  return (
    <div className={`overflow-hidden rounded-lg ${ui.card}`}>
      <div className={`grid grid-cols-4 gap-4 px-4 py-3 ${ui.hairlineB} ${ui.tableHead}`}>
        {Array.from({ length: columns }).map((_, index) => (
          <Skeleton key={`head-${index}`} className="h-3 w-24" />
        ))}
      </div>
      <div>
        {Array.from({ length: rows }).map((_, rowIndex) => (
          <div
            key={`row-${rowIndex}`}
            className={`grid grid-cols-4 gap-4 px-4 py-3 ${ui.hairlineT}`}
          >
            {Array.from({ length: columns }).map((__, columnIndex) => (
              <Skeleton
                key={`cell-${rowIndex}-${columnIndex}`}
                className="h-4 w-full"
              />
            ))}
          </div>
        ))}
      </div>
    </div>
  )
}
