export function MentorStatCard({ label, value, icon: Icon, tone = "orange", delta, helper }) {
  const toneMap = {
    orange: ["rgba(242,111,33,0.1)", "rgba(242,111,33,0.2)", "#F26F21"],
    blue: ["rgba(37,99,235,0.08)", "rgba(37,99,235,0.18)", "#2563EB"],
    green: ["rgba(5,150,105,0.08)", "rgba(5,150,105,0.18)", "#059669"],
    amber: ["rgba(217,119,6,0.08)", "rgba(217,119,6,0.18)", "#D97706"],
    red: ["rgba(220,38,38,0.08)", "rgba(220,38,38,0.18)", "#DC2626"],
  };
  const [bg, border, color] = toneMap[tone] || toneMap.orange;

  return (
    <div className="rounded-2xl border bg-white p-5" style={{ borderColor: "#E5E7EB", boxShadow: "0 10px 30px rgba(0,0,0,0.02)" }}>
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-sm font-semibold text-slate-500">{label}</p>
          <p className="mt-2 text-3xl font-bold text-slate-900">{value}</p>
        </div>
        {Icon && (
          <div className="flex h-11 w-11 items-center justify-center rounded-xl" style={{ background: bg, border: `1px solid ${border}` }}>
            <Icon className="h-5 w-5" style={{ color }} />
          </div>
        )}
      </div>
      {(delta || helper) && (
        <div className="mt-4 flex items-center justify-between gap-2 text-xs">
          {delta && <span className="font-bold" style={{ color }}>{delta}</span>}
          {helper && <span className="text-slate-500">{helper}</span>}
        </div>
      )}
    </div>
  );
}
