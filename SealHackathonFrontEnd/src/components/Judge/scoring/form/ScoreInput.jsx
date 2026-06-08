export function ScoreInput({ value, maxScore, disabled, error, onChange }) {
  return (
    <div>
      <input
        type="number"
        min="0"
        max={maxScore}
        step="0.1"
        value={value ?? ""}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value)}
        className="w-full rounded-xl border border-slate-200 px-3 py-2 text-sm font-semibold outline-none transition focus:border-orange-300 focus:ring-2 focus:ring-orange-100 disabled:bg-slate-100 disabled:text-slate-400"
        placeholder={`0 - ${maxScore}`}
      />
      {error && <p className="mt-1 text-xs font-semibold text-red-600">{error}</p>}
    </div>
  );
}
