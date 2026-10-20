import { AI_PROVIDER_TYPE_LABELS } from '@/constants/ai'
import { adminUi } from '@/theme/admin'
import { formatAiDate } from '../aiOutput'
import { AiRowActions } from './AiRowActions'
import { AiStatusBadge } from './AiStatusBadge'

export function AiModelsTable({ rows, busy, onEdit, onDelete }) {
  return (
    <div className={`overflow-x-auto rounded-lg ${adminUi.card}`}>
      <table className="w-full border-collapse text-left text-[14px]">
        <thead className={`text-[12px] font-medium tracking-wide uppercase ${adminUi.tableHead}`}>
          <tr>
            <th className="px-4 py-3">Model</th>
            <th className="px-4 py-3">Tên gửi provider</th>
            <th className="px-4 py-3">Provider</th>
            <th className="px-4 py-3">Trạng thái</th>
            <th className="px-4 py-3">Cập nhật</th>
            <th className="px-4 py-3" />
          </tr>
        </thead>
        <tbody>
          {rows.map((model) => (
            <tr key={model.id} className={`${adminUi.rail} ${adminUi.tableRow}`}>
              <td className={`px-4 py-3 font-medium ${adminUi.tableCellStrong}`}>{model.displayName}</td>
              <td className={`px-4 py-3 font-mono text-[12px] ${adminUi.tableCell}`}>{model.modelName}</td>
              <td className={`px-4 py-3 ${adminUi.tableCell}`}>
                {model.providerName}
                <span className={`ml-1.5 text-[12px] ${adminUi.caption}`}>
                  {AI_PROVIDER_TYPE_LABELS[model.providerType] ?? model.providerType}
                </span>
              </td>
              <td className="px-4 py-3">
                <AiStatusBadge status={model.status} />
              </td>
              <td className={`px-4 py-3 ${adminUi.tableCell}`}>{formatAiDate(model.updatedAtUtc)}</td>
              <td className="px-4 py-3">
                <AiRowActions
                  label={model.displayName}
                  disabled={busy}
                  onEdit={() => onEdit(model)}
                  onDelete={() => onDelete(model)}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
