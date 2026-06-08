import { judgeRounds } from "../judgeMockData";
import { JudgeStatCard } from "../shared/JudgeStatCard";
import { judgeIcons } from "../shared/judgeIcons";

export function JudgeStatsCards({ submissions }) {
  const completed = submissions.filter((item) => item.status === "Scored" || item.status === "Locked").length;
  const pending = submissions.filter((item) => item.status === "Not scored" || item.status === "Draft").length;
  const locked = judgeRounds.filter((round) => round.ranking?.calculatedAt != null).length;
  const activeRound = judgeRounds.find((round) => round.status === "Scoring");

  const stats = [
    { label: "Assigned Rounds", value: judgeRounds.length, icon: judgeIcons.CalendarDays, tone: "orange", helper: "Available in this event" },
    { label: "Active Scoring", value: activeRound ? activeRound.name : "None", icon: judgeIcons.Timer, tone: "purple", helper: "Current scoring window" },
    { label: "Completed Scores", value: `${completed}/${submissions.length}`, icon: judgeIcons.CheckCircle2, tone: "green", helper: `${pending} remaining` },
    { label: "Locked Rounds", value: locked, icon: judgeIcons.Lock, tone: "red", helper: "Finalized by ranking" },
  ];

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
      {stats.map((stat) => <JudgeStatCard key={stat.label} {...stat} />)}
    </div>
  );
}
