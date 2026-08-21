import { adminUi } from '@/theme/admin'

function formatDate(value) {
  return value ? new Intl.DateTimeFormat('vi-VN', { dateStyle: 'short', timeStyle: 'medium' }).format(new Date(value)) : '—'
}

function badgeTone(status) {
  if (status === 'COMPLETED') return adminUi.badgeSuccess
  if (status === 'FAILED' || status === 'PARTIAL_FAILED') return adminUi.badgeDanger
  if (status === 'QUEUED' || status === 'PROCESSING') return adminUi.badgeWarning
  return adminUi.badgeMuted
}

export function MediaJobsTable({ rows }) {
  return <div className="overflow-x-auto rounded-lg border border-line"><table className="w-full min-w-[1120px] text-left text-[13px]">
    <thead className={adminUi.tableHead}><tr><th className="px-3 py-3">Job</th><th className="px-3 py-3">Trạng thái</th><th className="px-3 py-3">Tiến độ</th><th className="px-3 py-3">Subject</th><th className="px-3 py-3">Lần chạy</th><th className="px-3 py-3">Cập nhật</th><th className="px-3 py-3">Lỗi gần nhất</th></tr></thead>
    <tbody>{rows.map((job) => <tr key={job.id} className={adminUi.tableRow}>
      <td className="max-w-64 px-3 py-3"><p className={`font-medium ${adminUi.title}`}>{job.jobType}</p><p className={`mt-1 font-mono text-[10px] ${adminUi.caption}`}>{job.id}</p>{job.correlationId ? <p className={`mt-1 font-mono text-[10px] ${adminUi.caption}`}>corr: {job.correlationId}</p> : null}</td>
      <td className="px-3 py-3"><span className={`rounded-full px-2 py-1 text-[11px] ${badgeTone(job.status)}`}>{job.status}</span></td>
      <td className="px-3 py-3 font-mono tabular-nums">{job.expectedItemCount == null ? '—' : `${(job.processedItemCount + job.failedItemCount).toLocaleString()} / ${job.expectedItemCount.toLocaleString()}`}<p className={`mt-1 text-[11px] ${adminUi.caption}`}>{job.progressPercent == null ? 'Chưa xác định' : `${job.progressPercent}%`} · lỗi {job.failedItemCount.toLocaleString()}</p></td>
      <td className="max-w-52 px-3 py-3"><p className={adminUi.tableCellStrong}>{job.subjectType}</p><p className={`mt-1 truncate font-mono text-[10px] ${adminUi.caption}`}>{job.subjectId}</p></td>
      <td className="px-3 py-3 font-mono tabular-nums">{job.attemptCount}</td>
      <td className="px-3 py-3 whitespace-nowrap font-mono text-[11px]">{formatDate(job.updatedAtUtc)}</td>
      <td className={`max-w-72 px-3 py-3 ${job.lastError ? adminUi.dangerText : adminUi.tableCell}`}>{job.lastError ?? '—'}</td>
    </tr>)}</tbody>
  </table></div>
}
