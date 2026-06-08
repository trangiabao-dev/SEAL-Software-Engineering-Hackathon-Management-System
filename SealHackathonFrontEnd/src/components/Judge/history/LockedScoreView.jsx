import { JudgeModal } from "../shared/JudgeModal";
import { LockNotice } from "../scoring/LockNotice";

export function LockedScoreView({ record, criteria, onClose }) {
  const locked = record.ranking?.calculatedAt != null || record.status === "Locked";

  return (
    <JudgeModal title="Score summary" subtitle={`${record.teamName} • ${record.university}`} onClose={onClose}>
      <div className="space-y-4">
        {locked && <LockNotice />}
        <div className="rounded-xl bg-slate-50 p-4">
          <p className="text-xs font-bold uppercase tracking-wide text-slate-500">SubmittedAt</p>
          <p className="mt-1 font-bold text-slate-900">{record.scoredAt || "—"}</p>
        </div>
        <div className="space-y-3">
          {criteria.map((criterion) => (
            <div key={criterion.id} className="rounded-xl border border-slate-100 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="font-bold text-slate-900">{criterion.name}</p>
                  <p className="mt-1 text-sm text-slate-500">{record.comments?.[criterion.id] || "No comment submitted."}</p>
                </div>
                <p className="text-lg font-bold text-slate-900">{record.scores?.[criterion.id] ?? "—"}/{criterion.maxScore}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </JudgeModal>
  );
}
