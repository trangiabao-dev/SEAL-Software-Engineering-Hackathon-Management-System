import React, { useState } from 'react';
import { ChevronDown } from 'lucide-react';

const faqs = [
  {
    q: 'Who can participate in FPTU Hackathon 2026?',
    a: 'The event is open to all currently enrolled students at FPT University campuses across Vietnam. Teams of 2–5 members are required. Students from any major are welcome.',
  },
  {
    q: 'Is there a registration fee?',
    a: 'No, participation is completely free. We provide meals, snacks, workspaces, and mentorship at no cost to participants.',
  },
  {
    q: 'What should I bring?',
    a: 'Bring your laptop, chargers, and any hardware you plan to use. We provide high-speed internet, power outlets, and collaborative workspace.',
  },
  {
    q: 'Do I need to have a project idea before registering?',
    a: 'No pre-formed idea is required. The theme will be revealed at the Opening Ceremony. You should have your team ready and a range of skills covered.',
  },
  {
    q: 'How are projects judged?',
    a: 'Projects are evaluated on Innovation & Creativity, Technical Complexity, Impact & Feasibility, and Quality of Presentation by a panel of industry experts.',
  },
  {
    q: 'Can I use AI tools and open-source libraries?',
    a: 'Yes! You may use any open-source libraries, APIs, and AI tools. The key requirement is that the core solution must be built during the 48-hour hacking period.',
  },
];

function FAQItem({ q, a, index }) {
  const [open, setOpen] = useState(false);
  return (
    <div
      className="border-b border-white/[0.05] last:border-0"
    >
      <button
        onClick={() => setOpen(!open)}
        className="w-full flex items-center justify-between py-5 text-left group"
        aria-expanded={open}
      >
        <span
          className="font-bold text-white group-hover:text-[#F26F21] transition-colors text-sm md:text-base flex items-start"
          style={{ fontFamily: 'Montserrat, sans-serif' }}
        >
          <span className="text-[#F26F21]/80 mr-3.5 font-extrabold text-xs md:text-sm pt-0.5">
            {String(index + 1).padStart(2, '0')}
          </span>
          <span>{q}</span>
        </span>
        <ChevronDown
          className={`w-4 h-4 flex-shrink-0 ml-4 transition-transform duration-300 ${open ? 'rotate-180 text-[#F26F21]' : 'text-slate-500 group-hover:text-slate-300'}`}
        />
      </button>
      <div
        className={`grid transition-all duration-300 ease-in-out ${
          open ? 'grid-rows-[1fr] opacity-100 pb-5' : 'grid-rows-[0fr] opacity-0 pointer-events-none'
        }`}
      >
        <div className="overflow-hidden">
          <p className="text-slate-400 text-xs md:text-sm leading-relaxed pl-7 pr-4">
            {a}
          </p>
        </div>
      </div>
    </div>
  );
}

export default function FAQ() {
  return (
    <section id="faq" className="relative py-28 overflow-hidden bg-[#080A0F]">
      <div className="absolute top-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-white/5 to-transparent" />

      <div className="max-w-4xl mx-auto px-6 relative z-10">
        {/* Header */}
        <div className="text-center mb-20 space-y-4">
          <p className="text-[10px] font-bold tracking-widest uppercase text-[#F26F21]">
            QUESTIONS & ANSWERS
          </p>
          <h2
            className="text-3xl md:text-4xl font-extrabold text-white tracking-tight"
            style={{ fontFamily: 'Montserrat, sans-serif' }}
          >
            FAQ
          </h2>
        </div>

        <div className="bg-white/[0.01] border border-white/[0.05] rounded-2xl p-6 md:p-10">
          {faqs.map((item, i) => (
            <FAQItem key={i} q={item.q} a={item.a} index={i} />
          ))}
        </div>

        <p className="text-center text-slate-500 text-xs mt-10">
          Still have questions?{' '}
          <a href="mailto:hackathon@fpt.edu.vn" className="text-[#F26F21] hover:text-[#e05811] font-semibold underline underline-offset-4 transition-colors">
            Email the organizing team
          </a>
        </p>
      </div>
    </section>
  );
}
