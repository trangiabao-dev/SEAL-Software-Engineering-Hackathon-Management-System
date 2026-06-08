import { judgeIcons } from "../shared/judgeIcons";
import { Code2, CloudUpload, Users, LogOut, Trophy, X } from "lucide-react";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import { logoutSuccess } from "../../../store/authSlice";
import authService from "../../../services/authService";

const navItems = [
  {
    id: "dashboard",
    label: "Dashboard",
    labelVi: "Tổng quan",
    icon: judgeIcons.LayoutDashboard,
  },
  {
    id: "rounds",
    label: "Rounds",
    labelVi: "Vòng chấm",
    icon: judgeIcons.CalendarDays,
  },
  {
    id: "scoring",
    label: "Scoring",
    labelVi: "Chấm điểm",
    icon: judgeIcons.Gavel,
  },
  {
    id: "history",
    label: "History",
    labelVi: "Lịch sử",
    icon: judgeIcons.FileText,
  },
];

export function JudgeSidebar({ active, onNav, isOpen, onClose }) {
  const { ShieldCheck, X } = judgeIcons;
  const sidebarClasses = `fixed inset-y-0 left-0 z-30 flex w-64 transform flex-col bg-gradient-to-b from-[#F26F21] to-[#7F1D1D] transition-transform duration-300 md:static md:inset-auto md:translate-x-0 ${isOpen ? "translate-x-0" : "-translate-x-full"}`;

  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { user } = useSelector((s) => s.auth); // lấy user thật từ Redux

  const handleLogout = async () => {
    try {
      await authService.logout();
    } catch (_) {
      // Kể cả lỗi vẫn logout FE bình thường
    } finally {
      dispatch(logoutSuccess());
      navigate("/");
    }
  };

  return (
    <aside
      className={sidebarClasses}
      style={{ borderRight: "1px solid rgba(255,255,255,0.1)" }}
    >
      <button
        className="absolute right-4 top-4 text-white md:hidden"
        onClick={onClose}
        aria-label="Close sidebar"
      >
        <X className="h-5 w-5" />
      </button>

      <div
        className="px-6 py-7"
        style={{ borderBottom: "1px solid rgba(255,255,255,0.15)" }}
      >
        <div className="mb-1 flex items-center gap-3">
          <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-white/15 text-white shadow-lg">
            <ShieldCheck className="h-6 w-6" />
          </div>
          <div>
            <p
              className="text-lg font-bold leading-tight text-white"
              style={{ fontFamily: "'Montserrat', 'Inter', sans-serif" }}
            >
              SEAL
            </p>
            <p className="text-xs font-semibold uppercase tracking-widest text-white/70">
              Judge
            </p>
          </div>
        </div>
        <p className="mt-4 text-sm leading-5 text-white/75">
          Focused scoring workspace for assigned rounds and submissions.
        </p>
      </div>

      <nav className="flex-1 space-y-1 overflow-y-auto px-3 py-5">
        {navItems.map((item) => {
          const Icon = item.icon;
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              onClick={() => {
                onNav(item.id);
                onClose?.();
              }}
              className="group flex w-full items-center gap-3 rounded-xl px-3 py-3 text-left transition-all"
              style={{
                background: isActive ? "rgba(255,255,255,0.18)" : "transparent",
                color: "white",
              }}
            >
              <Icon
                className={`h-5 w-5 ${isActive ? "text-white" : "text-white/75 group-hover:text-white"}`}
              />
              <span className="min-w-0 flex-1">
                <span className="block text-sm font-bold">{item.label}</span>
                <span className="block text-xs text-white/60">
                  {item.labelVi}
                </span>
              </span>
            </button>
          );
        })}
      </nav>

      <div
        className="p-4"
        style={{ borderTop: "1px solid rgba(255,255,255,0.15)" }}
      >
        <div className="rounded-2xl bg-white/10 p-4 text-white">
          <p className="text-sm font-bold">RBL Blind Mode</p>
          <p className="mt-1 text-xs leading-5 text-white/70">
            Team support identities and relationships are unavailable in this
            workspace.
          </p>
        </div>
        <button
          className="w-full flex items-center gap-2 px-3 py-2.5 rounded-xl text-sm font-semibold border border-white/20 bg-white/10 hover:bg-white/20 text-white transition-all duration-200 active:scale-[0.98] mt-3"
          onClick={handleLogout}
        >
          <LogOut className="w-4 h-4" />
          Logout
        </button>
      </div>
    </aside>
  );
}
