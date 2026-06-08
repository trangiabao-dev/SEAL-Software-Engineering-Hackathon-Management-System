import { JudgeBadge } from "../shared/JudgeBadge";
import { JudgeActionButton } from "../shared/JudgeActionButton";
import { judgeIcons } from "../shared/judgeIcons";

export function SubmissionCard({ submission, isLocked, isTimeLocked, onOpen }) {
  const tone = submission.status === "Scored" || submission.status === "Locked" ? "success" : submission.status === "Draft" ? "warning" : "neutral";

  return (
    <div className="rounded-xl border border-slate-100 p-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h4 className="font-bold text-slate-900">{submission.teamName}</h4>
          <p className="mt-1 text-sm text-slate-500">{submission.university}</p>
        </div>
        <JudgeBadge tone={tone}>{submission.status}</JudgeBadge>
      </div>
      <div className="mt-4 grid gap-2 text-sm text-slate-600 md:grid-cols-2">
        <a className="truncate font-semibold text-orange-700" href={submission.demoUrl} target="_blank" rel="noreferrer">DemoUrl</a>
        <a className="truncate font-semibold text-orange-700" href={submission.reportUrl} target="_blank" rel="noreferrer">ReportUrl</a>
      </div>
      <div className="mt-4">
        <JudgeActionButton disabled={isLocked || isTimeLocked} onClick={() => onOpen(submission)} icon={judgeIcons.Gavel}>
          {submission.status === "Scored" || isLocked ? "View" : "Score"}
        </JudgeActionButton>
      </div>
    </div>
  );
}
