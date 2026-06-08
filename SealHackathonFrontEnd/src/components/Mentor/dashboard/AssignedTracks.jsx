import { assignedTracks } from "../mentorMockData";
import { MentorBadge } from "../shared/MentorBadge";
import { MentorPanel } from "../shared/MentorPanel";
import { MentorProgressBar } from "../shared/MentorProgressBar";
import { mentorIcons } from "../shared/mentorIcons";

export function AssignedTracks() {
  return (
    <MentorPanel title="Assigned Tracks" subtitle="Track workload follows the maximum 3 teams per mentor rule" icon={mentorIcons.GitBranch}>
      <div className="grid gap-4 md:grid-cols-2">
        {assignedTracks.map((track) => {
          const workload = Math.round((track.teams / track.maxTeamsPerMentor) * 100);
          return (
            <div key={track.id} className="rounded-xl border border-slate-100 p-4">
              <div className="mb-3 flex items-start justify-between gap-3">
                <div>
                  <h4 className="font-bold text-slate-900">{track.name}</h4>
                  <p className="mt-1 text-sm text-slate-500">{track.description}</p>
                </div>
                <MentorBadge tone={track.status === "At capacity" ? "warning" : "success"}>{track.status}</MentorBadge>
              </div>
              <MentorProgressBar value={workload} label={`${track.teams}/${track.maxTeamsPerMentor} teams`} />
              <div className="mt-4 grid grid-cols-2 gap-3 text-sm">
                <div className="rounded-lg bg-slate-50 p-3">
                  <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Topics</p>
                  <p className="mt-1 font-bold text-slate-900">{track.topics}</p>
                </div>
                <div className="rounded-lg bg-slate-50 p-3">
                  <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Limit</p>
                  <p className="mt-1 font-bold text-slate-900">Max 3 teams</p>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </MentorPanel>
  );
}
