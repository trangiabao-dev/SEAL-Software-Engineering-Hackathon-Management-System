import { Users, Circle, Activity } from 'lucide-react';

const members = [
  { name: 'Nguyen Van A', role: 'Leader', initials: 'NA', online: true, color: '#F26F21', activity: 'Pushed commit 3m ago' },
  { name: 'Tran Thi B', role: 'Developer', initials: 'TB', online: true, color: '#6366f1', activity: 'Reviewing PR 12m ago' },
  { name: 'Le Van C', role: 'Designer', initials: 'LC', online: false, color: '#06b6d4', activity: 'Updated mockups 1h ago' },
  { name: 'Pham Thi D', role: 'Developer', initials: 'PD', online: true, color: '#10b981', activity: 'Fixed bug 25m ago' },
];

const roleColors = {
  Leader: { bg: '#FFF6F0', border: '#FFD0B5', text: '#F26F21' },
  Developer: { bg: '#EEF2FF', border: '#C7D2FE', text: '#4F46E5' },
  Designer: { bg: '#ECFEFF', border: '#A5F3FC', text: '#0891B2' },
};

const activityLog = [
  { time: '14:32', msg: 'Team Alpha submitted a draft', accent: '#F26F21' },
  { time: '13:55', msg: 'New message in challenge Q&A', accent: '#6366f1' },
  { time: '12:10', msg: 'Challenge rules updated by organizers', accent: '#f59e0b' },
  { time: '10:00', msg: 'Hackathon officially started', accent: '#10b981' },
];

export function TeamStatus() {
  return (
    <div className="rounded-2xl p-6 transition-all duration-300"
      style={{
        background: '#FFFFFF',
        border: '1px solid #E5E7EB',
        boxShadow: '0 10px 30px rgba(0,0,0,0.02)',
      }}>
      <div className="flex items-center gap-3 mb-5">
        <div className="w-8 h-8 rounded-lg flex items-center justify-center"
          style={{ background: 'rgba(242,111,33,0.1)', border: '1px solid rgba(242,111,33,0.2)' }}>
          <Users className="w-4 h-4" style={{ color: '#F26F21' }} />
        </div>
        <div>
          <h3 className="text-sm font-bold text-[#111827]">Team Status</h3>
          <p className="text-[11px] text-slate-500">
            <span className="text-emerald-600 font-semibold">{members.filter(m => m.online).length}</span> of {members.length} online
          </p>
        </div>
      </div>

      {/* Members */}
      <div className="space-y-2 mb-5">
        {members.map(m => {
          const rc = roleColors[m.role] || roleColors['Developer'];
          return (
            <div key={m.name} className="flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200"
              style={{ background: '#F9FAFB', border: '1px solid #E5E7EB' }}
              onMouseEnter={e => { e.currentTarget.style.background = '#F3F4F6'; }}
              onMouseLeave={e => { e.currentTarget.style.background = '#F9FAFB'; }}
            >
              {/* Avatar */}
              <div className="relative w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold text-white flex-shrink-0"
                style={{ background: m.color }}>
                {m.initials}
                <span className="absolute bottom-0 right-0 w-2.5 h-2.5 rounded-full border-2 border-white"
                  style={{ background: m.online ? '#22c55e' : '#9ca3af' }} />
              </div>
              {/* Info */}
              <div className="flex-1 min-w-0">
                <p className="text-xs font-semibold text-[#111827] truncate">{m.name}</p>
                <p className="text-[10px] text-slate-500 truncate">{m.activity}</p>
              </div>
              {/* Role badge */}
              <span className="px-2 py-0.5 rounded-md text-[10px] font-bold uppercase tracking-wider flex-shrink-0"
                style={{ background: rc.bg, border: `1px solid ${rc.border}`, color: rc.text }}>
                {m.role}
              </span>
            </div>
          );
        })}
      </div>

      {/* Activity Log */}
      <div className="border-t pt-4" style={{ borderColor: '#E5E7EB' }}>
        <div className="flex items-center gap-2 mb-3">
          <Activity className="w-3.5 h-3.5" style={{ color: '#F26F21' }} />
          <p className="text-[11px] font-semibold uppercase tracking-widest text-[#F26F21]">Activity Log</p>
        </div>
        <div className="space-y-2.5">
          {activityLog.map((entry, i) => (
            <div key={i} className="flex items-start gap-3">
              <div className="w-1.5 h-1.5 rounded-full mt-1.5 flex-shrink-0" style={{ background: entry.accent }} />
              <div className="flex-1">
                <p className="text-xs text-slate-600 leading-snug">{entry.msg}</p>
              </div>
              <p className="text-[10px] text-slate-400 flex-shrink-0 font-mono">{entry.time}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
