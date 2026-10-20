import { AI_PROVIDER_TYPE_LABELS } from '@/constants/ai'
import { adminUi } from '@/theme/admin'
import { formatAiDate } from '../aiOutput'
import { AiRowActions } from './AiRowActions'
import { AiStatusBadge } from './AiStatusBadge'

export function AiBindingsTable({ rows, busy, onEdit, onDelete }) {
  return (
    <div className={`overflow-x-auto rounded-lg ${adminUi.card}`}>
      <table className="w-full border-collapse text-left text-[14px]">
        <thead className={`text-[12px] font-medium tracking-wide uppercase ${adminUi.tableHead}`}>
          <tr>
            <th className="px-4 py-3">Service</th>
            <th className="px-4 py-3">Model</th>
            <th className="px-4 py-3">Provider</th>
            <th className="px-4 py-3">Trạng thái model</th>
            <th className="px-4 py-3">Cập nhật</th>
            <th className="px-4 py-3" />
          </tr>
        </thead>
        <tbody>
          {rows.map((binding) => (
            <tr key={binding.id} className={`${adminUi.rail} ${adminUi.tableRow}`}>
              <td className={`px-4 py-3 font-medium ${adminUi.tableCellStrong}`}>{binding.serviceName}</td>
              <td className={`px-4 py-3 ${adminUi.tableCell}`}>
                {binding.modelDisplayName}
                <span className={`ml-1.5 font-mono text-[12px] ${adminUi.caption}`}>{binding.modelName}</span>
              </td>
              <td className={`px-4 py-3 ${adminUi.tableCell}`}>
                {binding.providerName}
                <span className={`ml-1.5 text-[12px] ${adminUi.caption}`}>
                  {AI_PROVIDER_TYPE_LABELS[binding.providerType] ?? binding.providerType}
                </span>
              </td>
              <td className="px-4 py-3">
                <AiStatusBadge status={binding.modelStatus} />
              </td>
              <td className={`px-4 py-3 ${adminUi.tableCell}`}>{formatAiDate(binding.updatedAtUtc)}</td>
              <td className="px-4 py-3">
                <AiRowActions
                  label={binding.serviceName}
                  disabled={busy}
                  onEdit={() => onEdit(binding)}
                  onDelete={() => onDelete(binding)}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
