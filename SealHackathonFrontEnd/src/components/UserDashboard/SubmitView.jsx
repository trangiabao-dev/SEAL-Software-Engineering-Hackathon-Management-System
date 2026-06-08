import { SubmissionZone } from './SubmissionZone';
import { CheckCircle, Clock, AlertCircle } from 'lucide-react';

const timeline = [
  { label: 'Project registered', done: true, time: '09:00' },
  { label: 'Draft submitted', done: true, time: '11:30' },
  { label: 'Final submission', done: false, time: 'Pending' },
  { label: 'Judging begins', done: false, time: '18:00' },
];

export function SubmitView() {
  return (
    <div className="space-y-6">
      {/* Alert */}
      <div className="flex items-start gap-3 px-4 py-3.5 rounded-xl"
        style={{ background: '#FFFBEB', border: '1px solid #FDE68A' }}>
        <AlertCircle className="w-4 h-4 text-amber-600 flex-shrink-0 mt-0.5" />
        <div>
          <p className="text-xs font-bold text-amber-900 mb-0.5">Final Submission Deadline Approaching</p>
          <p className="text-xs text-amber-700">Make sure your project is complete and all files are uploaded before the deadline. Late submissions will not be accepted.</p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
        {/* Submission form — wider */}
        <div className="lg:col-span-3">
          <SubmissionZone />
        </div>

        {/* Timeline */}
        <div className="lg:col-span-2 rounded-2xl p-5"
          style={{
            background: '#FFFFFF',
            border: '1px solid #E5E7EB',
            boxShadow: '0 10px 30px rgba(0,0,0,0.02)',
          }}>
          <div className="flex items-center gap-2 mb-5">
            <Clock className="w-4 h-4" style={{ color: '#F26F21' }} />
            <h3 className="text-sm font-bold text-[#111827]">Submission Timeline</h3>
          </div>
          <div className="relative pl-5 space-y-6">
            {/* vertical line */}
            <div className="absolute left-[7px] top-1 bottom-1 w-px" style={{ background: '#E5E7EB' }} />
            {timeline.map((step, i) => (
              <div key={i} className="relative flex items-start gap-3">
                <div className="absolute -left-5 w-3.5 h-3.5 rounded-full flex items-center justify-center border flex-shrink-0"
                  style={{
                    background: step.done ? '#D1FAE5' : '#F3F4F6',
                    borderColor: step.done ? '#10B981' : '#E5E7EB',
                    boxShadow: step.done ? '0 0 8px rgba(16,185,129,0.2)' : 'none',
                  }}>
                  {step.done && <div className="w-1.5 h-1.5 rounded-full bg-emerald-500" />}
                </div>
                <div className="flex-1">
                  <p className="text-xs font-semibold"
                    style={{ color: step.done ? '#374151' : '#9CA3AF' }}>{step.label}</p>
                  <p className="text-[10px] mt-0.5 font-mono"
                    style={{ color: step.done ? '#059669' : '#9CA3AF' }}>{step.time}</p>
                </div>
              </div>
            ))}
          </div>

          {/* Score preview */}
          <div className="mt-6 p-4 rounded-xl" style={{ background: '#FFF6F0', border: '1px solid #FFD0B5' }}>
            <p className="text-[10px] font-semibold uppercase tracking-widest text-[#F26F21] mb-2">Current Score Preview</p>
            <div className="flex items-end gap-1">
              <span className="text-3xl font-black text-[#111827]" style={{ fontFamily: "'Montserrat', sans-serif" }}>850</span>
              <span className="text-sm text-slate-500 mb-1">/ 1500 pts</span>
            </div>
            <div className="mt-2 h-1.5 rounded-full overflow-hidden" style={{ background: '#E5E7EB' }}>
              <div className="h-full rounded-full transition-all duration-700"
                style={{ width: '56.7%', background: 'linear-gradient(90deg, #F26F21, #f59e0b)' }} />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
