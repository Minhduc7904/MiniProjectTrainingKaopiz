import { AI_PROVIDER_TYPE_LABELS } from '@/constants/ai'
import { adminUi } from '@/theme/admin'
import { formatAiDate } from '../aiOutput'
import { AiRowActions } from './AiRowActions'
import { AiStatusBadge } from './AiStatusBadge'

export function AiProvidersTable({ rows, busy, onEdit, onDelete }) {
  return (
    <div className={`overflow-x-auto rounded-lg ${adminUi.card}`}>
      <table className="w-full border-collapse text-left text-[14px]">
        <thead className={`text-[12px] font-medium tracking-wide uppercase ${adminUi.tableHead}`}>
          <tr>
            <th className="px-4 py-3">Provider</th>
            <th className="px-4 py-3">Loại AI</th>
            <th className="px-4 py-3">Base URL</th>
            <th className="px-4 py-3">API key</th>
            <th className="px-4 py-3">Trạng thái</th>
            <th className="px-4 py-3">Cập nhật</th>
            <th className="px-4 py-3" />
          </tr>
        </thead>
        <tbody>
          {rows.map((provider) => (
            <tr key={provider.id} className={`${adminUi.rail} ${adminUi.tableRow}`}>
              <td className={`px-4 py-3 font-medium ${adminUi.tableCellStrong}`}>{provider.name}</td>
              <td className={`px-4 py-3 ${adminUi.tableCell}`}>
                {AI_PROVIDER_TYPE_LABELS[provider.providerType] ?? provider.providerType}
              </td>
              <td className={`max-w-[260px] truncate px-4 py-3 font-mono text-[12px] ${adminUi.tableCell}`}>
                {provider.baseUrl}
              </td>
              <td className={`px-4 py-3 font-mono text-[12px] ${adminUi.tableCell}`}>{provider.apiKeyHint}</td>
              <td className="px-4 py-3">
                <AiStatusBadge status={provider.status} />
              </td>
              <td className={`px-4 py-3 ${adminUi.tableCell}`}>{formatAiDate(provider.updatedAtUtc)}</td>
              <td className="px-4 py-3">
                <AiRowActions
                  label={provider.name}
                  disabled={busy}
                  onEdit={() => onEdit(provider)}
                  onDelete={() => onDelete(provider)}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
