import { icons } from "./CoordinatorUI";
import { Layers } from "lucide-react";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import { Code2, CloudUpload, Users, LogOut, Trophy, X } from "lucide-react";
import { logoutSuccess } from "../../store/authSlice";
import authService from "../../services/authService";

const navItems = [
  {
    id: "dashboard",
    label: "Dashboard",
    labelVi: "Tổng quan",
    icon: icons.LayoutDashboard,
  },
  {
    id: "competition-setup",
    label: "Competition Setup",
    labelVi: "Cài đặt thi đấu",
    icon: Layers,
  },
  {
    id: "criteria",
    label: "Criteria",
    labelVi: "Tiêu chí",
    icon: icons.SlidersHorizontal,
  },
  { id: "topics", label: "Topics", labelVi: "Đề tài", icon: icons.Lightbulb },
  { id: "users", label: "Users", labelVi: "Người dùng", icon: icons.Users },
  { id: "teams", label: "Teams", labelVi: "Đội thi", icon: icons.UserRoundCog },
  { id: "mentors", label: "Mentors", labelVi: "Cố vấn", icon: icons.Handshake },
  { id: "judges", label: "Judges", labelVi: "Giám khảo", icon: icons.Scale },
  { id: "results", label: "Results", labelVi: "Kết quả", icon: icons.Trophy },
  {
    id: "export",
    label: "Export",
    labelVi: "Xuất dữ liệu",
    icon: icons.Download,
  },
  { id: "audit", label: "Audit", labelVi: "Nhật ký", icon: icons.ShieldCheck },
];

export function CoordinatorSidebar({ active, onNav, isOpen, onClose }) {
  const { Trophy, X } = icons;
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
          <div
            className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl"
            style={{
              background: "rgba(255,255,255,0.2)",
              border: "1px solid rgba(255,255,255,0.3)",
            }}
          >
            <Trophy className="h-5 w-5 text-white" />
          </div>
          <div>
            <p className="text-xs font-bold uppercase tracking-widest text-white">
              SEAL Hackathon
            </p>
            <p className="text-xs" style={{ color: "rgba(255,255,255,0.65)" }}>
              Coordinator Console
            </p>
          </div>
        </div>
        <div
          className="mt-4 rounded-lg px-3 py-2"
          style={{
            background: "rgba(255,255,255,0.12)",
            border: "1px solid rgba(255,255,255,0.18)",
          }}
        >
          <p
            className="text-[10px] font-semibold uppercase tracking-widest"
            style={{ color: "rgba(255,255,255,0.65)" }}
          >
            Active event
          </p>
          <p className="truncate text-sm font-bold text-white">
            FPT Hackathon 2026
          </p>
        </div>
      </div>

      <nav className="flex-1 space-y-1 overflow-y-auto px-3 py-4">
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
              className="group flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left transition-all hover:bg-white/10 hover:translate-x-1 active:scale-[0.98]"
              style={{
                background: isActive ? "rgba(255,255,255,0.18)" : "transparent",
                color: "white",
                border: isActive
                  ? "1px solid rgba(255,255,255,0.22)"
                  : "1px solid transparent",
              }}
            >
              <Icon className="h-4 w-4 flex-shrink-0" />
              <span className="min-w-0 flex-1">
                <span className="block truncate text-sm font-semibold">
                  {item.label}
                </span>
                <span
                  className="block truncate text-[10px]"
                  style={{ color: "rgba(255,255,255,0.6)" }}
                >
                  {item.labelVi}
                </span>
              </span>
            </button>
          );
        })}
      </nav>

      <div
        className="px-4 py-4"
        style={{ borderTop: "1px solid rgba(255,255,255,0.15)" }}
      >
        <div
          className="rounded-xl px-3 py-3"
          style={{ background: "rgba(255,255,255,0.1)" }}
        >
          <p className="text-xs font-bold text-white">System Role</p>
          <p className="text-xs" style={{ color: "rgba(255,255,255,0.65)" }}>
            Highest-level coordinator access
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
