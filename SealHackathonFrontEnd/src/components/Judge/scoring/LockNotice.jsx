import { judgeIcons } from "../shared/judgeIcons";

export function LockNotice({ type = "ranking" }) {
  const { Lock } = judgeIcons;
  const message = type === "time" ? "Chưa đến thời gian chấm điểm" : "Kết quả đã được chốt. Không thể chỉnh sửa điểm.";

  return (
    <div className="flex items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
      <Lock className="mt-0.5 h-4 w-4 flex-shrink-0" />
      <div>
        <p className="font-bold">{message}</p>
        <p className="mt-1 text-amber-700">Scoring actions are unavailable while this lock is active.</p>
      </div>
    </div>
  );
}
