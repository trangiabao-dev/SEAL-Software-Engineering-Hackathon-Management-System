export function EmptyState({ icon: Icon, title, description }) {
  return (
    <div className="rounded-xl border border-dashed border-slate-200 bg-slate-50 px-4 py-10 text-center">
      {Icon && (
        <div className="mx-auto mb-3 flex h-11 w-11 items-center justify-center rounded-full bg-white text-slate-400">
          <Icon className="h-5 w-5" />
        </div>
      )}
      <p className="font-bold text-slate-800">{title}</p>
      {description && <p className="mx-auto mt-1 max-w-md text-sm text-slate-500">{description}</p>}
    </div>
  );
}
