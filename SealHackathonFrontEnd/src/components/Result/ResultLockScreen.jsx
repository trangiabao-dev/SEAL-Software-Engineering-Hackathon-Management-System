import React, { useState, useEffect } from 'react';
import { Trophy, Lock, ShieldCheck, Clock } from 'lucide-react';

function CountdownUnit({ value, label }) {
  return (
    <div className="flex flex-col items-center">
      <div className="bg-[#0E111A] border border-white/[0.06] rounded-xl px-4 sm:px-5 py-3 sm:py-4 min-w-[64px] sm:min-w-[80px] text-center shadow-[0_4px_20px_rgba(0,0,0,0.3)]">
        <span
          className="text-3xl sm:text-4xl md:text-5xl font-extrabold text-[#F26F21] tabular-nums"
          style={{ fontFamily: 'Montserrat, sans-serif' }}
        >
          {String(value).padStart(2, '0')}
        </span>
      </div>
      <span className="mt-2 text-[9px] sm:text-[10px] font-bold tracking-widest uppercase text-slate-500">
        {label}
      </span>
    </div>
  );
}

const reviewSteps = [
  { label: 'Submissions Collected', done: true },
  { label: 'Judge Scoring Complete', done: true },
  { label: 'Final Review in Progress', done: false },
  { label: 'Results Published', done: false },
];

export default function ResultLockScreen({ announcementTime }) {
  const getTimeLeft = () => {
    const diff = new Date(announcementTime) - new Date();
    if (diff <= 0) return { days: 0, hours: 0, minutes: 0, seconds: 0 };
    return {
      days: Math.floor(diff / (1000 * 60 * 60 * 24)),
      hours: Math.floor((diff / (1000 * 60 * 60)) % 24),
      minutes: Math.floor((diff / (1000 * 60)) % 60),
      seconds: Math.floor((diff / 1000) % 60),
    };
  };

  const [timeLeft, setTimeLeft] = useState(getTimeLeft());

  useEffect(() => {
    const timer = setInterval(() => setTimeLeft(getTimeLeft()), 1000);
    return () => clearInterval(timer);
  }, [announcementTime]);

  return (
    <div className="min-h-screen bg-[#080A0F] dot-bg flex items-center justify-center relative overflow-hidden">
      {/* Ambient glows */}
      <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
        <div className="w-[600px] h-[600px] rounded-full bg-[#F26F21]/[0.02] blur-[150px]" />
      </div>
      <div className="absolute top-20 left-10 w-[300px] h-[300px] rounded-full bg-[#38b6ff]/[0.015] blur-[120px] pointer-events-none" />

      <div className="relative z-10 text-center px-6 max-w-2xl mx-auto fade-in-up">
        {/* Lock icon with glow */}
        <div className="mx-auto mb-8 w-24 h-24 rounded-2xl flex items-center justify-center bg-[#F26F21]/[0.06] border border-[#F26F21]/20 shadow-[0_0_40px_rgba(242,111,33,0.08)]">
          <Trophy className="w-10 h-10 text-[#F26F21]" />
        </div>

        {/* Title */}
        <h1
          className="text-3xl sm:text-4xl md:text-5xl font-extrabold text-white tracking-tight mb-4"
          style={{ fontFamily: 'Montserrat, sans-serif' }}
        >
          Results Are Being{' '}
          <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#F26F21] to-orange-400">
            Finalized
          </span>
        </h1>

        <p className="text-slate-400 text-sm md:text-base leading-relaxed max-w-lg mx-auto mb-10">
          The organizing committee is reviewing all submissions and scores.
          Official results will be announced once the review process is complete.
        </p>

        {/* Countdown */}
        <div className="space-y-4 mb-12">
          <div className="flex items-center justify-center gap-2 text-slate-500">
            <Clock className="w-3.5 h-3.5" />
            <p className="text-[10px] font-bold tracking-widest uppercase">
              Announcement In
            </p>
          </div>
          <div className="flex items-start justify-center gap-2 sm:gap-4">
            <CountdownUnit value={timeLeft.days} label="Days" />
            <span className="text-2xl sm:text-3xl font-bold text-slate-700 mt-2 sm:mt-3">:</span>
            <CountdownUnit value={timeLeft.hours} label="Hours" />
            <span className="text-2xl sm:text-3xl font-bold text-slate-700 mt-2 sm:mt-3">:</span>
            <CountdownUnit value={timeLeft.minutes} label="Min" />
            <span className="text-2xl sm:text-3xl font-bold text-slate-700 mt-2 sm:mt-3">:</span>
            <CountdownUnit value={timeLeft.seconds} label="Sec" />
          </div>
        </div>

        {/* Review progress */}
        <div className="max-w-sm mx-auto">
          <div className="flex flex-col gap-3">
            {reviewSteps.map((step, i) => (
              <div key={step.label} className="flex items-center gap-3">
                <div
                  className={`w-7 h-7 rounded-lg flex items-center justify-center flex-shrink-0 border ${
                    step.done
                      ? 'bg-[#F26F21]/10 border-[#F26F21]/30'
                      : 'bg-white/[0.02] border-white/[0.06]'
                  }`}
                >
                  {step.done ? (
                    <ShieldCheck className="w-3.5 h-3.5 text-[#F26F21]" />
                  ) : (
                    <Lock className="w-3 h-3 text-slate-600" />
                  )}
                </div>
                <span
                  className={`text-xs font-medium ${
                    step.done ? 'text-slate-300' : 'text-slate-600'
                  }`}
                >
                  {step.label}
                </span>
                {i === 2 && !step.done && (
                  <span className="ml-auto text-[9px] font-bold tracking-wider uppercase text-[#F26F21] animate-pulse">
                    IN PROGRESS
                  </span>
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Bottom hint */}
        <p className="mt-12 text-[11px] text-slate-600 tracking-wide">
          This page will automatically update when results are published.
        </p>
      </div>
    </div>
  );
}
