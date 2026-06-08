import React from 'react';
import { Users, BarChart3, Layers, Scale } from 'lucide-react';

const statConfig = [
  {
    key: 'totalParticipants',
    label: 'Total Participants',
    icon: Users,
    color: '#F26F21',
    suffix: '',
  },
  {
    key: 'totalSubmissions',
    label: 'Projects Submitted',
    icon: BarChart3,
    color: '#38b6ff',
    suffix: '',
  },
  {
    key: 'tracksCompeted',
    label: 'Competition Tracks',
    icon: Layers,
    color: '#a78bfa',
    suffix: '',
  },
  {
    key: 'judgesInvolved',
    label: 'Expert Judges',
    icon: Scale,
    color: '#34d399',
    suffix: '',
  },
];

export default function StatisticsCards({ stats }) {
  return (
    <section className="py-12">
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 md:gap-6">
        {statConfig.map(({ key, label, icon: Icon, color }) => (
          <div
            key={key}
            className="group relative rounded-2xl p-6 md:p-8 text-center transition-all duration-300 hover:translate-y-[-2px] bg-white/[0.01] border border-white/[0.06] hover:border-white/[0.12]"
          >
            {/* Icon */}
            <div
              className="w-11 h-11 rounded-xl flex items-center justify-center mx-auto mb-4"
              style={{
                background: `${color}08`,
                border: `1px solid ${color}20`,
              }}
            >
              <Icon className="w-5 h-5" style={{ color }} />
            </div>

            {/* Stat value */}
            <p
              className="text-2xl md:text-3xl font-extrabold text-white mb-1 tabular-nums"
              style={{ fontFamily: 'Montserrat, sans-serif' }}
            >
              {stats[key]?.toLocaleString()}
            </p>

            {/* Label */}
            <p className="text-[10px] font-bold tracking-widest uppercase text-slate-500">
              {label}
            </p>
          </div>
        ))}
      </div>
    </section>
  );
}
