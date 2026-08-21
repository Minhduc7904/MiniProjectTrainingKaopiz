import { ChevronLeft, ChevronRight } from 'lucide-react'
import { studentUi } from '@/theme/student'

export function StudentPagination({ onPageChange, page, totalPages }) {
  if (totalPages <= 1) return null
  return (
    <nav aria-label="Phân trang khóa học" className={studentUi.pagination}>
      <button className={studentUi.paginationButton} type="button" disabled={page <= 1} onClick={() => onPageChange(page - 1)}><ChevronLeft aria-hidden="true" size={18} />Trang trước</button>
      <span className={studentUi.paginationStatus}>Trang {page} / {totalPages}</span>
      <button className={studentUi.paginationButton} type="button" disabled={page >= totalPages} onClick={() => onPageChange(page + 1)}>Trang sau<ChevronRight aria-hidden="true" size={18} /></button>
    </nav>
  )
}
