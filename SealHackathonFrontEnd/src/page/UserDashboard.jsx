import { useState } from "react";
import { Sidebar } from "../components/UserDashboard/Sidebar";
import { Header } from "../components/UserDashboard/Header";
import { ChallengesView } from "../components/UserDashboard/ChallengesView";
import { SubmitView } from "../components/UserDashboard/SubmitView";
import { TeamView } from "../components/UserDashboard/TeamView";

const viewTitles = {
  challenges: {
    title: "View Challenges",
    sub: "Browse all active and upcoming challenges",
  },
  submit: {
    title: "Submit Project",
    sub: "Upload your project files or link your repository",
  },
  team: {
    title: "Team Information",
    sub: "Manage your team and track progress",
  },
};

export default function UserDashboard() {
  const [activeNav, setActiveNav] = useState("challenges");
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div
      className="flex min-h-screen overflow-x-hidden"
      style={{
        background: "#F9FAFB",
        fontFamily:
          "'Montserrat', 'Inter', ui-sans-serif, system-ui, sans-serif",
      }}
    >
      {/* Background grid texture */}
      <div
        className="fixed inset-0 pointer-events-none"
        aria-hidden="true"
        style={{
          backgroundImage:
            "linear-gradient(rgba(242,111,33,0.02) 1px, transparent 1px), linear-gradient(90deg, rgba(242,111,33,0.02) 1px, transparent 1px)",
          backgroundSize: "48px 48px",
        }}
      />
      {/* Ambient glow */}
      <div
        className="fixed top-0 left-64 right-0 h-96 pointer-events-none"
        aria-hidden="true"
        style={{
          background:
            "radial-gradient(ellipse 60% 40% at 50% 0%, rgba(242,111,33,0.03) 0%, transparent 100%)",
        }}
      />

      {/* Mobile Sidebar Backdrop Overlay */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-20 bg-slate-950/40 backdrop-blur-sm transition-all duration-300 md:hidden animate-fade-in"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Mobile Sidebar Drawer */}
      <Sidebar
        active={activeNav}
        onNav={(id) => {
          setActiveNav(id);
          setSidebarOpen(false);
        }}
        isOpen={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
      />

      {/* Main layout */}
      <div className="flex-1 flex flex-col min-w-0 min-h-screen">
        <Header onMenuClick={() => setSidebarOpen(true)} />

        {/* Page title strip */}
        <div className="px-8 py-5 border-b" style={{ borderColor: "#E5E7EB" }}>
          <div className="flex items-end gap-3">
            <div>
              <h2
                className="text-2xl font-black tracking-tight text-[#111827]"
                style={{ fontFamily: "'Montserrat', sans-serif" }}
              >
                {viewTitles[activeNav].title}
              </h2>
              <p className="text-sm text-slate-500 mt-0.5">
                {viewTitles[activeNav].sub}
              </p>
            </div>
            {/* neon accent line */}
            <div
              className="mb-1 flex-1 h-px"
              style={{
                background:
                  "linear-gradient(90deg, rgba(242,111,33,0.4), transparent)",
              }}
            />
          </div>
        </div>

        {/* Content */}
        <main className="flex-1 px-8 py-7">
          <div key={activeNav} className="animate-fade-in">
            {activeNav === "challenges" && <ChallengesView />}
            {activeNav === "submit" && <SubmitView />}
            {activeNav === "team" && <TeamView />}
          </div>
        </main>


        {/* Footer */}
        <footer
          className="px-8 py-4 border-t flex items-center justify-between text-[11px]"
          style={{ borderColor: "#E5E7EB", color: "#4B5563" }}
        >
          <span>FPT Hackathon 2026 &mdash; All rights reserved.</span>
          <span className="font-semibold" style={{ color: "#F26F21" }}>
            Team Alpha &bull; CH-001
          </span>
        </footer>
      </div>
    </div>
  );
}
