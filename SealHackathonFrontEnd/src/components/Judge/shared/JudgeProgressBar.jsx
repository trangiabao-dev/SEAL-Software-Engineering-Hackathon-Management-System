export function JudgeProgressBar({ value = 0, label, color = "#F26F21" }) {
  const safeValue = Math.min(100, Math.max(0, Math.round(value)));

  return (
    <div>
      {(label || value !== undefined) && (
        <div className="mb-2 flex items-center justify-between text-sm">
          {label && <span className="font-semibold text-slate-700">{label}</span>}
          <span className="font-bold text-slate-900">{safeValue}%</span>
        </div>
      )}
      <div className="h-2.5 overflow-hidden rounded-full bg-slate-100">
        <div className="h-full rounded-full transition-all" style={{ width: `${safeValue}%`, background: color }} />
      </div>
    </div>
  );
}
