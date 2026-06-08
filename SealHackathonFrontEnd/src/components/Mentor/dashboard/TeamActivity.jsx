import { recentTeamActivity } from "../mentorMockData";
import { MentorBadge } from "../shared/MentorBadge";
import { MentorPanel } from "../shared/MentorPanel";
import { mentorIcons } from "../shared/mentorIcons";

function statusTone(status) {
  if (status === "Submitted") return "success";
  if (status === "Missing") return "danger";
  return "warning";
}

export function TeamActivity() {
  return (
    <MentorPanel title="Recent Team Activity" subtitle="Latest read-only activity from assigned teams" icon={mentorIcons.Activity}>
      <div className="space-y-3">
        {recentTeamActivity.map((item) => (
          <div key={item.id} className="flex items-start justify-between gap-3 rounded-xl border border-slate-100 p-4">
            <div className="min-w-0">
              <p className="font-bold text-slate-900">{item.team}</p>
              <p className="mt-1 text-sm text-slate-500">{item.action}</p>
            </div>
            <MentorBadge tone={statusTone(item.status)}>{item.status}</MentorBadge>
          </div>
        ))}
      </div>
    </MentorPanel>
  );
}
