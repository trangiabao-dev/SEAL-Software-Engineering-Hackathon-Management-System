import { Bell, Clock, ChevronDown, Menu } from "lucide-react";
import { useEffect, useState } from "react";
import { useSelector } from "react-redux";

// Deadline: 48 hours from a fixed reference (demo)
const DEADLINE = new Date(Date.now() + 48 * 60 * 60 * 1000);

function pad(n) {
  return String(n).padStart(2, "0");
}

function useCountdown(target) {
  const [remaining, setRemaining] = useState(0);

  useEffect(() => {
    const tick = () => {
      const diff = Math.max(0, target.getTime() - Date.now());
      setRemaining(diff);
    };
    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, [target]);

  const totalSecs = Math.floor(remaining / 1000);
  const hours = Math.floor(totalSecs / 3600);
  const minutes = Math.floor((totalSecs % 3600) / 60);
  const seconds = totalSecs % 60;
  return { hours, minutes, seconds };
}

export function Header({ onMenuClick }) {
  const { hours, minutes, seconds } = useCountdown(DEADLINE);
  const [hasNotif, setHasNotif] = useState(true);
  const { user } = useSelector((s) => s.auth); // lấy user thật từ Redux
  return (
    <header
      className="flex items-center justify-between px-8 py-4 border-b"
      style={{ background: "#FFFFFF", borderColor: "#E5E7EB" }}
    >
      {/* Mobile menu button */}
      <button
        className="block md:hidden text-gray-600"
        onClick={onMenuClick}
        aria-label="Open sidebar"
      >
        <Menu className="w-6 h-6" />
      </button>
      {/* Greeting */}
      <div className="flex-1 min-w-0 ml-4">
        <p
          className="text-xs font-semibold tracking-widest uppercase"
          style={{ color: "#6B7280" }}
        >
          Welcome back
        </p>
        <h1
          className="text-sm sm:text-xl font-bold tracking-tight truncate"
          style={{
            fontFamily: "'Montserrat', 'Inter', sans-serif",
            color: "#111827",
          }}
        >
          {user.username}{" "}
          <span className="font-semibold text-xs sm:text-sm" style={{ color: "#F26F21" }}>
            / Team Alpha
          </span>
        </h1>
      </div>

      {/* Right cluster */}
      <div className="flex items-center gap-4">
        {/* Countdown */}
        <div
          className="hidden sm:flex items-center gap-3 px-4 py-2.5 rounded-xl"
          style={{ background: "#FFF6F0", border: "1px solid #FFD0B5" }}
        >
          <Clock className="w-4 h-4" style={{ color: "#F26F21" }} />
          <div
            className="flex items-center gap-1 font-mono font-bold text-sm"
            style={{ color: "#111827" }}
          >
            <span className="tabular-nums">{pad(hours)}</span>
            <span style={{ color: "#F26F21" }}>:</span>
            <span className="tabular-nums">{pad(minutes)}</span>
            <span style={{ color: "#F26F21" }}>:</span>
            <span className="tabular-nums">{pad(seconds)}</span>
          </div>
          <span
            className="text-[10px] font-semibold uppercase tracking-widest"
            style={{ color: "#F26F21" }}
          >
            Remaining
          </span>
        </div>

        {/* Bell */}
        <button
          className="relative w-10 h-10 rounded-xl flex items-center justify-center bg-[#F9FAFB] border border-[#E5E7EB] hover:bg-[#FFF6F0] hover:border-[#FFD0B5] transition-all duration-200 active:scale-[0.95]"
          onClick={() => setHasNotif(false)}
        >
          <Bell className="w-4 h-4" style={{ color: "#6B7280" }} />
          {hasNotif && (
            <span
              className="absolute top-2 right-2 w-2 h-2 rounded-full"
              style={{ background: "#F26F21" }}
            />
          )}
        </button>
      </div>
    </header>
  );
}
