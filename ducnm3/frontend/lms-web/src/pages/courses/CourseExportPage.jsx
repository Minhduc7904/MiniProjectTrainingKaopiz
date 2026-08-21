import { Download } from 'lucide-react'
import { useRef, useState } from 'react'
import { exportCoursesRequest } from '@/api/coursesApi'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { Button } from '@/components/ui/admin/Button'
import { Dropdown } from '@/components/ui/admin/Dropdown'
import { EmptyState } from '@/components/ui/admin/EmptyState'
import { FieldLabel, TextInput } from '@/components/ui/admin/Field'
import { Icon } from '@/components/ui/admin/Icon'
import { LoadingState } from '@/components/ui/admin/LoadingState'
import { PageHeader } from '@/components/ui/admin/PageHeader'
import { COURSE_STATUS, COURSE_STATUS_LABELS } from '@/constants/courseStatus'
import { EXPORT_COURSES_ACTIVITY } from '@/constants/activities/getCourses'

function formatBytes(value) {
  return `${new Intl.NumberFormat('vi-VN').format(value)} byte`
}

function formatDuration(milliseconds) {
  if (!milliseconds) return '0s'
  const seconds = Math.floor(milliseconds / 1000)
  return `${Math.floor(seconds / 60)}m ${seconds % 60}s`
}

function formatRate(bytes, milliseconds) {
  if (!milliseconds) return '0 byte/s'
  return `${formatBytes(Math.round(bytes / (milliseconds / 1000)))}/s`
}

function parseCsvPreview(csv, limit = 100) {
  const rows = []; let row = []; let value = ''; let quoted = false
  for (let index = 0; index < csv.length && rows.length < limit; index += 1) {
    const char = csv[index]
    if (char === '"') {
      if (quoted && csv[index + 1] === '"') { value += '"'; index += 1 } else quoted = !quoted
    } else if (char === ',' && !quoted) { row.push(value); value = '' }
    else if ((char === '\n' || char === '\r') && !quoted) {
      if (char === '\r' && csv[index + 1] === '\n') index += 1
      row.push(value); rows.push(row); row = []; value = ''
    } else value += char
  }
  if (value || row.length) { row.push(value); rows.push(row) }
  return rows
}

export function CourseExportPage() {
  const [status, setStatus] = useState('')
  const [limit, setLimit] = useState(null)
  const [bytesLoaded, setBytesLoaded] = useState(0)
  const [rowsLoaded, setRowsLoaded] = useState(0)
  const [totalBytes, setTotalBytes] = useState(null)
  const [elapsedMs, setElapsedMs] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [preview, setPreview] = useState([])
  const controllerRef = useRef(null)

  const exportCsv = async () => {
    controllerRef.current?.abort()
    const controller = new AbortController()
    controllerRef.current = controller
    setLoading(true)
    setError(null)
    setBytesLoaded(0)
    setRowsLoaded(0)
    setTotalBytes(null)
    setElapsedMs(0)
    setPreview([])
    try {
      const result = await exportCoursesRequest(
        { status: status || undefined, limit: limit ?? undefined, signal: controller.signal },
        ({ bytesLoaded: value, totalBytes: total, rowsLoaded: rows, elapsedMs: elapsed }) => {
          setBytesLoaded(value)
          setTotalBytes(total)
          setRowsLoaded(rows)
          setElapsedMs(elapsed)
        },
      )
      setPreview(parseCsvPreview(await result.blob.text()))
      const url = URL.createObjectURL(result.blob)
      const link = document.createElement('a')
      link.href = url
      link.download = result.filename
      link.click()
      URL.revokeObjectURL(url)
    } catch (cause) {
      if (cause.name !== 'AbortError') {
        setError({ code: 'EXPORT_FAILED', message: 'Không thể tải CSV.' })
      }
    } finally {
      if (controllerRef.current === controller) controllerRef.current = null
      setLoading(false)
    }
  }

  const receivedRecords = Math.max(0, rowsLoaded - 1)
  const query = { ...(status ? { status } : {}), ...(limit != null ? { limit } : {}) }

  return (
    <Workbench
      input={(
        <InputPanel
          guided={(
            <div className="px-5 py-4">
              <PageHeader eyebrow="Course" title="Xuất CSV khóa học" description="Giới hạn số Course và theo dõi tiến trình tải realtime." />
              <div className="mt-5 space-y-4">
                <Dropdown label="Trạng thái" value={status} disabled={loading} options={[{ value: '', label: 'Tất cả' }, ...Object.values(COURSE_STATUS).map((value) => ({ value, label: COURSE_STATUS_LABELS[value] }))]} onChange={setStatus} />
                <div className="flex flex-col gap-1">
                  <FieldLabel htmlFor="course-export-limit" hint="Để trống để export toàn bộ Course.">Số Course</FieldLabel>
                  <TextInput id="course-export-limit" type="number" min="1" max="300000" value={limit ?? ''} disabled={loading} placeholder="Tất cả" onChange={(event) => setLimit(event.target.value === '' ? null : Number(event.target.value))} />
                </div>
              </div>
              <Button className="mt-5" disabled={loading} onClick={exportCsv}><Icon icon={Download} />Xuất CSV</Button>
            </div>
          )}
          manual={<div className="px-5 py-4"><PageHeader eyebrow="GET" title="Export query" description="Chọn filter và giới hạn số Course trước khi tải CSV." /></div>}
        />
      )}
      output={(
        <OutputPanel json={{ data: { bytesLoaded, rowsLoaded: receivedRecords, preview }, error, meta: { query } }} activity={EXPORT_COURSES_ACTIVITY} run={{ loading, success: !loading && bytesLoaded > 0 && !error, error }}>
          <div className="p-5">
            {loading ? (
              <div className="space-y-2">
                <LoadingState label={`Đang tải CSV: ${receivedRecords.toLocaleString()} records · ${formatBytes(bytesLoaded)}`} />
                <p className="text-[12px] text-muted">{formatRate(bytesLoaded, elapsedMs)} · {formatDuration(elapsedMs)}{totalBytes ? ` · ${formatBytes(totalBytes)} total` : ''}</p>
              </div>
            ) : null}
            {error ? <EmptyState title="Xuất CSV thất bại" description={`${error.code}: ${error.message}`} /> : null}
            {!loading && !error && bytesLoaded === 0 ? <EmptyState title="Sẵn sàng xuất CSV" description="Nhập số Course hoặc để trống để export toàn bộ. Tiến trình sẽ hiển thị realtime." /> : null}
            {!loading && !error && preview.length > 0 ? <div><p className="mb-3 text-[13px]">Preview tối đa 100 dòng · {receivedRecords.toLocaleString()} records · {formatBytes(bytesLoaded)}</p><div className="overflow-auto rounded-lg border border-line"><table className="w-full text-left text-[13px]"><thead><tr>{preview[0].map((cell) => <th key={cell} className="px-3 py-2">{cell.replace(/^\uFEFF/, '')}</th>)}</tr></thead><tbody>{preview.slice(1).map((row, index) => <tr key={`${row[0]}-${index}`} className="border-t border-line">{preview[0].map((_, column) => <td key={column} className="px-3 py-2">{row[column]}</td>)}</tr>)}</tbody></table></div></div> : null}
          </div>
        </OutputPanel>
      )}
    />
  )
}
