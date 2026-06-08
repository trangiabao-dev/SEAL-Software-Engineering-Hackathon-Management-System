import React, { useState, useEffect } from 'react';
import { ChevronDown, Terminal as TerminalIcon } from 'lucide-react';

function CountdownUnit({ value, label }) {
  return (
    <div className="flex flex-col items-center">
      <div className="bg-[#0E111A] border border-white/[0.06] rounded-xl px-5 py-4 min-w-[80px] text-center shadow-[0_4px_20px_rgba(0,0,0,0.3)]">
        <span
          className="text-4xl md:text-5xl font-extrabold text-[#F26F21] tabular-nums"
          style={{ fontFamily: 'Montserrat, sans-serif' }}
        >
          {String(value).padStart(2, '0')}
        </span>
      </div>
      <span className="mt-2.5 text-[10px] font-bold tracking-widest uppercase text-slate-500">
        {label}
      </span>
    </div>
  );
}

function DeveloperMockup() {
  return (
    <div className="w-full max-w-md rounded-xl border border-white/[0.08] bg-[#0A0C12]/90 shadow-[0_24px_50px_rgba(0,0,0,0.5)] overflow-hidden float-anim">
      {/* OS Bar */}
      <div className="flex items-center justify-between px-4 py-3 bg-[#0C0F17] border-b border-white/[0.06]">
        <div className="flex items-center gap-1.5">
          <span className="w-3 h-3 rounded-full bg-[#FF5F56]/85" />
          <span className="w-3 h-3 rounded-full bg-[#FFBD2E]/85" />
          <span className="w-3 h-3 rounded-full bg-[#27C93F]/85" />
        </div>
        <div className="text-[11px] font-mono text-slate-500 flex items-center gap-1">
          <TerminalIcon className="w-3 h-3 text-[#F26F21]/80" />
          hackathon.config.ts
        </div>
        <div className="w-8" />
      </div>

      {/* Editor Content */}
      <div className="p-6 font-mono text-xs text-left text-slate-300 space-y-5 bg-[#08090E]">
        {/* Code Snippet */}
        <div>
          <span className="text-[#a78bfa]">import</span> {'{'} <span className="text-[#38b6ff]">Hackathon</span> {'}'} <span className="text-[#a78bfa]">from</span> <span className="text-green-300">"fptu-core"</span>;
          <br />
          <br />
          <span className="text-pink-400">const</span> <span className="text-blue-400">config</span> = <span className="text-[#38b6ff]">Hackathon</span>.<span>initialize</span>({'{'}
          <div className="pl-4">
            <span className="text-slate-400">theme:</span> <span className="text-green-300">"Code the Future, Build the World"</span>,
            <br />
            <span className="text-slate-400">duration:</span> <span className="text-amber-400">48</span>, <span className="text-slate-500">// hours of non-stop coding</span>
            <br />
            <span className="text-slate-400">prizes:</span> <span className="text-green-300">"100,000,000 VND Pool"</span>,
            <br />
            <span className="text-slate-400">allowAI:</span> <span className="text-[#38b6ff]">true</span>,
            <br />
            <span className="text-slate-400">mentorship:</span> <span className="text-green-300">"24/7 Industry Expert Panel"</span>
          </div>
          {'});'}
        </div>

        {/* Console Log Area */}
        <div className="h-px bg-white/[0.06]" />

        {/* Real-time stats */}
        <div className="space-y-2">
          <div className="flex items-center justify-between text-[11px] font-semibold tracking-wide">
            <span className="text-slate-500 uppercase">Registration Capacity</span>
            <span className="text-[#F26F21]">87 / 100 Teams Filled</span>
          </div>
          <div className="w-full bg-white/[0.04] h-2 rounded-full overflow-hidden border border-white/[0.04]">
            <div
              className="bg-gradient-to-r from-[#F26F21] via-orange-500 to-[#38b6ff] h-full rounded-full transition-all duration-1000"
              style={{ width: '87%' }}
            />
          </div>
        </div>

        {/* Terminal output */}
        <div className="text-[11px] text-slate-500 space-y-1.5 pt-1">
          <div>$ npm run build --release</div>
          <div className="text-green-400/90">✔ Build succeeded (2.4s)</div>
          <div className="text-green-400/90">✔ Active connections secure</div>
          <div className="text-blue-400/90">→ Live status monitoring online...</div>
        </div>
      </div>
    </div>
  );
}

export default function Hero({ onRegisterClick }) {
  const TARGET_DATE = new Date('2026-03-15T08:00:00');

  const getTimeLeft = () => {
    const diff = TARGET_DATE - new Date();
    if (diff <= 0) return { days: 0, hours: 0, minutes: 0 };
    return {
      days: Math.floor(diff / (1000 * 60 * 60 * 24)),
      hours: Math.floor((diff / (1000 * 60 * 60)) % 24),
      minutes: Math.floor((diff / (1000 * 60)) % 60),
    };
  };

  const [timeLeft, setTimeLeft] = useState(getTimeLeft());

  useEffect(() => {
    const timer = setInterval(() => setTimeLeft(getTimeLeft()), 1000 * 60);
    return () => clearInterval(timer);
  }, []);

  return (
    <section
      id="home"
      className="relative min-h-screen flex items-center bg-[#080A0F] dot-bg overflow-hidden pt-20"
    >
      {/* Soft gradient backdrops */}
      <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
        <div className="w-[800px] h-[800px] rounded-full bg-[#F26F21]/[0.02] blur-[150px]" />
      </div>
      <div className="absolute top-20 right-10 w-[400px] h-[400px] rounded-full bg-[#38b6ff]/[0.015] blur-[120px] pointer-events-none" />

      <div className="max-w-7xl mx-auto px-6 py-16 w-full grid md:grid-cols-12 gap-12 items-center relative z-10">
        {/* Text side */}
        <div className="fade-in-up md:col-span-7 space-y-8">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full border border-white/[0.06] bg-white/[0.02]">
            <span className="w-2 h-2 rounded-full bg-[#F26F21] animate-pulse" />
            <span className="text-[11px] font-semibold tracking-wider uppercase text-slate-300">
              March 15–17, 2026 · FPT University HCMC
            </span>
          </div>

          <div className="space-y-4">
            <h1
              className="text-4xl md:text-5xl lg:text-6xl font-extrabold tracking-tight text-white leading-[1.1]"
              style={{ fontFamily: 'Montserrat, sans-serif' }}
            >
              Code the{' '}
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#F26F21] to-orange-400">
                Future
              </span>
              ,<br />
              Build the{' '}
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#38b6ff] to-cyan-400">
                World
              </span>
            </h1>

            <p className="text-slate-400 text-sm md:text-base leading-relaxed max-w-lg">
              48 hours. One campus. Unlimited possibilities. Join the premier tech marathon
              at FPT University and collaborate with emerging developers to build solutions that matter.
            </p>
          </div>

          {/* Countdown */}
          <div className="space-y-3">
            <p className="text-[10px] font-bold tracking-widest uppercase text-slate-500">
              Event Starts In
            </p>
            <div className="flex items-start gap-4">
              <CountdownUnit value={timeLeft.days} label="Days" />
              <span className="text-3xl font-bold text-slate-700 mt-3">:</span>
              <CountdownUnit value={timeLeft.hours} label="Hours" />
              <span className="text-3xl font-bold text-slate-700 mt-3">:</span>
              <CountdownUnit value={timeLeft.minutes} label="Minutes" />
            </div>
          </div>

          {/* CTAs */}
          <div className="flex flex-wrap gap-4 pt-2">
            <button
              type="button"
              onClick={onRegisterClick}
              className="px-7 py-3 bg-[#F26F21] text-white font-bold tracking-wide rounded-lg hover:bg-[#e05811] shadow-[0_1px_2px_rgba(0,0,0,0.1)] transition-all duration-200 text-xs active:scale-[0.98]"
            >
              REGISTER NOW
            </button>
            <a
              href="#about"
              className="px-7 py-3 border border-white/10 text-slate-200 font-bold tracking-wide rounded-lg hover:bg-white/5 hover:text-white transition-all duration-200 text-xs flex items-center active:scale-[0.98]"
            >
              LEARN MORE
            </a>
          </div>
        </div>

        {/* Dashboard graphic side */}
        <div className="hidden md:flex items-center justify-center md:col-span-5">
          <DeveloperMockup />
        </div>
      </div>

      {/* Scroll cue */}
      <a
        href="#about"
        className="absolute bottom-6 left-1/2 -translate-x-1/2 flex flex-col items-center text-slate-500 hover:text-[#F26F21] transition-colors"
      >
        <span className="text-[10px] font-bold tracking-widest uppercase mb-1">Scroll</span>
        <ChevronDown className="w-4 h-4 animate-bounce" />
      </a>
    </section>
  );
}
