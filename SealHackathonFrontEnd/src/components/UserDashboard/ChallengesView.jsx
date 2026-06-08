import { ChallengeCard } from './ChallengeCard';
import { Flame, Award, Lock } from 'lucide-react';

const otherChallenges = [
  { id: 'CH-002', title: 'Blockchain Supply Chain', difficulty: 'Medium', track: 'Web3', points: 1000, locked: false },
  { id: 'CH-003', title: 'Sustainable Energy Dashboard', difficulty: 'Easy', track: 'GreenTech', points: 700, locked: false },
  { id: 'CH-004', title: 'Quantum Algorithm Optimizer', difficulty: 'Hard', track: 'Quantum', points: 2000, locked: true },
];

const difficultyColor = {
  Hard: '#DC2626',
  Medium: '#D97706',
  Easy: '#059669',
};

export function ChallengesView() {
  return (
    <div className="space-y-6">
      {/* Active challenge */}
      <div>
        <div className="flex items-center gap-2 mb-3">
          <Flame className="w-4 h-4" style={{ color: '#F26F21' }} />
          <h2 className="text-sm font-bold uppercase tracking-widest" style={{ color: '#F26F21' }}>Active Challenge</h2>
        </div>
        <ChallengeCard />
      </div>

      {/* Other challenges */}
      <div>
        <div className="flex items-center gap-2 mb-3">
          <Award className="w-4 h-4 text-slate-500" />
          <h2 className="text-sm font-bold uppercase tracking-widest text-slate-500">Other Tracks</h2>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          {otherChallenges.map(ch => (
            <div key={ch.id}
               className={`rounded-xl p-4 transition-all duration-250 cursor-pointer border ${
                 ch.locked
                   ? 'bg-[#F3F4F6] border-[#E5E7EB] opacity-60'
                   : 'bg-white border-[#E5E7EB] shadow-[0_4px_16px_rgba(0,0,0,0.02)] hover:border-[#F26F21]/60 hover:-translate-y-0.5 hover:shadow-[0_8px_24px_rgba(242,111,33,0.04)] active:scale-[0.99]'
               }`}
            >
              <div className="flex items-start justify-between mb-2">
                <span className="text-[10px] font-semibold text-slate-400 uppercase tracking-wider">{ch.id}</span>
                {ch.locked
                  ? <Lock className="w-3.5 h-3.5 text-slate-400" />
                  : <span className="text-[10px] font-bold" style={{ color: difficultyColor[ch.difficulty] }}>{ch.difficulty}</span>
                }
              </div>
              <p className="text-sm font-semibold text-[#111827] mb-1 leading-snug">{ch.title}</p>
              <p className="text-[11px] text-slate-500">{ch.track}</p>
              <div className="mt-3 pt-3 border-t flex items-center justify-between"
                style={{ borderColor: '#F3F4F6' }}>
                <span className="text-xs text-slate-600 font-medium">{ch.points.toLocaleString()} pts</span>
                {!ch.locked && (
                  <span className="text-[10px] font-semibold uppercase tracking-wider" style={{ color: '#F26F21' }}>View</span>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
