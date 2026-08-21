import { RotateCcw } from 'lucide-react'
import { PageHeader } from '@/components/ui/admin/PageHeader'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { LoadingState } from '@/components/ui/admin/LoadingState'
import { Pagination } from '@/components/ui/admin/Pagination'
import { TableSkeleton } from '@/components/ui/admin/Skeleton'
import { Button } from '@/components/ui/admin/Button'
import { Icon } from '@/components/ui/admin/Icon'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { useStudentsList } from '@/hooks/students/useStudentsList'
import { GET_STUDENTS_ACTIVITY } from '@/constants/activities/getStudents'
import { STUDENT_COPY } from '@/constants/studentCopy'
import { UI_LABELS } from '@/constants/ui'
import { adminUi } from '@/theme/admin'
import { StudentsFilters } from './components/StudentsFilters'
import { StudentsManualForm } from './components/StudentsManualForm'
import { StudentsTable } from './components/StudentsTable'

function toOutputJson({ data, pagination, traceId, error, success, query }) {
  if (error) {
    return {
      data: null,
      error,
      meta: { traceId, query },
    }
  }

  return {
    data,
    meta: {
      pagination,
      traceId,
      query,
    },
    success,
  }
}

export function StudentsPage() {
  const {
    data,
    pagination,
    query,
    loading,
    success,
    error,
    traceId,
    setQuery,
    load,
    reset,
  } = useStudentsList()

  return (
    <Workbench
      input={
        <InputPanel
          actions={
            <Button variant="ghost" disabled={loading} onClick={reset}>
              <Icon icon={RotateCcw} />
              {UI_LABELS.reset}
            </Button>
          }
          guided={
            <div className="flex h-full min-h-0 flex-col">
              <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">
                <PageHeader
                  eyebrow="Student"
                  title="Danh sách học viên"
                  description="Chọn bộ lọc và trang, rồi đọc kết quả ở Output."
                />
                <StudentsFilters query={query} loading={loading} onChange={load} />
              </div>
              <div className={`shrink-0 px-5 py-3 ${adminUi.hairlineT}`}>
                <Pagination
                  page={query.page}
                  totalPages={pagination.totalPages}
                  totalItems={pagination.totalItems}
                  pageSize={query.pageSize}
                  loading={loading}
                  itemLabel={STUDENT_COPY.item}
                  onPageChange={(page) => load({ ...query, page })}
                  onPageSizeChange={(pageSize) =>
                    load({
                      ...query,
                      page: 1,
                      pageSize,
                    })
                  }
                />
              </div>
            </div>
          }
          manual={
            <StudentsManualForm
              query={query}
              loading={loading}
              onChange={setQuery}
              onSubmit={load}
            />
          }
        />
      }
      output={
        <OutputPanel
          json={toOutputJson({
            data,
            pagination,
            traceId,
            error,
            success,
            query,
          })}
          activity={GET_STUDENTS_ACTIVITY}
          run={{ loading, success, error }}
        >
          {error ? (
            <EmptyState
              title="Không mở được sổ"
              description={`${error.code}: ${error.message}`}
            />
          ) : null}
          {!error && loading && data.length === 0 ? <TableSkeleton /> : null}
          {!error && success && !loading && data.length === 0 ? (
            <EmptyState
              title="Sổ trống"
              description="Chưa có học viên khớp bộ lọc hiện tại."
            />
          ) : null}
          {!error && data.length > 0 ? <StudentsTable rows={data} /> : null}
          {!error && loading && data.length > 0 ? (
            <div className="mt-3">
              <LoadingState />
            </div>
          ) : null}
        </OutputPanel>
      }
    />
  )
}
