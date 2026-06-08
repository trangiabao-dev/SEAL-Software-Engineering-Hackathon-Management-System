export function CommentInput({ value, disabled, onChange }) {
  return (
    <textarea
      value={value ?? ""}
      disabled={disabled}
      onChange={(event) => onChange(event.target.value)}
      rows={3}
      className="w-full resize-none rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none transition focus:border-orange-300 focus:ring-2 focus:ring-orange-100 disabled:bg-slate-100 disabled:text-slate-400"
      placeholder="Optional comment"
    />
  );
}
