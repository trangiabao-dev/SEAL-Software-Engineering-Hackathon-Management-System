import { useEffect, useState } from "react";
import { judgeIcons } from "../shared/judgeIcons";

const DEADLINE = new Date("2026-05-25T18:00:00+07:00");

function pad(n) {
  return String(n).padStart(2, "0");
}

function useCountdown(target) {
  const [remaining, setRemaining] = useState(0);

  useEffect(() => {
    const tick = () => setRemaining(Math.max(0, target.getTime() - Date.now()));
    tick();
    const timer = setInterval(tick, 1000);
    return () => clearInterval(timer);
  }, [target]);

  const totalSecs = Math.floor(remaining / 1000);
  return {
    hours: Math.floor(totalSecs / 3600),
    minutes: Math.floor((totalSecs % 3600) / 60),
    seconds: totalSecs % 60,
  };
}

export function JudgeHeader({ onMenuClick }) {
  const { Bell, Clock, Menu } = judgeIcons;
  const { hours, minutes, seconds } = useCountdown(DEADLINE);
  const [hasNotification, setHasNotification] = useState(true);

  return (
    <header className="flex items-center justify-between gap-4 border-b px-4 py-4 sm:px-8" style={{ background: "#FFFFFF", borderColor: "#E5E7EB" }}>
      <button className="block text-gray-600 md:hidden" onClick={onMenuClick} aria-label="Open sidebar">
        <Menu className="h-6 w-6" />
      </button>

      <div className="min-w-0 flex-1">
        <p className="text-xs font-semibold uppercase tracking-widest" style={{ color: "#6B7280" }}>Welcome back</p>
        <h1 className="truncate text-lg font-bold tracking-tight sm:text-xl" style={{ fontFamily: "'Montserrat', 'Inter', sans-serif", color: "#111827" }}>
          Judge Console <span className="font-semibold" style={{ color: "#F26F21" }}>/ SEAL Hackathon</span>
        </h1>
      </div>

      <div className="flex items-center gap-3">
        <div className="hidden items-center gap-3 rounded-xl px-4 py-2.5 sm:flex" style={{ background: "#FFF6F0", border: "1px solid #FFD0B5" }}>
          <Clock className="h-4 w-4" style={{ color: "#F26F21" }} />
          <div className="flex items-center gap-1 font-mono text-sm font-bold" style={{ color: "#111827" }}>
            <span>{pad(hours)}</span><span style={{ color: "#F26F21" }}>:</span><span>{pad(minutes)}</span><span style={{ color: "#F26F21" }}>:</span><span>{pad(seconds)}</span>
          </div>
          <span className="text-[10px] font-semibold uppercase tracking-widest" style={{ color: "#F26F21" }}>Scoring closes</span>
        </div>

        <button
          className="relative flex h-10 w-10 items-center justify-center rounded-xl transition-all"
          style={{ background: "#F9FAFB", border: "1px solid #E5E7EB" }}
          onClick={() => setHasNotification(false)}
          aria-label="Judge notifications"
        >
          <Bell className="h-4 w-4 text-slate-500" />
          {hasNotification && <span className="absolute right-2 top-2 h-2 w-2 rounded-full" style={{ background: "#F26F21" }} />}
        </button>
      </div>
    </header>
  );
}
