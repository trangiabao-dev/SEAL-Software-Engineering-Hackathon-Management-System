import { useMemo, useState } from "react";
import { currentTime, judgeRounds } from "../judgeMockData";
import { EmptyState } from "../shared/EmptyState";
import { JudgeActionButton } from "../shared/JudgeActionButton";
import { JudgeBadge } from "../shared/JudgeBadge";
import { JudgePanel } from "../shared/JudgePanel";
import { JudgeTable } from "../shared/JudgeTable";
import { judgeIcons } from "../shared/judgeIcons";
import { LockNotice } from "./LockNotice";
import { ScoringForm } from "./form/ScoringForm";

function statusTone(status) {
  if (status === "Scored" || status === "Locked") return "success";
  if (status === "Draft") return "warning";
  return "neutral";
}

function getRound(submission) {
  return judgeRounds.find((round) => round.id === submission.roundId);
}

function isTimeLocked(submission) {
  const round = getRound(submission);
  return round ? currentTime < new Date(round.endTime) : false;
}

function isRankingLocked(submission) {
  const round = getRound(submission);
  return submission.ranking?.calculatedAt != null || round?.ranking?.calculatedAt != null;
}

export function SubmissionScoringList({ submissions, savingId, submittingId, onSaveDraft, onSubmitScores }) {
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("All");
  const [selected, setSelected] = useState(null);
  const { ExternalLink, FileText, Filter, Gavel, Search } = judgeIcons;

  const filtered = useMemo(() => {
    const keyword = search.trim().toLowerCase();
    return submissions.filter((item) => {
      const matchesKeyword = !keyword || [item.teamName, item.university].some((value) => value.toLowerCase().includes(keyword));
      const matchesStatus = status === "All" || item.status === status;
      return matchesKeyword && matchesStatus;
    });
  }, [search, status, submissions]);

  const columns = [
    { key: "teamName", label: "TeamName" },
    { key: "university", label: "University" },
    { key: "demoUrl", label: "DemoUrl" },
    { key: "reportUrl", label: "ReportUrl" },
    { key: "status", label: "Submission Status" },
    { key: "action", label: "Action" },
  ];

  return (
    <div className="space-y-6">
      <JudgePanel title="Submission scoring list" subtitle="Only judge-visible submission fields are displayed" icon={Gavel}>
        <div className="mb-5 grid gap-3 md:grid-cols-[1fr_auto]">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search TeamName or University"
              className="w-full rounded-xl border border-slate-200 bg-white py-2.5 pl-10 pr-4 text-sm outline-none transition focus:border-orange-300 focus:ring-2 focus:ring-orange-100"
            />
          </div>
          <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2.5">
            <Filter className="h-4 w-4 text-slate-400" />
            <select value={status} onChange={(event) => setStatus(event.target.value)} className="bg-transparent text-sm font-semibold text-slate-700 outline-none">
              {["All", "Not scored", "Draft", "Scored", "Locked", "Locked until scoring time"].map((item) => <option key={item} value={item}>{item}</option>)}
            </select>
          </div>
        </div>

        {filtered.length > 0 ? (
          <JudgeTable
            columns={columns}
            rows={filtered}
            renderCell={(submission, key) => {
              const timeLocked = isTimeLocked(submission);
              const rankingLocked = isRankingLocked(submission);
              if (key === "teamName") return <p className="font-bold text-slate-900">{submission.teamName}</p>;
              if (key === "demoUrl" || key === "reportUrl") return <a href={submission[key]} target="_blank" rel="noreferrer" className="inline-flex items-center gap-1 font-bold text-orange-700">Open <ExternalLink className="h-4 w-4" /></a>;
              if (key === "status") return <JudgeBadge tone={statusTone(submission.status)}>{submission.status}</JudgeBadge>;
              if (key === "action") {
                return (
                  <JudgeActionButton disabled={timeLocked} onClick={() => setSelected(submission)} icon={submission.status === "Scored" || rankingLocked ? FileText : Gavel}>
                    {submission.status === "Scored" || rankingLocked ? "View" : "Score"}
                  </JudgeActionButton>
                );
              }
              return submission[key];
            }}
          />
        ) : (
          <EmptyState icon={Gavel} title="No submissions found" description="Try adjusting search or status filters." />
        )}
      </JudgePanel>

      <JudgePanel title="Round access rule" subtitle="Scoring is unavailable before the configured round EndTime" icon={judgeIcons.Lock}>
        <LockNotice type="time" />
      </JudgePanel>

      {selected && (
        <ScoringForm
          submission={submissions.find((item) => item.id === selected.id) || selected}
          isLocked={isRankingLocked(selected)}
          isTimeLocked={isTimeLocked(selected)}
          saving={savingId === selected.id}
          submitting={submittingId === selected.id}
          onClose={() => setSelected(null)}
          onSaveDraft={onSaveDraft}
          onSubmitScores={(...args) => {
            onSubmitScores(...args);
            setSelected(null);
          }}
        />
      )}
    </div>
  );
}
