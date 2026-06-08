import React from 'react';
import { Trophy, Medal, Award } from 'lucide-react';

const prizes = [
  {
    rank: '2nd Place',
    icon: Medal,
    amount: '30,000,000',
    currency: 'VND',
    perks: ['Internship Fast-Track', 'Mentor Sessions x3', 'Tech Swag Kit'],
    color: '#94a3b8',
    borderColor: 'border-slate-500/20',
    hoverBorderColor: 'group-hover:border-slate-500/40',
    order: 'md:order-1',
    scale: '',
  },
  {
    rank: '1st Place',
    icon: Trophy,
    amount: '50,000,000',
    currency: 'VND',
    perks: ['Full Internship Offer', 'Mentor Sessions x6', 'Hardware Bundle', 'Media Coverage'],
    color: '#F26F21',
    borderColor: 'border-[#F26F21]/30',
    hoverBorderColor: 'group-hover:border-[#F26F21]/60',
    order: 'md:order-2',
    scale: 'md:scale-105',
    featured: true,
  },
  {
    rank: '3rd Place',
    icon: Award,
    amount: '15,000,000',
    currency: 'VND',
    perks: ['Resume Boost Program', 'Mentor Session x1', 'Event Certificate'],
    color: '#c97c3a',
    borderColor: 'border-amber-700/20',
    hoverBorderColor: 'group-hover:border-amber-700/40',
    order: 'md:order-3',
    scale: '',
  },
];

export default function Prizes() {
  return (
    <section id="prizes" className="relative py-28 overflow-hidden bg-[#080A0F]">
      {/* Background accents */}
      <div className="absolute top-0 right-0 w-[400px] h-[400px] rounded-full bg-[#F26F21]/[0.01] blur-[120px] pointer-events-none" />
      <div className="absolute top-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-white/5 to-transparent" />

      <div className="max-w-7xl mx-auto px-6 relative z-10">
        {/* Header */}
        <div className="text-center mb-20 space-y-4">
          <p className="text-[10px] font-bold tracking-widest uppercase text-[#F26F21]">
            REWARDS
          </p>
          <h2
            className="text-3xl md:text-4xl font-extrabold text-white tracking-tight"
            style={{ fontFamily: 'Montserrat, sans-serif' }}
          >
            Prize <span className="text-[#F26F21]">Pool</span>
          </h2>
          <p className="text-slate-400 text-sm md:text-base max-w-xl mx-auto leading-relaxed">
            Over{' '}
            <span className="text-[#F26F21] font-semibold">100,000,000 VND</span>{' '}
            in total rewards, tech packages, and career opportunities.
          </p>
        </div>

        {/* Cards */}
        <div className="grid md:grid-cols-3 gap-6 items-stretch max-w-5xl mx-auto">
          {prizes.map(({ rank, icon: Icon, amount, currency, perks, color, borderColor, hoverBorderColor, order, scale, featured }) => (
            <div
              key={rank}
              className={`group relative rounded-2xl p-8 flex flex-col items-center text-center transition-all duration-300 hover:translate-y-[-4px] bg-white/[0.01] border ${borderColor} ${hoverBorderColor} ${order} ${scale} ${
                featured ? 'bg-gradient-to-b from-[#F26F21]/[0.03] to-transparent' : ''
              }`}
            >
              {featured && (
                <div className="absolute -top-3.5 left-1/2 -translate-x-1/2 px-4 py-1 rounded-full bg-[#F26F21] text-white text-[9px] font-bold tracking-widest uppercase shadow-md">
                  GRAND PRIZE
                </div>
              )}

              {/* Icon */}
              <div
                className="w-14 h-14 rounded-xl flex items-center justify-center mb-6"
                style={{
                  background: `${color}08`,
                  border: `1px solid ${color}20`,
                }}
              >
                <Icon className="w-6 h-6" style={{ color }} />
              </div>

              {/* Rank */}
              <p
                className="text-[10px] font-bold tracking-widest uppercase mb-2"
                style={{ color }}
              >
                {rank}
              </p>

              {/* Amount */}
              <p
                className="text-3xl md:text-4xl font-extrabold text-white mb-1"
                style={{ fontFamily: 'Montserrat, sans-serif' }}
              >
                {amount}
              </p>
              <p className="text-xs font-semibold text-slate-500 mb-6 uppercase tracking-wider">{currency}</p>

              {/* Divider */}
              <div className="w-full h-px mb-6 bg-white/[0.06]" />

              {/* Perks */}
              <ul className="flex flex-col gap-3.5 w-full text-left flex-1 justify-center">
                {perks.map((perk) => (
                  <li key={perk} className="flex items-center gap-2.5 text-xs md:text-sm text-slate-400">
                    <span className="w-1.5 h-1.5 rounded-full flex-shrink-0" style={{ background: color }} />
                    <span>{perk}</span>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Special prizes note */}
        <p className="text-center text-slate-500 text-[11px] mt-12 tracking-wide font-medium">
          + Special category prizes, best UI/UX award, best social impact award, and team certificate of participation.
        </p>
      </div>
    </section>
  );
}
