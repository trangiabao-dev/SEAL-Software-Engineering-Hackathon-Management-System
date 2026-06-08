import { JudgeActionButton } from "../../shared/JudgeActionButton";
import { JudgeProgressBar } from "../../shared/JudgeProgressBar";
import { judgeIcons } from "../../shared/judgeIcons";

export function SubmitPanel({ criteria, scores, totalScore, disabled, saving, submitting, onSaveDraft, onSubmit }) {
  const completed = criteria.filter((criterion) => scores[criterion.id] !== undefined && scores[criterion.id] !== "").length;
  const progress = criteria.length ? (completed / criteria.length) * 100 : 0;

  return (
    <aside className="sticky top-6 rounded-2xl border bg-white p-5" style={{ borderColor: "#E5E7EB", boxShadow: "0 10px 30px rgba(0,0,0,0.02)" }}>
      <div className="mb-5">
        <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Score preview</p>
        <p className="mt-1 text-3xl font-bold text-slate-900">{totalScore}</p>
      </div>
      <JudgeProgressBar value={progress} label="Criteria completed" />
      <div className="mt-5 space-y-3">
        <JudgeActionButton className="w-full" disabled={disabled || saving || submitting} onClick={onSaveDraft} icon={judgeIcons.FileText}>
          {saving ? "Saving..." : "Save draft"}
        </JudgeActionButton>
        <JudgeActionButton className="w-full" variant="primary" disabled={disabled || saving || submitting} onClick={onSubmit} icon={judgeIcons.CheckCircle2}>
          {submitting ? "Submitting..." : "Submit all scores"}
        </JudgeActionButton>
      </div>
    </aside>
  );
}
