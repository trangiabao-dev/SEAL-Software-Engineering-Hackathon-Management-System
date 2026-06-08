import { activeEvent, currentRound, mentorTeams } from "../mentorMockData";
import { MentorBadge } from "../shared/MentorBadge";
import { MentorPanel } from "../shared/MentorPanel";
import { MentorProgressBar } from "../shared/MentorProgressBar";
import { mentorIcons } from "../shared/mentorIcons";
import { AssignedTracks } from "./AssignedTracks";
import { MentorStatsCards } from "./MentorStatsCards";
import { TeamActivity } from "./TeamActivity";

export function MentorOverview({ onViewTeams }) {
  const submitted = mentorTeams.filter((team) => team.submission.status === "Submitted").length;
  const submissionProgress = Math.round((submitted / mentorTeams.length) * 100);

  return (
    <div className="space-y-6">
      <MentorStatsCards />

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <MentorPanel title="Active Event" subtitle="Current event and round status" icon={mentorIcons.CalendarDays} className="xl:col-span-2">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-xl border border-slate-100 p-4">
              <div className="mb-3 flex items-start justify-between gap-2">
                <h4 className="font-bold text-slate-900">{activeEvent.name}</h4>
                <MentorBadge tone="orange">{activeEvent.status}</MentorBadge>
              </div>
              <p className="text-sm text-slate-500">{activeEvent.location}</p>
              <p className="mt-3 text-sm font-semibold text-slate-700">{activeEvent.timeline}</p>
            </div>
            <div className="rounded-xl border border-slate-100 p-4">
              <div className="mb-3 flex items-start justify-between gap-2">
                <h4 className="font-bold text-slate-900">{currentRound.name}</h4>
                <MentorBadge tone="success">{currentRound.status}</MentorBadge>
              </div>
              <p className="text-sm text-slate-500">{currentRound.window}</p>
              <p className="mt-3 text-sm font-semibold text-slate-700">Closes in {currentRound.closesIn}</p>
            </div>
          </div>
        </MentorPanel>

        <MentorPanel
          title="Submission Progress"
          subtitle="Assigned team submission summary"
          icon={mentorIcons.FileText}
          actions={<button className="rounded-xl border border-orange-200 bg-orange-50 px-3 py-2 text-sm font-bold text-orange-700 transition hover:bg-orange-100" onClick={onViewTeams}>View teams</button>}
        >
          <MentorProgressBar value={submissionProgress} label="Submitted" />
          <div className="mt-4 space-y-3">
            {mentorTeams.map((team) => (
              <div key={team.id} className="flex items-center justify-between gap-3 text-sm">
                <span className="truncate font-semibold text-slate-700">{team.teamName}</span>
                <MentorBadge tone={team.submission.status === "Submitted" ? "success" : team.submission.status === "Missing" ? "danger" : "warning"}>{team.submission.status}</MentorBadge>
              </div>
            ))}
          </div>
        </MentorPanel>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <div className="xl:col-span-2"><AssignedTracks /></div>
        <TeamActivity />
      </div>
    </div>
  );
}
