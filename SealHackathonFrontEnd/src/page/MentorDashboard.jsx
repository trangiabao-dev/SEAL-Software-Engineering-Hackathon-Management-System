import { useState } from "react";
import { MentorOverview } from "../components/Mentor/dashboard/MentorOverview";
import { MentorHeader } from "../components/Mentor/layout/MentorHeader";
import { MentorPageTitle } from "../components/Mentor/layout/MentorPageTitle";
import { MentorSidebar } from "../components/Mentor/layout/MentorSidebar";
import { MentorTeams } from "../components/Mentor/teams/MentorTeams";

const viewTitles = {
  dashboard: {
    title: "Mentor Dashboard",
    sub: "Monitor assigned tracks, teams, topics, and read-only submissions",
  },
  teams: {
    title: "Assigned Teams",
    sub: "View team members, submissions, status, and assigned topics",
  },
};

export default function MentorDashboard() {
  const [activeNav, setActiveNav] = useState("dashboard");
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const title = viewTitles[activeNav] || viewTitles.dashboard;

  const views = {
    dashboard: <MentorOverview onViewTeams={() => setActiveNav("teams")} />,
    teams: <MentorTeams />,
  };

  return (
    <div
      className="flex min-h-screen overflow-x-hidden"
      style={{ background: "#F9FAFB" }}
    >
      {sidebarOpen && (
        <button
          className="fixed inset-0 z-20 bg-black/40 md:hidden"
          onClick={() => setSidebarOpen(false)}
          aria-label="Close sidebar overlay"
        />
      )}
      <MentorSidebar
        active={activeNav}
        onNav={setActiveNav}
        isOpen={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
      />

      <div className="flex min-w-0 flex-1 flex-col">
        <MentorHeader onMenuClick={() => setSidebarOpen(true)} />
        <MentorPageTitle title={title.title} sub={title.sub} />
        <main className="flex-1 px-4 py-6 sm:px-8">
          {views[activeNav] || views.dashboard}
        </main>
        <footer
          className="border-t bg-white px-4 py-4 text-center text-xs text-slate-500 sm:px-8"
          style={{ borderColor: "#E5E7EB" }}
        >
          SEAL Hackathon Mentor Console • Read-only supervision workspace
        </footer>
      </div>
    </div>
  );
}
