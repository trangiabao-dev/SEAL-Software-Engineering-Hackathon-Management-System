import {
  dashboardStats,
  events,
  recentActivity,
  rounds,
  teams,
} from "../coordinatorMockData";
import {
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorProgressBar,
  CoordinatorStatCard,
  icons,
} from "../CoordinatorUI";

export function DashboardOverview() {
  const statIcons = [icons.Users, icons.UserCheck, icons.Timer, icons.Scale];

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
        {dashboardStats.map((stat, index) => (
          <CoordinatorStatCard
            key={stat.id}
            {...stat}
            icon={statIcons[index]}
          />
        ))}
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <CoordinatorPanel
          title="Event status summary"
          subtitle="Active event and operational readiness"
          icon={icons.CalendarDays}
          className="xl:col-span-2"
        >
          <div className="grid gap-4 md:grid-cols-3">
            {events.map((event) => (
              <div
                key={event.id}
                className="rounded-xl border border-slate-100 p-4"
              >
                <div className="mb-3 flex items-start justify-between gap-2">
                  <h4 className="font-bold text-slate-900">{event.name}</h4>
                  {event.active && (
                    <CoordinatorBadge tone="orange">Active</CoordinatorBadge>
                  )}
                </div>
                <p className="text-sm text-slate-500">{event.description}</p>
                <div className="mt-4 flex items-center justify-between text-sm">
                  <span className="text-slate-500">Teams</span>
                  <span className="font-bold text-slate-900">
                    {event.teams}
                  </span>
                </div>
                <div className="mt-3">
                  <CoordinatorBadge
                    tone={
                      event.status === "Ongoing"
                        ? "success"
                        : event.status === "Draft"
                          ? "warning"
                          : "neutral"
                    }
                  >
                    {event.status}
                  </CoordinatorBadge>
                </div>
              </div>
            ))}
          </div>
        </CoordinatorPanel>

        <CoordinatorPanel
          title="Quick actions"
          subtitle="Common coordinator workflows"
          icon={icons.Activity}
        >
          <div className="space-y-3">
            {[
              "Create event",
              "Approve pending teams",
              "Assign judges",
              "Export results",
            ].map((action) => (
              <button
                key={action}
                className="flex w-full items-center justify-between rounded-xl border border-slate-100 px-4 py-3 text-left text-sm font-semibold text-slate-700 hover:bg-orange-50"
              >
                {action}
                <icons.MoreHorizontal className="h-4 w-4 text-slate-400" />
              </button>
            ))}
          </div>
        </CoordinatorPanel>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
        <CoordinatorPanel
          title="Submission progress"
          subtitle="Readiness by team status"
          icon={icons.Upload}
        >
          <div className="space-y-4">
            {teams.map((team) => (
              <CoordinatorProgressBar
                key={team.id}
                label={`${team.name} • ${team.submission}`}
                value={team.readiness}
              />
            ))}
          </div>
        </CoordinatorPanel>

        <CoordinatorPanel
          title="Round timeline overview"
          subtitle="Current competition flow"
          icon={icons.Timer}
        >
          <div className="space-y-4">
            {rounds.map((round, index) => (
              <div key={round.id} className="flex gap-3">
                <div className="flex flex-col items-center">
                  <div
                    className="flex h-8 w-8 items-center justify-center rounded-full text-xs font-bold text-white"
                    style={{
                      background:
                        round.status === "Active" ? "#F26F21" : "#CBD5E1",
                    }}
                  >
                    {index + 1}
                  </div>
                  {index < rounds.length - 1 && (
                    <div className="h-9 w-px bg-slate-200" />
                  )}
                </div>
                <div className="flex-1 pb-2">
                  <div className="flex items-center justify-between gap-3">
                    <p className="font-bold text-slate-900">{round.name}</p>
                    <CoordinatorBadge
                      tone={
                        round.status === "Active"
                          ? "orange"
                          : round.status === "Closed"
                            ? "success"
                            : "neutral"
                      }
                    >
                      {round.status}
                    </CoordinatorBadge>
                  </div>
                  <p className="mt-1 text-sm text-slate-500">
                    {round.startTime} → {round.endTime}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </CoordinatorPanel>
      </div>

      <CoordinatorPanel
        title="Recent activity"
        subtitle="Latest administrative and judging events"
        icon={icons.Bell}
      >
        <div className="divide-y divide-slate-100">
          {recentActivity.map((item) => (
            <div
              key={item.id}
              className="flex items-center justify-between gap-4 py-3 first:pt-0 last:pb-0"
            >
              <div>
                <p className="font-semibold text-slate-800">{item.action}</p>
                <p className="text-sm text-slate-500">{item.user}</p>
              </div>
              <div className="text-right">
                <CoordinatorBadge tone={item.tone}>
                  {item.time}
                </CoordinatorBadge>
              </div>
            </div>
          ))}
        </div>
      </CoordinatorPanel>
    </div>
  );
}
