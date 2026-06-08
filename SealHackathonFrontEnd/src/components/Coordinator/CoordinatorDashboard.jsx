import { useState } from "react";
import { CoordinatorHeader } from "./CoordinatorHeader";
import { CoordinatorPageTitle } from "./CoordinatorPageTitle";
import { CoordinatorSidebar } from "./CoordinatorSidebar";
import { DashboardOverview } from "./dashboard/DashboardOverview";
import { CompetitionSetup } from "./CompetitionSetup";
import { CriteriaManagement } from "./criteria/CriteriaManagement";
import { TopicsManagement } from "./topics/TopicsManagement";
import { UsersManagement } from "./users/UsersManagement";
import { TeamsManagement } from "./teams/TeamsManagement";
import { MentorsManagement } from "./mentors/MentorsManagement";
import { JudgesManagement } from "./judges/JudgesManagement";
import { ResultsManagement } from "./results/ResultsManagement";
import { ExportManagement } from "./export/ExportManagement";
import { AuditLog } from "./audit/AuditLog";

const viewTitles = {
  dashboard: {
    title: "Coordinator Dashboard",
    sub: "Monitor hackathon operations, registrations, judging, and results",
  },
  "competition-setup": {
    title: "Competition Setup",
    sub: "Manage events, tracks, and rounds in a unified tree view",
  },
  criteria: {
    title: "Criteria Management",
    sub: "Define rubrics, scoring weights, and judging requirements",
  },
  topics: {
    title: "Topic Management",
    sub: "Manage challenge topics, attachments, and round associations",
  },
  users: {
    title: "User Approval Management",
    sub: "Review pending leaders and manage approval decisions",
  },
  teams: {
    title: "Team Approval Management",
    sub: "Approve teams, review details, and manage disqualification",
  },
  mentors: {
    title: "Mentor Assignment",
    sub: "Assign mentors to tracks and teams while monitoring workload",
  },
  judges: {
    title: "Judge Assignment",
    sub: "Assign judges to rounds and manage guest judge participation",
  },
  results: {
    title: "Result Finalization",
    sub: "Review rankings, advancing teams, and final score locks",
  },
  export: {
    title: "Export Management",
    sub: "Export official reports, spreadsheets, anonymous data, and history",
  },
  audit: {
    title: "Audit Log",
    sub: "Review administrative actions, entity changes, and detailed history",
  },
};

const views = {
  dashboard: DashboardOverview,
  "competition-setup": CompetitionSetup,
  criteria: CriteriaManagement,
  topics: TopicsManagement,
  users: UsersManagement,
  teams: TeamsManagement,
  mentors: MentorsManagement,
  judges: JudgesManagement,
  results: ResultsManagement,
  export: ExportManagement,
  audit: AuditLog,
};

export default function CoordinatorDashboard() {
  const [activeNav, setActiveNav] = useState("dashboard");
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const ActiveView = views[activeNav] || DashboardOverview;
  const title = viewTitles[activeNav] || viewTitles.dashboard;

  return (
    <div
      className="flex min-h-screen overflow-x-hidden"
      style={{ background: "#F9FAFB" }}
    >
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-20 bg-slate-950/40 md:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}
      <CoordinatorSidebar
        active={activeNav}
        onNav={setActiveNav}
        isOpen={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
      />

      <div className="flex min-w-0 flex-1 flex-col">
        <CoordinatorHeader onMenuClick={() => setSidebarOpen(true)} />
        <CoordinatorPageTitle title={title.title} sub={title.sub} />
        <main className="flex-1 px-4 py-6 sm:px-8">
          <div className="animate-fade-in" key={activeNav}>
            <ActiveView />
          </div>
        </main>
        <footer
          className="border-t px-4 py-4 text-center text-xs text-slate-400 sm:px-8"
          style={{ borderColor: "#E5E7EB" }}
        >
          SEAL Hackathon Management System • Coordinator Console
        </footer>
      </div>
    </div>
  );
}

export { CoordinatorDashboard };
