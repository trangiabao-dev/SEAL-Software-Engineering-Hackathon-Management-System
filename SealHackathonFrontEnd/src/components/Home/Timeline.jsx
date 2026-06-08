import React from "react";
import { ClipboardList, Mic, Terminal, Monitor } from "lucide-react";

const stages = [
  {
    icon: ClipboardList,
    phase: "Phase 01",
    title: "Registration",
    date: "Jan 15 – Feb 28, 2026",
    color: "#F26F21",
    desc: "Form your team (2–5 members) and register online. Submit your initial concept and tech stack.",
  },
  {
    icon: Mic,
    phase: "Phase 02",
    title: "Opening Ceremony",
    date: "March 15, 2026",
    color: "#38b6ff",
    desc: "Kick off with keynote speakers, sponsor presentations, theme reveal, and team networking mixer.",
  },
  {
    icon: Terminal,
    phase: "Phase 03",
    title: "Hacking 48h",
    date: "March 15–17, 2026",
    color: "#a78bfa",
    desc: "Two days of non-stop building. Mentors on-site, workshops, meals provided. Code, iterate, and ship.",
  },
  {
    icon: Monitor,
    phase: "Phase 04",
    title: "Demo Day",
    date: "March 17, 2026",
    color: "#34d399",
    desc: "Present your project to judges from top tech companies. Winners announced at the closing ceremony.",
  },
];

export default function Timeline() {
  return (
    <section id="timeline" className="relative py-28 overflow-hidden bg-[#080A0F]">
      {/* Background glow */}
      <div className="absolute bottom-0 left-0 w-[400px] h-[400px] rounded-full bg-[#F26F21]/[0.01] blur-[120px] pointer-events-none" />

      <div className="max-w-7xl mx-auto px-6 relative z-10">
        {/* Header */}
        <div className="text-center mb-20 space-y-4">
          <p className="text-[10px] font-bold tracking-widest uppercase text-[#F26F21]">
            SCHEDULE
          </p>
          <h2
            className="text-3xl md:text-4xl font-extrabold text-white tracking-tight"
            style={{ fontFamily: "Montserrat, sans-serif" }}
          >
            Event <span className="text-[#F26F21]">Timeline</span>
          </h2>
        </div>

        {/* Desktop horizontal timeline */}
        <div className="hidden lg:block">
          {/* Connector line */}
          <div className="relative flex items-start justify-between gap-4">
            <div className="absolute top-[22px] left-[10%] right-[10%] h-[1px] bg-white/[0.08]" />

            {stages.map(
              (
                { icon: Icon, phase, title, date, color, desc },
                idx,
              ) => (
                <div
                  key={idx}
                  className="relative flex flex-col items-center flex-1 group"
                >
                  {/* Node */}
                  <div
                    className="relative z-10 w-11 h-11 rounded-full flex items-center justify-center mb-6 transition-all duration-300 group-hover:scale-105 bg-[#0C0F17] border"
                    style={{
                      borderColor: `${color}40`,
                    }}
                  >
                    <Icon className="w-4.5 h-4.5" style={{ color }} />
                  </div>

                  {/* Card */}
                  <div className="bg-white/[0.01] border border-white/[0.05] rounded-xl p-5 w-full transition-all duration-300 group-hover:border-white/[0.12] group-hover:bg-white/[0.02]">
                    <p
                      className="text-[10px] font-bold tracking-wider uppercase mb-1"
                      style={{ color }}
                    >
                      {phase}
                    </p>
                    <h3
                      className="text-sm font-bold text-white mb-0.5"
                      style={{ fontFamily: "Montserrat, sans-serif" }}
                    >
                      {title}
                    </h3>
                    <p className="text-[11px] text-slate-500 mb-3">{date}</p>
                    <p className="text-xs text-slate-400 leading-relaxed">
                      {desc}
                    </p>
                  </div>
                </div>
              ),
            )}
          </div>
        </div>

        {/* Mobile vertical timeline */}
        <div className="lg:hidden flex flex-col gap-0">
          {stages.map(
            ({ icon: Icon, phase, title, date, color, desc }, idx) => (
              <div key={idx} className="flex gap-5">
                {/* Left: node + line */}
                <div className="flex flex-col items-center">
                  <div
                    className="w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0 bg-[#0C0F17] border"
                    style={{
                      borderColor: `${color}40`,
                    }}
                  >
                    <Icon className="w-4.5 h-4.5" style={{ color }} />
                  </div>
                  {idx < stages.length - 1 && (
                    <div
                      className="w-[1px] flex-1 my-2 bg-white/[0.08]"
                    />
                  )}
                </div>

                {/* Right: card */}
                <div className="bg-white/[0.01] border border-white/[0.05] rounded-xl p-5 mb-4 flex-1">
                  <p
                    className="text-[10px] font-bold tracking-wider uppercase mb-1"
                    style={{ color }}
                  >
                    {phase}
                  </p>
                  <h3
                    className="text-sm font-bold text-white mb-0.5"
                    style={{ fontFamily: "Montserrat, sans-serif" }}
                  >
                    {title}
                  </h3>
                  <p className="text-[11px] text-slate-500 mb-2">{date}</p>
                  <p className="text-xs text-slate-400 leading-relaxed">
                    {desc}
                  </p>
                </div>
              </div>
            ),
          )}
        </div>
      </div>
    </section>
  );
}
