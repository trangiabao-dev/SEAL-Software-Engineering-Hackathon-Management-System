import { useState } from "react";
import { judgeCriteria } from "../judgeMockData";
import { JudgeActionButton } from "../shared/JudgeActionButton";
import { JudgeBadge } from "../shared/JudgeBadge";
import { JudgePanel } from "../shared/JudgePanel";
import { JudgeTable } from "../shared/JudgeTable";
import { judgeIcons } from "../shared/judgeIcons";
import { LockedScoreView } from "./LockedScoreView";

export function HistoryTable({ records }) {
  const [selected, setSelected] = useState(null);
  const columns = [
    { key: "teamName", label: "TeamName" },
    { key: "university", label: "University" },
    { key: "roundName", label: "Round" },
    { key: "scoredAt", label: "SubmittedAt" },
    { key: "totalScore", label: "TotalScore" },
    { key: "status", label: "Status" },
    { key: "details", label: "Details" },
  ];

  return (
    <>
      <JudgeTable
        columns={columns}
        rows={records}
        emptyMessage="No scoring history yet"
        renderCell={(record, key) => {
          if (key === "teamName") return <p className="font-bold text-slate-900">{record.teamName}</p>;
          if (key === "status") return <JudgeBadge tone={record.status === "Locked" ? "neutral" : "success"}>{record.status}</JudgeBadge>;
          if (key === "details") return <JudgeActionButton onClick={() => setSelected(record)} icon={judgeIcons.Eye}>View details</JudgeActionButton>;
          return record[key] || "—";
        }}
      />
      {selected && <LockedScoreView record={selected} criteria={judgeCriteria} onClose={() => setSelected(null)} />}
    </>
  );
}

export function HistoryPanel({ records }) {
  return (
    <JudgePanel title="Previously scored submissions" subtitle="Submitted score summaries are read-only" icon={judgeIcons.FileText}>
      <HistoryTable records={records} />
    </JudgePanel>
  );
}
