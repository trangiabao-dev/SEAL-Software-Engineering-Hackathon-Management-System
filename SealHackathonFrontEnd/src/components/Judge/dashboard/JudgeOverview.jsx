import { JudgePanel } from "../shared/JudgePanel";
import { JudgeProgressBar } from "../shared/JudgeProgressBar";
import { judgeIcons } from "../shared/judgeIcons";
import { AssignedRounds } from "./AssignedRounds";
import { JudgeStatsCards } from "./JudgeStatsCards";
import { RecentScoringActivity } from "./RecentScoringActivity";

export function JudgeOverview({ submissions, onOpenScoring }) {
  const scored = submissions.filter((item) => item.status === "Scored" || item.status === "Locked").length;
  const progress = submissions.length ? (scored / submissions.length) * 100 : 0;

  return (
    <div className="space-y-6">
      <JudgeStatsCards submissions={submissions} />
      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <div className="xl:col-span-2"><AssignedRounds onOpenScoring={onOpenScoring} /></div>
        <JudgePanel title="Scoring Progress" subtitle="Overall completion across assigned submissions" icon={judgeIcons.Trophy}>
          <JudgeProgressBar value={progress} label="Completed" />
          <div className="mt-5 grid grid-cols-2 gap-3 text-sm">
            <div className="rounded-xl bg-slate-50 p-4">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Scored</p>
              <p className="mt-1 text-xl font-bold text-slate-900">{scored}</p>
            </div>
            <div className="rounded-xl bg-slate-50 p-4">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Total</p>
              <p className="mt-1 text-xl font-bold text-slate-900">{submissions.length}</p>
            </div>
          </div>
        </JudgePanel>
      </div>
      <RecentScoringActivity />
    </div>
  );
}
