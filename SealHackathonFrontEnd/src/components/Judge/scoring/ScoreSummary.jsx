import { JudgeBadge } from "../shared/JudgeBadge";

export function ScoreSummary({ submission, criteria }) {
  const completed = criteria.filter((criterion) => submission.scores?.[criterion.id] !== undefined && submission.scores?.[criterion.id] !== "").length;
  const total = submission.totalScore ?? "—";

  return (
    <div className="rounded-xl border border-slate-100 bg-slate-50 p-4">
      <div className="flex items-center justify-between gap-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Score summary</p>
          <p className="mt-1 text-2xl font-bold text-slate-900">{total}</p>
        </div>
        <JudgeBadge tone={submission.status === "Scored" || submission.status === "Locked" ? "success" : "warning"}>{completed}/{criteria.length} criteria</JudgeBadge>
      </div>
    </div>
  );
}
