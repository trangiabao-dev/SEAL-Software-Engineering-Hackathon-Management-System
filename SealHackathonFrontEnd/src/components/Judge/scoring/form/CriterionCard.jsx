import { CommentInput } from "./CommentInput";
import { ScoreInput } from "./ScoreInput";

export function CriterionCard({ criterion, score, comment, error, disabled, onScoreChange, onCommentChange }) {
  return (
    <div className="rounded-2xl border border-slate-100 bg-white p-5">
      <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h4 className="font-bold text-slate-900">{criterion.name}</h4>
          <p className="mt-1 text-sm leading-6 text-slate-500">{criterion.description}</p>
        </div>
        <div className="rounded-xl bg-orange-50 px-3 py-2 text-right text-xs font-bold text-orange-700">
          MaxScore {criterion.maxScore}<br />Weight {Math.round(criterion.weight * 100)}%
        </div>
      </div>
      <div className="grid gap-4 md:grid-cols-[180px_1fr]">
        <ScoreInput value={score} maxScore={criterion.maxScore} disabled={disabled} error={error} onChange={onScoreChange} />
        <CommentInput value={comment} disabled={disabled} onChange={onCommentChange} />
      </div>
    </div>
  );
}
