import { StatusBadge } from '@/components/ui/StatusBadge'
import { ui } from '@/theme'

function formatCreatedAt(value) {
  if (!value) {
    return '—'
  }

  return new Intl.DateTimeFormat('vi-VN', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function StudentsTable({ rows }) {
  return (
    <div className={`overflow-hidden rounded-lg ${ui.card}`}>
      <table className="w-full border-collapse text-left text-[14px]">
        <thead
          className={`text-[12px] font-medium tracking-wide uppercase ${ui.tableHead}`}
        >
          <tr>
            <th className="px-4 py-3">Học viên</th>
            <th className="px-4 py-3">Email</th>
            <th className="px-4 py-3">Trạng thái</th>
            <th className="px-4 py-3">Ngày vào sổ</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((student) => (
            <tr key={student.id} className={`${ui.rail} ${ui.tableRow}`}>
              <td className={`px-4 py-3 font-medium ${ui.tableCellStrong}`}>
                {student.displayName}
              </td>
              <td className={`px-4 py-3 ${ui.tableCell}`}>{student.email}</td>
              <td className="px-4 py-3">
                <StatusBadge status={student.status} />
              </td>
              <td className={`px-4 py-3 ${ui.tableCell}`}>
                {formatCreatedAt(student.createdAtUtc)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
