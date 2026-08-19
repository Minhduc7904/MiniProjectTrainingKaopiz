import { useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { Dropdown } from '@/components/ui/Dropdown'
import { EmptyState } from '@/components/ui/EmptyState'
import { LoadingState } from '@/components/ui/LoadingState'
import { PageHeader } from '@/components/ui/PageHeader'
import { Pagination } from '@/components/ui/Pagination'
import { StatusBadge } from '@/components/ui/StatusBadge'
import { InputPanel } from '@/components/layout/InputPanel'
import { OutputPanel } from '@/components/layout/OutputPanel'
import { Workbench } from '@/components/layout/Workbench'
import { COURSE_STATUS, COURSE_STATUS_LABELS } from '@/constants/courseStatus'
import { COURSE_SORT_BY } from '@/constants/inputs/getCourses'
import { SORT_DIRECTIONS } from '@/constants/queryParams'
import { GET_COURSES_ACTIVITY } from '@/constants/activities/getCourses'
import { fetchCoursesList } from '@/features/courses/coursesSlice'
import { ui } from '@/theme'

export function CoursesPage() {
  const dispatch = useDispatch(); const list = useSelector((state) => state.courses.list); const loaded = useRef(false)
  useEffect(() => { if (!loaded.current) { loaded.current = true; dispatch(fetchCoursesList(list.query)) } }, [dispatch, list.query])
  const load = (patch) => dispatch(fetchCoursesList({ ...list.query, ...patch }))
  return <Workbench input={<InputPanel guided={<div className="flex h-full min-h-0 flex-col"><div className="min-h-0 flex-1 overflow-y-auto px-5 py-4"><PageHeader eyebrow="Course" title="Danh sách khóa học" description="Lọc và phân trang dữ liệu Course." /><div className="mt-5 flex flex-col gap-3"><Dropdown label="Trạng thái" value={list.query.status ?? ''} disabled={list.loading} options={[{ value: '', label: 'Tất cả' }, ...Object.values(COURSE_STATUS).map((value) => ({ value, label: COURSE_STATUS_LABELS[value] }))]} onChange={(status) => load({ status: status || undefined, page: 1 })} /><Dropdown label="Sắp xếp" value={list.query.sortBy} disabled={list.loading} options={[{ value: COURSE_SORT_BY.createdAt, label: 'Ngày tạo' }, { value: COURSE_SORT_BY.name, label: 'Tên khóa học' }]} onChange={(sortBy) => load({ sortBy, page: 1 })} /><Dropdown label="Thứ tự" value={list.query.sortDirection} disabled={list.loading} options={[{ value: SORT_DIRECTIONS.desc, label: 'Giảm dần' }, { value: SORT_DIRECTIONS.asc, label: 'Tăng dần' }]} onChange={(sortDirection) => load({ sortDirection, page: 1 })} /></div></div><div className={`shrink-0 px-5 py-3 ${ui.hairlineT}`}><Pagination {...list.pagination} page={list.query.page} pageSize={list.query.pageSize} loading={list.loading} itemLabel="khóa học" onPageChange={(page) => load({ page })} onPageSizeChange={(pageSize) => load({ page: 1, pageSize })} /></div></div>} manual={<pre className={`m-5 rounded-md p-3 ${ui.code}`}>{JSON.stringify(list.query, null, 2)}</pre>} />} output={<OutputPanel json={{ data: list.data, error: list.error, meta: { pagination: list.pagination, traceId: list.traceId, query: list.query } }} activity={GET_COURSES_ACTIVITY} run={list}><>{list.loading && !list.data.length ? <LoadingState label="Đang tải khóa học…" /> : null}{list.error ? <EmptyState title="Không tải được khóa học" description={list.error.message} /> : null}{!list.loading && !list.error && !list.data.length ? <EmptyState title="Chưa có khóa học" description="Không có dữ liệu phù hợp." /> : null}{list.data.length ? <div className={`overflow-hidden rounded-lg ${ui.card}`}><table className="w-full text-left text-[14px]"><thead className={ui.tableHead}><tr><th className="px-4 py-3">Tên</th><th className="px-4 py-3">Trạng thái</th><th className="px-4 py-3">Ngày tạo</th></tr></thead><tbody>{list.data.map((row) => <tr key={row.id} className={ui.tableRow}><td className="px-4 py-3">{row.name}</td><td className="px-4 py-3"><StatusBadge status={row.status} /></td><td className="px-4 py-3">{new Date(row.createdAtUtc).toLocaleString('vi-VN')}</td></tr>)}</tbody></table></div> : null}</></OutputPanel>} />
}
