import React, { useState, useEffect } from 'react';
import { Trophy, Calendar, ArrowLeft } from 'lucide-react';
import ResultLockScreen from '../components/Result/ResultLockScreen';
import PodiumSection from '../components/Result/PodiumSection';
import RankingTable from '../components/Result/RankingTable';
import StatisticsCards from '../components/Result/StatisticsCards';
import {
  announcementTime,
  teams,
  eventStats,
  currentUserTeamId,
} from '../components/Result/mockData';

export default function ResultPage() {
  const [isPublished, setIsPublished] = useState(
    new Date() >= new Date(announcementTime)
  );

  // Re-check every second so the page auto-transitions from locked → published
  useEffect(() => {
    if (isPublished) return;
    const timer = setInterval(() => {
      if (new Date() >= new Date(announcementTime)) {
        setIsPublished(true);
      }
    }, 1000);
    return () => clearInterval(timer);
  }, [isPublished]);

  /* ── STATE 1: LOCKED ── */
  if (!isPublished) {
    return <ResultLockScreen announcementTime={announcementTime} />;
  }

  /* ── STATE 2: PUBLISHED ── */
  return (
    <div className="min-h-screen bg-[#080A0F] dot-bg relative overflow-x-hidden">
      {/* Ambient glows */}
      <div className="absolute inset-0 flex items-center justify-start pointer-events-none">
        <div className="w-[700px] h-[700px] rounded-full bg-[#F26F21]/[0.015] blur-[180px]" />
      </div>
      <div className="absolute top-0 right-0 w-[500px] h-[500px] rounded-full bg-[#38b6ff]/[0.01] blur-[150px] pointer-events-none" />
      <div className="absolute top-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-white/5 to-transparent" />

      <div className="relative z-10 max-w-7xl mx-auto px-6 py-12 md:py-16">
        {/* Back link */}
        <button
          type="button"
          onClick={() => window.history.back()}
          className="inline-flex items-center gap-2 text-xs font-semibold text-slate-500 hover:text-[#F26F21] transition-colors mb-8 group"
        >
          <ArrowLeft className="w-3.5 h-3.5 group-hover:-translate-x-0.5 transition-transform" />
          BACK TO DASHBOARD
        </button>

        {/* Page header */}
        <div className="fade-in-up text-center mb-6 space-y-5">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full border border-white/[0.06] bg-white/[0.02]">
            <Trophy className="w-3.5 h-3.5 text-[#F26F21]" />
            <span className="text-[11px] font-semibold tracking-wider uppercase text-slate-300">
              OFFICIAL RESULTS
            </span>
          </div>

          <h1
            className="text-3xl sm:text-4xl md:text-5xl font-extrabold text-white tracking-tight"
            style={{ fontFamily: 'Montserrat, sans-serif' }}
          >
            FPT Hackathon{' '}
            <span className="text-transparent bg-clip-text bg-gradient-to-r from-[#F26F21] to-orange-400">
              2026
            </span>{' '}
            Results
          </h1>

          <p className="text-slate-400 text-sm md:text-base leading-relaxed max-w-2xl mx-auto">
            After 48 hours of intense competition,{' '}
            <span className="text-[#F26F21] font-semibold">{teams.length} teams</span>{' '}
            submitted their final projects. Here are the official results reviewed and
            approved by our panel of expert judges.
          </p>

          {/* Published timestamp */}
          <div className="inline-flex items-center gap-2 text-slate-500 text-[11px] font-medium tracking-wide">
            <Calendar className="w-3.5 h-3.5" />
            <span>
              Published on{' '}
              {new Date(announcementTime).toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
              })}
            </span>
          </div>
        </div>

        {/* Divider */}
        <div className="h-px bg-gradient-to-r from-transparent via-white/[0.06] to-transparent my-4" />

        {/* Podium */}
        <div className="fade-in-up" style={{ animationDelay: '0.1s' }}>
          <PodiumSection teams={teams} />
        </div>

        {/* Statistics */}
        <div className="fade-in-up" style={{ animationDelay: '0.2s' }}>
          <StatisticsCards stats={eventStats} />
        </div>

        {/* Divider */}
        <div className="h-px bg-gradient-to-r from-transparent via-white/[0.06] to-transparent my-2" />

        {/* Ranking Table */}
        <div className="fade-in-up" style={{ animationDelay: '0.3s' }}>
          <RankingTable teams={teams} currentUserTeamId={currentUserTeamId} />
        </div>

        {/* Footer */}
        <div className="text-center py-12 space-y-3">
          <div className="h-px bg-gradient-to-r from-transparent via-white/[0.06] to-transparent mb-8" />
          <p className="text-slate-500 text-xs font-medium tracking-wide">
            Results are final and have been verified by the official judging committee.
          </p>
          <p className="text-slate-600 text-[11px]">
            FPT Hackathon 2026 &mdash; Code the Future, Build the World
          </p>
        </div>
      </div>
    </div>
  );
}
