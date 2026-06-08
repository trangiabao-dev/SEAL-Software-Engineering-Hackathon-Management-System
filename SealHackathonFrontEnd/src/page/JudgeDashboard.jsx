import { useState } from "react";
import {
  judgeRounds,
  judgeSubmissions,
} from "../components/Judge/judgeMockData";
import { JudgeOverview } from "../components/Judge/dashboard/JudgeOverview";
import { ScoringHistory } from "../components/Judge/history/ScoringHistory";
import { JudgeHeader } from "../components/Judge/layout/JudgeHeader";
import { JudgePageTitle } from "../components/Judge/layout/JudgePageTitle";
import { JudgeSidebar } from "../components/Judge/layout/JudgeSidebar";
import { JudgeRounds } from "../components/Judge/rounds/JudgeRounds";
import { SubmissionScoringList } from "../components/Judge/scoring/SubmissionScoringList";

const viewTitles = {
  dashboard: {
    title: "Judge Dashboard",
    sub: "Assigned rounds, scoring progress, countdowns, and lock status",
  },
  rounds: {
    title: "Assigned Rounds",
    sub: "Review scoring windows and ranking lock state",
  },
  scoring: {
    title: "Submission Scoring",
    sub: "Score assigned submissions without team-support relationship visibility",
  },
  history: {
    title: "Scoring History",
    sub: "Read-only record of submitted scores and comments",
  },
};

function isSubmissionLocked(submission) {
  const round = judgeRounds.find((item) => item.id === submission.roundId);
  return (
    submission.ranking?.calculatedAt != null ||
    round?.ranking?.calculatedAt != null
  );
}

export default function JudgeDashboard() {
  const [activeNav, setActiveNav] = useState("dashboard");
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [submissions, setSubmissions] = useState(judgeSubmissions);
  const [savingId, setSavingId] = useState(null);
  const [submittingId, setSubmittingId] = useState(null);
  const title = viewTitles[activeNav] || viewTitles.dashboard;

  const handleSaveDraft = (submissionId, scores, comments, totalScore) => {
    const target = submissions.find((item) => item.id === submissionId);
    if (!target || isSubmissionLocked(target)) return;
    setSavingId(submissionId);
    setSubmissions((current) =>
      current.map((item) =>
        item.id === submissionId
          ? { ...item, scores, comments, totalScore, status: "Draft" }
          : item,
      ),
    );
    setTimeout(() => setSavingId(null), 350);
  };

  const handleSubmitScores = (submissionId, scores, comments, totalScore) => {
    const target = submissions.find((item) => item.id === submissionId);
    if (!target || isSubmissionLocked(target)) return;
    setSubmittingId(submissionId);
    setSubmissions((current) =>
      current.map((item) =>
        item.id === submissionId
          ? {
              ...item,
              scores,
              comments,
              totalScore,
              status: "Scored",
              scoredAt: new Date().toLocaleString(),
            }
          : item,
      ),
    );
    setTimeout(() => setSubmittingId(null), 350);
  };

  const views = {
    dashboard: (
      <JudgeOverview
        submissions={submissions}
        onOpenScoring={() => setActiveNav("scoring")}
      />
    ),
    rounds: <JudgeRounds onOpenScoring={() => setActiveNav("scoring")} />,
    scoring: (
      <SubmissionScoringList
        submissions={submissions}
        savingId={savingId}
        submittingId={submittingId}
        onSaveDraft={handleSaveDraft}
        onSubmitScores={handleSubmitScores}
      />
    ),
    history: <ScoringHistory submissions={submissions} />,
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
      <JudgeSidebar
        active={activeNav}
        onNav={setActiveNav}
        isOpen={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
      />

      <div className="flex min-w-0 flex-1 flex-col">
        <JudgeHeader onMenuClick={() => setSidebarOpen(true)} />
        <JudgePageTitle title={title.title} sub={title.sub} />
        <main className="flex-1 px-4 py-6 sm:px-8">
          {views[activeNav] || views.dashboard}
        </main>
        <footer
          className="border-t bg-white px-4 py-4 text-center text-xs text-slate-500 sm:px-8"
          style={{ borderColor: "#E5E7EB" }}
        >
          SEAL Hackathon Judge Console • RBL-safe scoring workspace
        </footer>
      </div>
    </div>
  );
}
