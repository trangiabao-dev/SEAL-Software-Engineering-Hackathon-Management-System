import { currentTime, judgeRounds } from "../judgeMockData";
import { JudgeActionButton } from "../shared/JudgeActionButton";
import { JudgePanel } from "../shared/JudgePanel";
import { JudgeProgressBar } from "../shared/JudgeProgressBar";
import { judgeIcons } from "../shared/judgeIcons";
import { RoundStatusBadge } from "../rounds/RoundStatusBadge";
import { LockNotice } from "../scoring/LockNotice";

function isRoundTimeLocked(round) {
  return currentTime < new Date(round.endTime);
}

export function AssignedRounds({ onOpenScoring }) {
  return (
    <JudgePanel title="Assigned Rounds" subtitle="Round access follows scoring window and ranking lock rules" icon={judgeIcons.CalendarDays}>
      <div className="grid gap-4 lg:grid-cols-3">
        {judgeRounds.map((round) => {
          const progress = round.assignedSubmissions ? (round.completedSubmissions / round.assignedSubmissions) * 100 : 0;
          const rankingLocked = round.ranking?.calculatedAt != null;
          const timeLocked = isRoundTimeLocked(round);
          return (
            <div key={round.id} className="rounded-xl border border-slate-100 p-4">
              <div className="mb-3 flex items-start justify-between gap-3">
                <div>
                  <h4 className="font-bold text-slate-900">{round.name}</h4>
                  <p className="mt-1 text-sm text-slate-500">{round.trackName}</p>
                </div>
                <RoundStatusBadge status={round.status} />
              </div>
              <div className="space-y-2 text-sm text-slate-600">
                <p><span className="font-semibold text-slate-700">EndTime:</span> {new Date(round.endTime).toLocaleString()}</p>
                <p><span className="font-semibold text-slate-700">Submissions:</span> {round.completedSubmissions}/{round.assignedSubmissions}</p>
              </div>
              <div className="mt-4"><JudgeProgressBar value={progress} label="Scoring progress" /></div>
              <div className="mt-4 space-y-3">
                {rankingLocked && <LockNotice />}
                {!rankingLocked && timeLocked && <LockNotice type="time" />}
                <JudgeActionButton variant="secondary" disabled={timeLocked || rankingLocked} onClick={onOpenScoring} icon={judgeIcons.Gavel}>
                  Open scoring
                </JudgeActionButton>
              </div>
            </div>
          );
        })}
      </div>
    </JudgePanel>
  );
}
