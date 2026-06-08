import { recentScoringActivity } from "../judgeMockData";
import { JudgeBadge } from "../shared/JudgeBadge";
import { JudgePanel } from "../shared/JudgePanel";
import { judgeIcons } from "../shared/judgeIcons";

export function RecentScoringActivity() {
  return (
    <JudgePanel title="Recent Scoring Activity" subtitle="Your recent scoring workspace events" icon={judgeIcons.Activity}>
      <div className="space-y-3">
        {recentScoringActivity.map((item) => (
          <div key={item.id} className="flex items-start justify-between gap-3 rounded-xl border border-slate-100 p-4">
            <div className="min-w-0">
              <p className="font-bold text-slate-900">{item.teamName}</p>
              <p className="mt-1 text-sm text-slate-500">{item.action}</p>
              <p className="mt-1 text-xs text-slate-400">{item.time}</p>
            </div>
            <JudgeBadge tone={item.tone}>{item.tone}</JudgeBadge>
          </div>
        ))}
      </div>
    </JudgePanel>
  );
}
