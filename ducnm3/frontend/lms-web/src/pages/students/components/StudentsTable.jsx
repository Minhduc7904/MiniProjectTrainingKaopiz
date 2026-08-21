import { StatusBadge } from '@/components/ui/admin/StatusBadge'
import { adminUi } from '@/theme/admin'

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
    <div className={`overflow-hidden rounded-lg ${adminUi.card}`}>
      <table className="w-full border-collapse text-left text-[14px]">
        <thead
          className={`text-[12px] font-medium tracking-wide uppercase ${adminUi.tableHead}`}
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
            <tr key={student.id} className={`${adminUi.rail} ${adminUi.tableRow}`}>
              <td className={`px-4 py-3 font-medium ${adminUi.tableCellStrong}`}>
                {student.displayName}
              </td>
              <td className={`px-4 py-3 ${adminUi.tableCell}`}>{student.email}</td>
              <td className="px-4 py-3">
                <StatusBadge status={student.status} />
              </td>
              <td className={`px-4 py-3 ${adminUi.tableCell}`}>
                {formatCreatedAt(student.createdAtUtc)}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
