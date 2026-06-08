import React from "react";
import { Lightbulb, Users, Code2 } from "lucide-react";

const features = [
  {
    icon: Lightbulb,
    title: "Innovation",
    color: "#F26F21",
    desc: "Push boundaries with cutting-edge solutions. We challenge you to think differently and solve real-world problems with creativity and tech.",
  },
  {
    icon: Users,
    title: "Networking",
    color: "#38b6ff",
    desc: "Connect with top engineers, designers, and entrepreneurs. Build relationships that last far beyond the 48-hour sprint.",
  },
  {
    icon: Code2,
    title: "Coding",
    color: "#a78bfa",
    desc: "Ship production-ready code under pressure. Sharpen your skills across frontend, backend, AI, and systems programming.",
  },
];

export default function About() {
  return (
    <section id="about" className="relative py-28 overflow-hidden bg-[#080A0F]">
      {/* Background accents */}
      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-px h-28 bg-gradient-to-b from-transparent via-white/5 to-transparent" />
      <div className="absolute top-1/4 right-0 w-[350px] h-[350px] rounded-full bg-[#38b6ff]/[0.01] blur-[100px] pointer-events-none" />

      <div className="max-w-7xl mx-auto px-6 relative z-10">
        {/* Section header */}
        <div className="text-center mb-20 space-y-4">
          <p className="text-[10px] font-bold tracking-widest uppercase text-[#F26F21]">
            ABOUT THE EVENT
          </p>
          <h2
            className="text-3xl md:text-4xl font-extrabold text-white tracking-tight"
            style={{ fontFamily: "Montserrat, sans-serif" }}
          >
            What is <span className="text-[#F26F21]">FPTU Hackathon</span>?
          </h2>
          <p className="text-slate-400 max-w-2xl mx-auto text-sm md:text-base leading-relaxed">
            FPTU Hackathon 2026 is the premier 48-hour coding marathon at FPT
            University, bringing together the brightest student minds across
            Vietnam to prototype, build, and present transformative technology
            solutions — in a single weekend.
          </p>
        </div>

        {/* Feature cards */}
        <div className="grid md:grid-cols-3 gap-6">
          {features.map(({ icon: Icon, title, color, desc }) => (
            <div
              key={title}
              className="bg-white/[0.01] border border-white/[0.05] rounded-2xl p-8 hover:bg-white/[0.03] hover:border-white/[0.12] transition-all duration-300 group relative overflow-hidden"
            >
              {/* Subtle top indicator bar */}
              <div
                className="absolute top-0 left-0 w-full h-[2px] opacity-0 group-hover:opacity-100 transition-opacity duration-300"
                style={{ backgroundColor: color }}
              />

              {/* Icon container */}
              <div
                className="w-12 h-12 rounded-xl flex items-center justify-center mb-6 transition-transform duration-300 group-hover:scale-105"
                style={{
                  background: `${color}08`,
                  border: `1px solid ${color}20`,
                }}
              >
                <Icon className="w-5 h-5" style={{ color }} />
              </div>

              <h3
                className="text-lg font-bold text-white mb-3 tracking-wide"
                style={{ fontFamily: "Montserrat, sans-serif" }}
              >
                {title}
              </h3>

              <p className="text-slate-400 text-xs md:text-sm leading-relaxed">
                {desc}
              </p>
            </div>
          ))}
        </div>

        {/* Stats bar */}
        <div className="mt-16 bg-white/[0.01] border border-white/[0.05] rounded-2xl p-8 grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
          {[
            { val: "500+", label: "Participants" },
            { val: "48h", label: "Hacking Time" },
            { val: "50+", label: "Mentors" },
            { val: "100M+", label: "Prize Pool (VND)" },
          ].map(({ val, label }) => (
            <div key={label} className="space-y-1">
              <p
                className="text-3xl font-extrabold text-[#F26F21]"
                style={{ fontFamily: "Montserrat, sans-serif" }}
              >
                {val}
              </p>
              <p className="text-[10px] font-bold tracking-widest uppercase text-slate-500">
                {label}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
