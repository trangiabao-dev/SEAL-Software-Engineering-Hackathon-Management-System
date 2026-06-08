import { JudgeActionButton } from "../../shared/JudgeActionButton";
import { JudgeModal } from "../../shared/JudgeModal";

export function ScoringConfirmationModal({ totalScore, onCancel, onConfirm, submitting }) {
  return (
    <JudgeModal
      title="Submit all scores?"
      subtitle="This will mark the current submission as scored in this mock workspace."
      onClose={onCancel}
      footer={
        <div className="flex justify-end gap-2">
          <JudgeActionButton onClick={onCancel} disabled={submitting}>Cancel</JudgeActionButton>
          <JudgeActionButton variant="primary" onClick={onConfirm} disabled={submitting}>{submitting ? "Submitting..." : "Confirm submit"}</JudgeActionButton>
        </div>
      }
    >
      <div className="rounded-xl bg-orange-50 p-4 text-sm text-orange-800">
        <p className="font-bold">Total score preview: {totalScore}</p>
        <p className="mt-1">Please confirm all criterion scores and comments are ready.</p>
      </div>
    </JudgeModal>
  );
}
