import { currentTime, judgeRounds } from "../judgeMockData";
import { JudgePanel } from "../shared/JudgePanel";
import { JudgeProgressBar } from "../shared/JudgeProgressBar";
import { judgeIcons } from "../shared/judgeIcons";
import { LockNotice } from "../scoring/LockNotice";
import { RoundStatusBadge } from "./RoundStatusBadge";

export function JudgeRounds({ onOpenScoring }) {
  return (
    <div className="space-y-6">
      <JudgePanel title="Assigned rounds list" subtitle="Scoring opens only after each round EndTime" icon={judgeIcons.CalendarDays}>
        <div className="space-y-4">
          {judgeRounds.map((round) => {
            const progress = round.assignedSubmissions ? (round.completedSubmissions / round.assignedSubmissions) * 100 : 0;
            const timeLocked = currentTime < new Date(round.endTime);
            const rankingLocked = round.ranking?.calculatedAt != null;
            return (
              <div key={round.id} className="rounded-2xl border border-slate-100 p-5">
                <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                  <div className="min-w-0 flex-1">
                    <div className="flex flex-wrap items-center gap-3">
                      <h3 className="text-lg font-bold text-slate-900">{round.name}</h3>
                      <RoundStatusBadge status={round.status} />
                    </div>
                    <p className="mt-1 text-sm text-slate-500">Track: {round.trackName}</p>
                    <div className="mt-4 grid gap-3 text-sm md:grid-cols-3">
                      <div className="rounded-xl bg-slate-50 p-3"><span className="block text-xs font-bold uppercase text-slate-500">StartTime</span><span className="font-semibold text-slate-800">{new Date(round.startTime).toLocaleString()}</span></div>
                      <div className="rounded-xl bg-slate-50 p-3"><span className="block text-xs font-bold uppercase text-slate-500">EndTime</span><span className="font-semibold text-slate-800">{new Date(round.endTime).toLocaleString()}</span></div>
                      <div className="rounded-xl bg-slate-50 p-3"><span className="block text-xs font-bold uppercase text-slate-500">Submissions</span><span className="font-semibold text-slate-800">{round.assignedSubmissions}</span></div>
                    </div>
                  </div>
                  <button
                    className="rounded-xl border border-orange-200 bg-orange-50 px-3.5 py-2 text-sm font-bold text-orange-700 transition disabled:cursor-not-allowed disabled:opacity-50"
                    disabled={timeLocked || rankingLocked}
                    onClick={onOpenScoring}
                  >
                    Open scoring list
                  </button>
                </div>
                <div className="mt-5"><JudgeProgressBar value={progress} label="Scoring progress" /></div>
                <div className="mt-4 space-y-3">
                  {timeLocked && !rankingLocked && <LockNotice type="time" />}
                  {rankingLocked && <LockNotice />}
                </div>
              </div>
            );
          })}
        </div>
      </JudgePanel>
    </div>
  );
}
