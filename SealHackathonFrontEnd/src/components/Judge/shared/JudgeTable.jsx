export function JudgeTable({ columns, rows, renderCell, emptyMessage = "No records found" }) {
  return (
    <div className="overflow-x-auto rounded-xl border border-slate-100">
      <table className="min-w-full divide-y divide-slate-100 text-sm">
        <thead className="bg-slate-50">
          <tr>
            {columns.map((column) => (
              <th key={column.key} className="whitespace-nowrap px-4 py-3 text-left text-xs font-bold uppercase tracking-wider text-slate-500">
                {column.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100 bg-white">
          {rows.length === 0 ? (
            <tr>
              <td colSpan={columns.length} className="px-4 py-8 text-center text-sm text-slate-500">{emptyMessage}</td>
            </tr>
          ) : rows.map((row) => (
            <tr key={row.id} className="transition-colors hover:bg-orange-50/30">
              {columns.map((column) => (
                <td key={column.key} className="whitespace-nowrap px-4 py-4 text-slate-600">
                  {renderCell ? renderCell(row, column.key) : row[column.key]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
