import { ui } from '@/theme'

function formatJson(value) {
  if (value === undefined) {
    return 'undefined'
  }

  return JSON.stringify(value, null, 2)
}

export function JsonView({ value }) {
  const source = formatJson(value)
  const lines = source.split('\n')

  return (
    <div className={`h-full overflow-auto ${ui.code}`}>
      <table className="w-full border-collapse">
        <tbody>
          {lines.map((line, index) => (
            <tr key={`line-${index + 1}`}>
              <td
                className={`w-10 px-3 py-0 align-top ${ui.codeGutter}`}
              >
                {index + 1}
              </td>
              <td className="whitespace-pre px-3 py-0">{line || ' '}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
