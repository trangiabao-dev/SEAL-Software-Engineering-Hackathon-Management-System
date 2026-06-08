import { Code2, CloudUpload, Users, LogOut, Trophy, X } from "lucide-react";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import { logoutSuccess } from "../../store/authSlice";
import authService from "../../services/authService";

const navItems = [
  {
    id: "challenges",
    label: "View Challenges",
    labelVi: "Xem đề",
    icon: Code2,
  },
  {
    id: "submit",
    label: "Submit Project",
    labelVi: "Submit",
    icon: CloudUpload,
  },
  {
    id: "team",
    label: "Team Information",
    labelVi: "Thông tin nhóm",
    icon: Users,
  },
];

export function Sidebar({ active, onNav, isOpen, onClose }) {
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

  // Sidebar classes: hidden on mobile unless isOpen, always visible on md+.
  const sidebarClasses = `fixed inset-y-0 left-0 z-30 w-64 flex flex-col bg-gradient-to-b from-[#F26F21] to-[#7F1D1D] transform transition-transform duration-300 md:translate-x-0 md:static md:inset-auto ${isOpen ? "translate-x-0" : "-translate-x-full"} md:block`;

  return (
    <aside
      className={sidebarClasses}
      style={{ borderRight: "1px solid rgba(255,255,255,0.1)" }}
    >
      {/* Mobile close button */}
      <button
        className="absolute top-4 right-4 md:hidden text-white"
        onClick={onClose}
        aria-label="Close sidebar"
      >
        <X className="w-5 h-5" />
      </button>

      {/* Logo & Team */}
      <div
        className="px-6 py-7 border-b"
        style={{ borderColor: "rgba(255,255,255,0.15)" }}
      >
        <div className="flex items-center gap-3 mb-1">
          <div
            className="w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0"
            style={{
              background: "rgba(255,255,255,0.2)",
              border: "1px solid rgba(255,255,255,0.3)",
            }}
          >
            <Trophy className="w-5 h-5 text-white" />
          </div>
          <div>
            <p className="text-xs font-bold tracking-widest uppercase text-white">
              FPT Hackathon
            </p>
            <p className="text-xs" style={{ color: "rgba(255,255,255,0.65)" }}>
              2026 Edition
            </p>
          </div>
        </div>
        <div
          className="mt-4 px-3 py-2 rounded-lg"
          style={{
            background: "rgba(255,255,255,0.12)",
            border: "1px solid rgba(255,255,255,0.2)",
          }}
        >
          <p
            className="text-[11px] uppercase tracking-wider font-semibold"
            style={{ color: "rgba(255,255,255,0.7)" }}
          >
            Team
          </p>
          <p className="text-white font-bold text-sm tracking-tight">
            Team Alpha
          </p>
        </div>
      </div>

      {/* Nav */}
      <nav className="flex-1 px-4 py-6 space-y-1">
        <p
          className="text-[10px] font-semibold tracking-widest uppercase px-3 mb-3"
          style={{ color: "rgba(255,255,255,0.5)" }}
        >
          Navigation
        </p>
        {navItems.map(({ id, label, labelVi, icon: Icon }) => {
          const isActive = active === id;
          return (
            <button
              key={id}
              onClick={() => onNav(id)}
              className={`w-full flex items-center gap-3 px-3 py-3 rounded-xl text-left transition-all duration-200 ${
                isActive
                  ? "bg-white border border-white/60 shadow-[0_4px_16px_rgba(0,0,0,0.15)] text-[#C2410C]"
                  : "bg-transparent border border-transparent hover:bg-white/10 text-white active:scale-[0.98]"
              }`}
            >
              <div
                className={`w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 transition-all duration-200 ${
                  isActive ? "bg-orange-700/10" : "bg-white/15"
                }`}
              >
                <Icon
                  className={`w-4 h-4 transition-colors duration-200 ${
                    isActive ? "text-[#C2410C]" : "text-white"
                  }`}
                />
              </div>
              <div>
                <p
                  className={`text-sm font-semibold transition-colors duration-200 ${
                    isActive ? "text-[#C2410C]" : "text-white"
                  }`}
                >
                  {label}
                </p>
                <p
                  className={`text-[10px] transition-colors duration-200 ${
                    isActive ? "text-orange-700/75" : "text-white/60"
                  }`}
                >
                  {labelVi}
                </p>
              </div>
              {isActive && (
                <div className="ml-auto w-1.5 h-1.5 rounded-full flex-shrink-0 bg-[#C2410C]" />
              )}
            </button>
          );
        })}
      </nav>

      {/* Footer */}
      <div
        className="px-4 pb-6 border-t pt-4 space-y-3"
        style={{ borderColor: "rgba(255,255,255,0.15)" }}
      >
        {/* Avatar */}
        <div
          className="flex items-center gap-3 px-3 py-2 rounded-xl"
          style={{
            background: "rgba(255,255,255,0.12)",
            border: "1px solid rgba(255,255,255,0.2)",
          }}
        >
          <div
            className="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold flex-shrink-0"
            style={{
              background: "rgba(255,255,255,0.25)",
              color: "#FFFFFF",
              border: "1.5px solid rgba(255,255,255,0.5)",
            }}
          >
            N
          </div>
          <div className="min-w-0">
            <p className="text-xs font-semibold text-white truncate">
              {user.username}
            </p>
            <p
              className="text-[10px] truncate"
              style={{ color: "rgba(255,255,255,0.6)" }}
            >
              Leader
            </p>
          </div>
        </div>
        <button
          className="w-full flex items-center gap-2 px-3 py-2.5 rounded-xl text-sm font-semibold border border-white/20 bg-white/10 hover:bg-white/20 text-white transition-all duration-200 active:scale-[0.98]"
          onClick={handleLogout}
        >
          <LogOut className="w-4 h-4" />
          Logout
        </button>
      </div>
    </aside>
  );
}
