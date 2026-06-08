import { mentors, teams } from "../coordinatorMockData";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorProgressBar,
  CoordinatorTable,
  icons,
} from "../CoordinatorUI";

export function MentorsManagement() {
  const columns = [
    { key: "name", label: "Mentor" },
    { key: "expertise", label: "Expertise" },
    { key: "tracks", label: "Tracks" },
    { key: "teams", label: "Teams" },
    { key: "workload", label: "Workload" },
    { key: "actions", label: "Actions" },
  ];
  return (
    <div className="space-y-6">
      <CoordinatorPanel
        title="Mentor workload validation"
        subtitle="Max 3 teams per mentor per track and max 2 tracks per mentor"
        icon={icons.Handshake}
      >
        <div className="grid gap-4 md:grid-cols-3">
          {mentors.map((mentor) => (
            <div
              key={mentor.id}
              className="rounded-xl border border-slate-100 p-4"
            >
              <div className="mb-3 flex items-center justify-between">
                <p className="font-bold text-slate-900">{mentor.name}</p>
                <CoordinatorBadge
                  tone={
                    mentor.status === "At limit"
                      ? "warning"
                      : mentor.status === "Busy"
                        ? "orange"
                        : "success"
                  }
                >
                  {mentor.status}
                </CoordinatorBadge>
              </div>
              <CoordinatorProgressBar
                label={`${mentor.teams} assigned teams`}
                value={mentor.workload}
                color={mentor.workload >= 100 ? "#D97706" : "#F26F21"}
              />
              <p className="mt-3 text-xs text-slate-500">
                Tracks: {mentor.tracks.join(", ")}
              </p>
            </div>
          ))}
        </div>
      </CoordinatorPanel>
      <CoordinatorPanel
        title="Mentor assignments"
        subtitle="Assign mentors to tracks and teams"
        icon={icons.Users}
        actions={
          <CoordinatorActionButton variant="primary" icon={icons.Plus}>
            Assign Mentor
          </CoordinatorActionButton>
        }
      >
        <CoordinatorTable
          columns={columns}
          rows={mentors}
          renderCell={(row, key) => {
            if (key === "tracks") return row.tracks.join(", ");
            if (key === "workload")
              return (
                <div className="w-36">
                  <CoordinatorProgressBar value={row.workload} />
                </div>
              );
            if (key === "actions")
              return (
                <div className="flex gap-2">
                  <CoordinatorActionButton>Assign Team</CoordinatorActionButton>
                  <CoordinatorActionButton icon={icons.Eye}>
                    View
                  </CoordinatorActionButton>
                </div>
              );
            return row[key];
          }}
        />
      </CoordinatorPanel>
      <CoordinatorPanel
        title="Mentor/team relationship"
        subtitle="Current assigned coverage"
        icon={icons.GitBranch}
      >
        <div className="grid gap-3 md:grid-cols-2">
          {teams.map((team) => (
            <div
              key={team.id}
              className="flex items-center justify-between rounded-xl border border-slate-100 p-4"
            >
              <div>
                <p className="font-bold text-slate-900">{team.name}</p>
                <p className="text-sm text-slate-500">{team.track}</p>
              </div>
              <CoordinatorBadge
                tone={team.mentor === "Unassigned" ? "warning" : "success"}
              >
                {team.mentor}
              </CoordinatorBadge>
            </div>
          ))}
        </div>
      </CoordinatorPanel>
    </div>
  );
}
