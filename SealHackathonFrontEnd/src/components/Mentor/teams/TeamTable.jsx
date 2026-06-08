import { MentorBadge } from "../shared/MentorBadge";
import { MentorProgressBar } from "../shared/MentorProgressBar";
import { MentorTable } from "../shared/MentorTable";
import { mentorIcons } from "../shared/mentorIcons";
import { TeamStatusBadge } from "./TeamStatusBadge";

function submissionTone(status) {
  if (status === "Submitted") return "success";
  if (status === "Missing") return "danger";
  return "warning";
}

export function TeamTable({ teams, onSelectTeam }) {
  const columns = [
    { key: "teamName", label: "TeamName" },
    { key: "university", label: "University" },
    { key: "track", label: "Track" },
    { key: "status", label: "Team Status" },
    { key: "submission", label: "Submission" },
    { key: "progress", label: "Progress" },
    { key: "details", label: "Details" },
  ];

  return (
    <MentorTable
      columns={columns}
      rows={teams}
      renderCell={(team, key) => {
        if (key === "teamName") {
          const leader = team.members.find((member) => member.isLeader);
          return <div><p className="font-bold text-slate-900">{team.teamName}</p><p className="text-xs text-slate-500">Leader: {leader?.fullName}</p></div>;
        }
        if (key === "status") return <TeamStatusBadge status={team.status} />;
        if (key === "submission") return <MentorBadge tone={submissionTone(team.submission.status)}>{team.submission.status}</MentorBadge>;
        if (key === "progress") return <div className="w-40"><MentorProgressBar value={team.progress} /></div>;
        if (key === "details") {
          return (
            <button
              className="inline-flex items-center gap-2 rounded-xl border border-slate-300 px-3 py-2 text-sm font-bold text-slate-700 transition hover:border-orange-400 hover:bg-orange-50 hover:text-orange-700"
              onClick={() => onSelectTeam(team)}
            >
              <mentorIcons.Eye className="h-4 w-4" />
              View details
            </button>
          );
        }
        return team[key];
      }}
    />
  );
}
