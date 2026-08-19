import { Download } from 'lucide-react'
import { useRef, useState } from 'react'
import { exportCoursesRequest } from '@/api/coursesApi'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { Button } from '@/components/ui/Button'
import { Dropdown } from '@/components/ui/Dropdown'
import { EmptyState } from '@/components/ui/EmptyState'
import { Icon } from '@/components/ui/Icon'
import { LoadingState } from '@/components/ui/LoadingState'
import { PageHeader } from '@/components/ui/PageHeader'
import { COURSE_STATUS, COURSE_STATUS_LABELS } from '@/constants/courseStatus'
import { EXPORT_COURSES_ACTIVITY } from '@/constants/activities/getCourses'

function formatBytes(value) {
  return `${new Intl.NumberFormat('vi-VN').format(value)} byte`
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
  const [bytesLoaded, setBytesLoaded] = useState(0)
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
    setPreview([])
    try {
      const result = await exportCoursesRequest(
        { status: status || undefined, signal: controller.signal },
        ({ bytesLoaded: value }) => setBytesLoaded(value),
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

  return (
    <Workbench
      input={<InputPanel guided={<div className="px-5 py-4"><PageHeader eyebrow="Course" title="Xuất CSV khóa học" description="Theo dõi trực tiếp số byte đã nhận trong lúc tải file." /><div className="mt-5"><Dropdown label="Trạng thái" value={status} disabled={loading} options={[{ value: '', label: 'Tất cả' }, ...Object.values(COURSE_STATUS).map((value) => ({ value, label: COURSE_STATUS_LABELS[value] }))]} onChange={setStatus} /></div><Button className="mt-5" disabled={loading} onClick={exportCsv}><Icon icon={Download} />Xuất CSV</Button></div>} manual={<div className="px-5 py-4"><PageHeader eyebrow="GET" title="Export query" description="Chọn filter trong tab Mẫu, sau đó tải CSV." /></div>} />}
      output={<OutputPanel json={{ data: { bytesLoaded, preview }, error, meta: { query: status ? { status } : {} } }} activity={EXPORT_COURSES_ACTIVITY} run={{ loading, success: !loading && bytesLoaded > 0 && !error, error }}><div className="p-5">{loading ? <LoadingState label={`Đang tải CSV: đã nhận ${formatBytes(bytesLoaded)}`} /> : null}{error ? <EmptyState title="Xuất CSV thất bại" description={`${error.code}: ${error.message}`} /> : null}{!loading && !error && bytesLoaded === 0 ? <EmptyState title="Sẵn sàng xuất CSV" description="Số byte nhận được sẽ hiển thị realtime trong lúc tải." /> : null}{!loading && !error && preview.length > 0 ? <div><p className="mb-3 text-[13px]">Preview tối đa 100 dòng · {formatBytes(bytesLoaded)}</p><div className="overflow-auto rounded-lg border border-line"><table className="w-full text-left text-[13px]"><thead><tr>{preview[0].map((cell) => <th key={cell} className="px-3 py-2">{cell.replace(/^\uFEFF/, '')}</th>)}</tr></thead><tbody>{preview.slice(1).map((row, index) => <tr key={`${row[0]}-${index}`} className="border-t border-line">{preview[0].map((_, column) => <td key={column} className="px-3 py-2">{row[column]}</td>)}</tr>)}</tbody></table></div></div> : null}</div></OutputPanel>}
    />
  )
}
