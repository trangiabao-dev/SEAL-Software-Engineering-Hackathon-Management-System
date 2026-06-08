import React from 'react';
import { Trophy, Medal, Award, Crown } from 'lucide-react';

const podiumConfig = [
  {
    rankIndex: 1, // 2nd place
    icon: Medal,
    label: '2nd Place',
    color: '#94a3b8',
    borderClass: 'border-slate-500/20 hover:border-slate-500/40',
    bgGradient: '',
    order: 'md:order-1',
    scale: '',
    badge: null,
  },
  {
    rankIndex: 0, // 1st place (center)
    icon: Trophy,
    label: '1st Place',
    color: '#F26F21',
    borderClass: 'border-[#F26F21]/30 hover:border-[#F26F21]/60',
    bgGradient: 'bg-gradient-to-b from-[#F26F21]/[0.04] to-transparent',
    order: 'md:order-2',
    scale: 'md:scale-105',
    badge: 'CHAMPION',
  },
  {
    rankIndex: 2, // 3rd place
    icon: Award,
    label: '3rd Place',
    color: '#c97c3a',
    borderClass: 'border-amber-700/20 hover:border-amber-700/40',
    bgGradient: '',
    order: 'md:order-3',
    scale: '',
    badge: null,
  },
];

function TeamAvatars({ members, color }) {
  const visibleCount = Math.min(members.length, 4);
  return (
    <div className="flex items-center justify-center -space-x-2 mt-4">
      {members.slice(0, visibleCount).map((name, i) => (
        <div
          key={name}
          className="w-8 h-8 rounded-full flex items-center justify-center text-[10px] font-bold border-2 border-[#080A0F]"
          style={{
            background: `${color}18`,
            color: color,
            zIndex: visibleCount - i,
          }}
          title={name}
        >
          {name.split(' ').map((w) => w[0]).join('')}
        </div>
      ))}
      {members.length > 4 && (
        <div
          className="w-8 h-8 rounded-full flex items-center justify-center text-[9px] font-bold border-2 border-[#080A0F] bg-white/[0.04] text-slate-500"
        >
          +{members.length - 4}
        </div>
      )}
    </div>
  );
}

export default function PodiumSection({ teams }) {
  const top3 = teams.filter((t) => t.rank <= 3).sort((a, b) => a.rank - b.rank);

  return (
    <section className="relative py-16 md:py-20">
      {/* Section header */}
      <div className="text-center mb-14 space-y-4">
        <div className="inline-flex items-center gap-2 mx-auto">
          <Crown className="w-4 h-4 text-[#F26F21]" />
          <p className="text-[10px] font-bold tracking-widest uppercase text-[#F26F21]">
            TOP PERFORMERS
          </p>
        </div>
        <h2
          className="text-3xl md:text-4xl font-extrabold text-white tracking-tight"
          style={{ fontFamily: 'Montserrat, sans-serif' }}
        >
          The <span className="text-[#F26F21]">Podium</span>
        </h2>
        <p className="text-slate-400 text-sm md:text-base max-w-xl mx-auto leading-relaxed">
          Congratulations to the top three teams for their outstanding performance
          and innovative solutions.
        </p>
      </div>

      {/* Podium cards */}
      <div className="grid md:grid-cols-3 gap-6 items-stretch max-w-5xl mx-auto">
        {podiumConfig.map(({ rankIndex, icon: Icon, label, color, borderClass, bgGradient, order, scale, badge }) => {
          const team = top3[rankIndex];
          if (!team) return null;

          return (
            <div
              key={team.id}
              className={`group relative rounded-2xl p-8 flex flex-col items-center text-center transition-all duration-300 hover:translate-y-[-4px] bg-white/[0.01] border ${borderClass} ${bgGradient} ${order} ${scale}`}
            >
              {/* Champion badge */}
              {badge && (
                <div className="absolute -top-3.5 left-1/2 -translate-x-1/2 px-4 py-1 rounded-full bg-[#F26F21] text-white text-[9px] font-bold tracking-widest uppercase shadow-md">
                  {badge}
                </div>
              )}

              {/* Rank icon */}
              <div
                className="w-14 h-14 rounded-xl flex items-center justify-center mb-5"
                style={{
                  background: `${color}08`,
                  border: `1px solid ${color}20`,
                }}
              >
                <Icon className="w-6 h-6" style={{ color }} />
              </div>

              {/* Rank label */}
              <p
                className="text-[10px] font-bold tracking-widest uppercase mb-1"
                style={{ color }}
              >
                {label}
              </p>

              {/* Team name */}
              <h3
                className="text-xl md:text-2xl font-extrabold text-white mb-1"
                style={{ fontFamily: 'Montserrat, sans-serif' }}
              >
                {team.name}
              </h3>

              {/* Project */}
              <p className="text-xs text-slate-400 mb-4 leading-relaxed max-w-[220px]">
                {team.project}
              </p>

              {/* Divider */}
              <div className="w-full h-px mb-4 bg-white/[0.06]" />

              {/* Score */}
              <div className="mb-1">
                <span
                  className="text-3xl md:text-4xl font-extrabold"
                  style={{ color, fontFamily: 'Montserrat, sans-serif' }}
                >
                  {team.score}
                </span>
              </div>
              <p className="text-[10px] font-semibold text-slate-500 uppercase tracking-wider mb-2">
                Final Score
              </p>

              {/* Track badge */}
              <div
                className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[10px] font-semibold tracking-wide border"
                style={{
                  background: `${color}08`,
                  borderColor: `${color}20`,
                  color: color,
                }}
              >
                <span className="w-1.5 h-1.5 rounded-full" style={{ background: color }} />
                {team.track}
              </div>

              {/* Team members */}
              <TeamAvatars members={team.members} color={color} />
            </div>
          );
        })}
      </div>
    </section>
  );
}
