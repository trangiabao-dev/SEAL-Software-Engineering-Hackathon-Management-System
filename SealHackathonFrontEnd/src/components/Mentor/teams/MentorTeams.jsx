import { useMemo, useState } from "react";
import { mentorTeams } from "../mentorMockData";
import { EmptyState } from "../shared/EmptyState";
import { MentorPanel } from "../shared/MentorPanel";
import { mentorIcons } from "../shared/mentorIcons";
import { TeamDetailModal } from "./details/TeamDetailModal";
import { TeamTable } from "./TeamTable";

const statuses = ["All", "Pending", "Approved", "Disqualified"];

export function MentorTeams() {
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("All");
  const [selectedTeam, setSelectedTeam] = useState(null);
  const { Filter, Search, Users } = mentorIcons;

  const filteredTeams = useMemo(() => {
    const keyword = search.trim().toLowerCase();
    return mentorTeams.filter((team) => {
      const matchesStatus = status === "All" || team.status === status;
      const matchesKeyword = !keyword || [team.teamName, team.university, team.track].some((value) => value.toLowerCase().includes(keyword));
      return matchesStatus && matchesKeyword;
    });
  }, [search, status]);

  return (
    <div className="space-y-6">
      <MentorPanel title="Assigned teams" subtitle="Read-only team list for your assigned tracks" icon={Users}>
        <div className="mb-5 grid gap-3 md:grid-cols-[1fr_auto]">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search by team, university, or track"
              className="w-full rounded-xl border border-slate-200 bg-white py-2.5 pl-10 pr-4 text-sm outline-none transition focus:border-orange-300 focus:ring-2 focus:ring-orange-100"
            />
          </div>
          <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2.5">
            <Filter className="h-4 w-4 text-slate-400" />
            <select value={status} onChange={(event) => setStatus(event.target.value)} className="bg-transparent text-sm font-semibold text-slate-700 outline-none">
              {statuses.map((item) => <option key={item} value={item}>{item}</option>)}
            </select>
          </div>
        </div>

        {filteredTeams.length > 0 ? (
          <TeamTable teams={filteredTeams} onSelectTeam={setSelectedTeam} />
        ) : (
          <EmptyState icon={Users} title="No assigned teams found" description="Try adjusting the search keyword or status filter." />
        )}
      </MentorPanel>

      <MentorPanel title="Permission boundary" subtitle="Mentor access is intentionally limited to preserve judging neutrality" icon={mentorIcons.CheckCircle2}>
        <div className="grid gap-3 md:grid-cols-3">
          {["Read-only team status", "Read-only submissions", "View-only topic details"].map((item) => (
            <div key={item} className="rounded-xl border border-emerald-100 bg-emerald-50 px-4 py-3 text-sm font-bold text-emerald-700">{item}</div>
          ))}
        </div>
      </MentorPanel>

      {selectedTeam && <TeamDetailModal team={selectedTeam} onClose={() => setSelectedTeam(null)} />}
    </div>
  );
}
