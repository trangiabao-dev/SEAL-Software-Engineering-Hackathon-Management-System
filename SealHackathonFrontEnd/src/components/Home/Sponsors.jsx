import React from 'react';

const tiers = [
  {
    tier: 'Platinum',
    color: '#e2e8f0',
    sponsors: ['FPT Software', 'FPT Telecom'],
    size: 'text-xl md:text-2xl',
  },
  {
    tier: 'Gold',
    color: '#F26F21',
    sponsors: ['VNG Corporation', 'Grab Vietnam', 'Momo'],
    size: 'text-lg md:text-xl',
  },
  {
    tier: 'Silver',
    color: '#38b6ff',
    sponsors: ['Axon Active', 'Rikkeisoft', 'TMA Solutions', 'KMS Technology'],
    size: 'text-sm md:text-base',
  },
];

export default function Sponsors() {
  return (
    <section id="sponsors" className="relative py-28 overflow-hidden bg-[#080A0F]">
      <div className="absolute top-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-white/5 to-transparent" />
      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[500px] h-[300px] rounded-full bg-[#38b6ff]/[0.01] blur-[120px] pointer-events-none" />

      <div className="max-w-7xl mx-auto px-6 relative z-10">
        {/* Header */}
        <div className="text-center mb-20 space-y-4">
          <p className="text-[10px] font-bold tracking-widest uppercase text-[#38b6ff]">
            PARTNERS
          </p>
          <h2
            className="text-3xl md:text-4xl font-extrabold text-white tracking-tight"
            style={{ fontFamily: 'Montserrat, sans-serif' }}
          >
            Our <span className="text-[#F26F21]">Sponsors</span>
          </h2>
        </div>

        {/* Tiers */}
        <div className="flex flex-col gap-16 max-w-5xl mx-auto">
          {tiers.map(({ tier, color, sponsors, size }) => (
            <div key={tier} className="text-center space-y-8">
              {/* Tier label */}
              <div className="flex items-center gap-6 justify-center">
                <div className="flex-1 max-w-[100px] h-px bg-white/[0.08]" />
                <span
                  className="text-[10px] font-bold tracking-widest uppercase px-3.5 py-1 rounded-full border"
                  style={{
                    color,
                    borderColor: `${color}30`,
                    background: `${color}06`,
                  }}
                >
                  {tier} Sponsor
                </span>
                <div className="flex-1 max-w-[100px] h-px bg-white/[0.08]" />
              </div>

              {/* Logos */}
              <div className="flex flex-wrap justify-center gap-4.5">
                {sponsors.map((name) => (
                  <div
                    key={name}
                    className="bg-white/[0.01] border border-white/[0.05] rounded-xl px-8 py-5 transition-all duration-300 hover:border-white/[0.12] hover:bg-white/[0.03] hover:scale-[1.02] cursor-pointer group"
                  >
                    <span
                      className={`font-black uppercase tracking-wider ${size} text-slate-400 group-hover:text-white transition-colors duration-300`}
                      style={{ fontFamily: 'Montserrat, sans-serif' }}
                    >
                      {name}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>

        {/* Become a sponsor CTA */}
        <div className="mt-24 text-center bg-white/[0.01] border border-white/[0.05] rounded-2xl p-10 max-w-3xl mx-auto">
          <p
            className="text-lg font-bold text-white mb-2"
            style={{ fontFamily: 'Montserrat, sans-serif' }}
          >
            Want to join us as a Sponsor?
          </p>
          <p className="text-slate-400 text-xs md:text-sm mb-8 max-w-md mx-auto leading-relaxed">
            Reach 500+ top-tier computer science students and emerging developers at Vietnam's leading tech campus.
          </p>
          <a
            href="mailto:hackathon@fpt.edu.vn"
            className="inline-flex items-center gap-2 px-6 py-2.5 border border-white/10 hover:border-white/20 text-slate-200 hover:text-white font-semibold tracking-wide rounded-lg hover:bg-white/5 transition-all duration-200 text-xs active:scale-[0.98]"
          >
            Get Sponsorship Deck
          </a>
        </div>
      </div>
    </section>
  );
}
