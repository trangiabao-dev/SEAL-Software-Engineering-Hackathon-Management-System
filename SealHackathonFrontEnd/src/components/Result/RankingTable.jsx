import React from 'react';
import { List, Star } from 'lucide-react';

const rankMedals = {
  1: { emoji: '🥇', color: '#F26F21', bgClass: 'bg-[#F26F21]/[0.04]', borderClass: 'border-l-[#F26F21]' },
  2: { emoji: '🥈', color: '#94a3b8', bgClass: 'bg-slate-500/[0.03]', borderClass: 'border-l-slate-500' },
  3: { emoji: '🥉', color: '#c97c3a', bgClass: 'bg-amber-700/[0.03]', borderClass: 'border-l-amber-700' },
};

export default function RankingTable({ teams, currentUserTeamId }) {
  return (
    <section className="py-12 md:py-16">
      {/* Section header */}
      <div className="text-center mb-12 space-y-4">
        <div className="inline-flex items-center gap-2 mx-auto">
          <List className="w-4 h-4 text-[#F26F21]" />
          <p className="text-[10px] font-bold tracking-widest uppercase text-[#F26F21]">
            FULL RANKINGS
          </p>
        </div>
        <h2
          className="text-3xl md:text-4xl font-extrabold text-white tracking-tight"
          style={{ fontFamily: 'Montserrat, sans-serif' }}
        >
          Official <span className="text-[#F26F21]">Leaderboard</span>
        </h2>
        <p className="text-slate-400 text-sm md:text-base max-w-xl mx-auto leading-relaxed">
          Complete ranking of all participating teams sorted by final score.
        </p>
      </div>

      {/* Table container */}
      <div className="rounded-2xl border border-white/[0.06] bg-white/[0.01] overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[640px]">
            <thead>
              <tr className="border-b border-white/[0.06]">
                {['Rank', 'Team', 'Project', 'Track', 'Score'].map((header) => (
                  <th
                    key={header}
                    className="px-5 py-4 text-[10px] font-bold tracking-widest uppercase text-slate-500 text-left"
                  >
                    {header}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {teams.map((team) => {
                const medal = rankMedals[team.rank];
                const isUser = team.id === currentUserTeamId;

                return (
                  <tr
                    key={team.id}
                    className={`border-b border-white/[0.03] transition-colors duration-200 ${
                      isUser
                        ? 'bg-[#F26F21]/[0.06] border-l-2 border-l-[#F26F21]'
                        : medal
                        ? `${medal.bgClass} border-l-2 ${medal.borderClass}`
                        : 'hover:bg-white/[0.015] border-l-2 border-l-transparent'
                    }`}
                  >
                    {/* Rank */}
                    <td className="px-5 py-4">
                      <div className="flex items-center gap-2">
                        {medal ? (
                          <span className="text-lg">{medal.emoji}</span>
                        ) : (
                          <span className="text-sm font-bold text-slate-500 tabular-nums w-6 text-center">
                            {team.rank}
                          </span>
                        )}
                      </div>
                    </td>

                    {/* Team name + avatar */}
                    <td className="px-5 py-4">
                      <div className="flex items-center gap-3">
                        <div
                          className="w-9 h-9 rounded-lg flex items-center justify-center text-[10px] font-bold flex-shrink-0"
                          style={{
                            background: isUser ? '#F26F2112' : 'rgba(255,255,255,0.03)',
                            color: isUser ? '#F26F21' : '#94a3b8',
                            border: `1px solid ${isUser ? '#F26F2125' : 'rgba(255,255,255,0.06)'}`,
                          }}
                        >
                          {team.avatar}
                        </div>
                        <div>
                          <p className={`text-sm font-bold ${isUser ? 'text-[#F26F21]' : 'text-white'}`}>
                            {team.name}
                            {isUser && (
                              <span className="ml-2 inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-[#F26F21]/10 border border-[#F26F21]/20 text-[8px] font-bold tracking-widest uppercase text-[#F26F21]">
                                <Star className="w-2.5 h-2.5" />
                                YOUR TEAM
                              </span>
                            )}
                          </p>
                          <p className="text-[11px] text-slate-500 mt-0.5">
                            {team.members.length} members
                          </p>
                        </div>
                      </div>
                    </td>

                    {/* Project */}
                    <td className="px-5 py-4">
                      <p className="text-xs text-slate-300 max-w-[200px] truncate">
                        {team.project}
                      </p>
                    </td>

                    {/* Track */}
                    <td className="px-5 py-4">
                      <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[10px] font-semibold tracking-wide bg-white/[0.03] border border-white/[0.06] text-slate-400">
                        <span
                          className="w-1.5 h-1.5 rounded-full"
                          style={{ background: medal?.color || '#64748b' }}
                        />
                        {team.track}
                      </span>
                    </td>

                    {/* Score */}
                    <td className="px-5 py-4">
                      <span
                        className={`text-lg font-extrabold tabular-nums ${
                          medal ? '' : 'text-slate-300'
                        }`}
                        style={{
                          color: medal?.color || (isUser ? '#F26F21' : undefined),
                          fontFamily: 'Montserrat, sans-serif',
                        }}
                      >
                        {team.score}
                      </span>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        {/* Footer note */}
        <div className="px-5 py-3 border-t border-white/[0.04] flex items-center justify-between">
          <p className="text-[10px] text-slate-600 tracking-wide">
            Showing {teams.length} of {teams.length} teams
          </p>
          <p className="text-[10px] text-slate-600 tracking-wide">
            Scores are out of 100.0
          </p>
        </div>
      </div>
    </section>
  );
}
