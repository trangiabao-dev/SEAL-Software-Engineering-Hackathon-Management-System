import { assignedTracks, mentorTeams } from "../mentorMockData";
import { MentorStatCard } from "../shared/MentorStatCard";
import { mentorIcons } from "../shared/mentorIcons";

export function MentorStatsCards() {
  const submitted = mentorTeams.filter((team) => team.submission.status === "Submitted").length;
  const stats = [
    { label: "Assigned Tracks", value: assignedTracks.length, icon: mentorIcons.GitBranch, tone: "orange", helper: "Active mentoring scope" },
    { label: "Teams Under Supervision", value: mentorTeams.length, icon: mentorIcons.Users, tone: "blue", helper: "Read-only access" },
    { label: "Current Round", value: "Round 2", icon: mentorIcons.Clock, tone: "amber", helper: "Prototype Submission" },
    { label: "Submitted Projects", value: `${submitted}/${mentorTeams.length}`, icon: mentorIcons.FileText, tone: "green", helper: "Submission progress" },
  ];

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
      {stats.map((stat) => <MentorStatCard key={stat.label} {...stat} />)}
    </div>
  );
}
