export function JudgePanel({ title, subtitle, icon: Icon, actions, children, className = "" }) {
  return (
    <section
      className={`rounded-2xl border bg-white p-5 ${className}`}
      style={{ borderColor: "#E5E7EB", boxShadow: "0 10px 30px rgba(0,0,0,0.02)" }}
    >
      {(title || actions) && (
        <div className="mb-5 flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div className="flex items-start gap-3">
            {Icon && (
              <div
                className="flex h-9 w-9 items-center justify-center rounded-lg"
                style={{ background: "rgba(242,111,33,0.1)", border: "1px solid rgba(242,111,33,0.2)" }}
              >
                <Icon className="h-4 w-4" style={{ color: "#F26F21" }} />
              </div>
            )}
            <div>
              {title && <h3 className="font-bold text-slate-900">{title}</h3>}
              {subtitle && <p className="mt-1 text-sm text-slate-500">{subtitle}</p>}
            </div>
          </div>
          {actions && <div className="flex flex-wrap gap-2">{actions}</div>}
        </div>
      )}
      {children}
    </section>
  );
}
