import { Zap, Clock, Star, ChevronRight } from 'lucide-react';

const difficultyMap = {
  Hard: { label: 'Hard', classes: 'text-red-600', bg: '#FEE2E2', border: '#FCA5A5' },
  Medium: { label: 'Medium', classes: 'text-amber-600', bg: '#FEF3C7', border: '#FDE68A' },
  Easy: { label: 'Easy', classes: 'text-emerald-600', bg: '#D1FAE5', border: '#A7F3D0' },
};

const challenge = {
  id: 'CH-001',
  title: 'AI-Powered Smart City Solution',
  description: 'Build an intelligent platform that leverages real-time data streams to optimize urban infrastructure, reduce energy waste, and improve citizen quality of life using modern AI/ML techniques.',
  difficulty: 'Hard',
  track: 'AI & Machine Learning',
  timeLeft: '47h 30m',
  points: 1500,
  tags: ['AI/ML', 'IoT', 'Smart City', 'Real-time'],
};

export function ChallengeCard() {
  const diff = difficultyMap[challenge.difficulty];

  return (
    <div className="rounded-2xl p-6 bg-white border border-[#FFD0B5] shadow-[0_10px_30px_rgba(242,111,33,0.04),0_1px_3px_rgba(0,0,0,0.01)] transition-all duration-300 hover:translate-y-[-2px] hover:shadow-[0_16px_40px_rgba(242,111,33,0.08)] hover:border-[#FFD0B5]/85">
      {/* Header row */}
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 rounded-lg flex items-center justify-center"
            style={{ background: 'rgba(242,111,33,0.1)', border: '1px solid rgba(242,111,33,0.2)' }}>
            <Zap className="w-4 h-4" style={{ color: '#F26F21' }} />
          </div>
          <div>
            <p className="text-[10px] font-semibold uppercase tracking-widest text-slate-400">Active Challenge</p>
            <p className="text-[11px] font-semibold" style={{ color: '#F26F21' }}>{challenge.id} &mdash; {challenge.track}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className="px-2.5 py-1 rounded-lg text-[11px] font-bold" style={{ background: diff.bg, border: `1px solid ${diff.border}` }}>
            <span className={diff.classes}>{diff.label}</span>
          </span>
        </div>
      </div>

      {/* Title */}
      <h2 className="text-lg font-bold text-[#111827] mb-2 leading-snug tracking-tight"
        style={{ fontFamily: "'Montserrat', 'Inter', sans-serif" }}>
        {challenge.title}
      </h2>
      <p className="text-sm text-slate-600 leading-relaxed mb-5">{challenge.description}</p>

      {/* Tags */}
      <div className="flex flex-wrap gap-2 mb-5">
        {challenge.tags.map(tag => (
          <span key={tag} className="px-2.5 py-1 rounded-lg text-[11px] font-semibold uppercase tracking-wider"
            style={{ background: '#F3F4F6', border: '1px solid #E5E7EB', color: '#4B5563' }}>
            {tag}
          </span>
        ))}
      </div>

      {/* Stats row */}
      <div className="flex items-center justify-between pt-4 border-t"
        style={{ borderColor: '#E5E7EB' }}>
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-1.5 text-slate-500 text-xs">
            <Clock className="w-3.5 h-3.5" style={{ color: '#F26F21' }} />
            <span className="font-semibold text-[#111827]">{challenge.timeLeft}</span>
            <span>left</span>
          </div>
          <div className="flex items-center gap-1.5 text-slate-500 text-xs">
            <Star className="w-3.5 h-3.5 text-amber-500" />
            <span className="font-semibold text-[#111827]">{challenge.points.toLocaleString()}</span>
            <span>pts</span>
          </div>
        </div>
        <button
          className="flex items-center gap-1.5 px-4 py-2 rounded-xl text-sm font-semibold text-white bg-gradient-to-r from-[#F26F21] to-[#c9520e] shadow-[0_4px_14px_rgba(242,111,33,0.25)] hover:shadow-[0_6px_20px_rgba(242,111,33,0.38)] hover:-translate-y-0.5 active:scale-[0.98] active:translate-y-0 transition-all duration-200"
        >
          View Details
          <ChevronRight className="w-3.5 h-3.5" />
        </button>
      </div>
    </div>
  );
}
