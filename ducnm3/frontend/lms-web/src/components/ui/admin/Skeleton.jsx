import { adminUi } from '@/theme/admin'

export function Skeleton({ className = '' }) {
  return (
    <span
      aria-hidden="true"
      className={['skeleton-pulse block rounded-md', adminUi.skeleton, className].join(
        ' ',
      )}
    />
  )
}

export function TableSkeleton({ rows = 6, columns = 4 }) {
  return (
    <div className={`overflow-hidden rounded-lg ${adminUi.card}`}>
      <div className={`grid grid-cols-4 gap-4 px-4 py-3 ${adminUi.hairlineB} ${adminUi.tableHead}`}>
        {Array.from({ length: columns }).map((_, index) => (
          <Skeleton key={`head-${index}`} className="h-3 w-24" />
        ))}
      </div>
      <div>
        {Array.from({ length: rows }).map((_, rowIndex) => (
          <div
            key={`row-${rowIndex}`}
            className={`grid grid-cols-4 gap-4 px-4 py-3 ${adminUi.hairlineT}`}
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
