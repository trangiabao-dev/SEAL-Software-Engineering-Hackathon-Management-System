import { useMemo, useState } from "react";
import { judgeCriteria } from "../../judgeMockData";
import { JudgeBadge } from "../../shared/JudgeBadge";
import { JudgeModal } from "../../shared/JudgeModal";
import { judgeIcons } from "../../shared/judgeIcons";
import { LockNotice } from "../LockNotice";
import { CriterionCard } from "./CriterionCard";
import { ScoringConfirmationModal } from "./ScoringConfirmationModal";
import { SubmitPanel } from "./SubmitPanel";

function validateScore(value, maxScore) {
  if (value === undefined || value === "") return "Score is required.";
  const numeric = Number(value);
  if (!Number.isFinite(numeric)) return "Score must be numeric.";
  if (numeric < 0 || numeric > maxScore) return `Score must be between 0 and ${maxScore}.`;
  return "";
}

function computeTotal(scores) {
  const weighted = judgeCriteria.reduce((sum, criterion) => {
    const raw = Number(scores[criterion.id] || 0);
    return sum + raw * criterion.weight;
  }, 0);
  return Number(weighted.toFixed(1));
}

export function ScoringForm({ submission, isLocked, isTimeLocked, saving, submitting, onClose, onSaveDraft, onSubmitScores }) {
  const [scores, setScores] = useState(submission.scores || {});
  const [comments, setComments] = useState(submission.comments || {});
  const [errors, setErrors] = useState({});
  const [confirmOpen, setConfirmOpen] = useState(false);
  const disabled = isLocked || isTimeLocked || submission.status === "Scored" || submission.status === "Locked";
  const totalScore = useMemo(() => computeTotal(scores), [scores]);

  const validateAll = () => {
    const nextErrors = {};
    judgeCriteria.forEach((criterion) => {
      const error = validateScore(scores[criterion.id], criterion.maxScore);
      if (error) nextErrors[criterion.id] = error;
    });
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const updateScore = (criterion, value) => {
    setScores((prev) => ({ ...prev, [criterion.id]: value }));
    setErrors((prev) => ({ ...prev, [criterion.id]: validateScore(value, criterion.maxScore) }));
  };

  const saveDraft = () => {
    if (disabled) return;
    onSaveDraft(submission.id, scores, comments, totalScore);
  };

  const requestSubmit = () => {
    if (disabled) return;
    if (!validateAll()) return;
    setConfirmOpen(true);
  };

  return (
    <>
      <JudgeModal title="Scoring Form" subtitle={`${submission.teamName} • ${submission.university}`} onClose={onClose} maxWidth="max-w-6xl">
        <div className="space-y-5">
          {(isLocked || submission.status === "Locked") && <LockNotice />}
          {isTimeLocked && !isLocked && <LockNotice type="time" />}
          <div className="grid gap-4 rounded-2xl border border-slate-100 bg-slate-50 p-4 md:grid-cols-4">
            <div><p className="text-xs font-bold uppercase text-slate-500">TeamName</p><p className="mt-1 font-bold text-slate-900">{submission.teamName}</p></div>
            <div><p className="text-xs font-bold uppercase text-slate-500">University</p><p className="mt-1 font-bold text-slate-900">{submission.university}</p></div>
            <div><p className="text-xs font-bold uppercase text-slate-500">DemoUrl</p><a className="mt-1 inline-flex items-center gap-1 font-bold text-orange-700" href={submission.demoUrl} target="_blank" rel="noreferrer">Open <judgeIcons.ExternalLink className="h-4 w-4" /></a></div>
            <div><p className="text-xs font-bold uppercase text-slate-500">Status</p><div className="mt-1"><JudgeBadge tone={disabled ? "neutral" : "warning"}>{submission.status}</JudgeBadge></div></div>
          </div>

          <div className="grid gap-6 lg:grid-cols-[1fr_280px]">
            <div className="space-y-4">
              {judgeCriteria.map((criterion) => (
                <CriterionCard
                  key={criterion.id}
                  criterion={criterion}
                  score={scores[criterion.id]}
                  comment={comments[criterion.id]}
                  error={errors[criterion.id]}
                  disabled={disabled}
                  onScoreChange={(value) => updateScore(criterion, value)}
                  onCommentChange={(value) => setComments((prev) => ({ ...prev, [criterion.id]: value }))}
                />
              ))}
            </div>
            <SubmitPanel criteria={judgeCriteria} scores={scores} totalScore={totalScore} disabled={disabled} saving={saving} submitting={submitting} onSaveDraft={saveDraft} onSubmit={requestSubmit} />
          </div>
        </div>
      </JudgeModal>
      {confirmOpen && <ScoringConfirmationModal totalScore={totalScore} submitting={submitting} onCancel={() => setConfirmOpen(false)} onConfirm={() => onSubmitScores(submission.id, scores, comments, totalScore)} />}
    </>
  );
}
