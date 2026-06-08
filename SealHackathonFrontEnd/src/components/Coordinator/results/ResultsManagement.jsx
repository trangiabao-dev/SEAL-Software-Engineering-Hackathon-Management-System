import { useState } from "react";
import { results } from "../coordinatorMockData";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorTable,
  ModalShell,
  icons,
} from "../CoordinatorUI";

export function ResultsManagement() {
  const [finalized, setFinalized] = useState(false);
  const [confirm, setConfirm] = useState(false);
  const columns = [
    { key: "rank", label: "Rank" },
    { key: "team", label: "Team" },
    { key: "track", label: "Track" },
    { key: "finalScore", label: "Final" },
    { key: "breakdown", label: "Score summary" },
    { key: "status", label: "Status" },
  ];
  return (
    <div className="space-y-6">
      <div className="grid gap-4 md:grid-cols-3">
        {results.slice(0, 3).map((row) => (
          <div
            key={row.id}
            className="rounded-2xl border bg-white p-5"
            style={{ borderColor: row.rank === 1 ? "#FFD0B5" : "#E5E7EB" }}
          >
            <div className="mb-4 flex items-center justify-between">
              <div
                className="flex h-11 w-11 items-center justify-center rounded-xl text-lg font-black text-white"
                style={{ background: row.rank === 1 ? "#F26F21" : "#64748B" }}
              >
                #{row.rank}
              </div>
              <CoordinatorBadge
                tone={row.status === "Advancing" ? "success" : "neutral"}
              >
                {row.status}
              </CoordinatorBadge>
            </div>
            <h3 className="font-bold text-slate-900">{row.team}</h3>
            <p className="text-sm text-slate-500">{row.track}</p>
            <p className="mt-4 text-3xl font-bold text-slate-900">
              {row.finalScore}
            </p>
          </div>
        ))}
      </div>
      <CoordinatorPanel
        title="Ranking table"
        subtitle={
          finalized
            ? "Results are locked after finalization"
            : "Review rankings before finalization"
        }
        icon={finalized ? icons.Lock : icons.Trophy}
        actions={
          <CoordinatorActionButton
            variant="primary"
            icon={finalized ? icons.Lock : icons.CheckCircle2}
            onClick={() => setConfirm(true)}
          >
            {finalized ? "Locked" : "Finalize Rankings"}
          </CoordinatorActionButton>
        }
      >
        <CoordinatorTable
          columns={columns}
          rows={results}
          renderCell={(row, key) => {
            if (key === "rank")
              return (
                <span className="font-black text-slate-900">#{row.rank}</span>
              );
            if (key === "breakdown")
              return (
                <span className="text-xs text-slate-500">
                  Innovation {row.innovation} • Technical {row.technical} •
                  Impact {row.impact}
                </span>
              );
            if (key === "status")
              return (
                <CoordinatorBadge
                  tone={row.status === "Advancing" ? "success" : "warning"}
                >
                  {row.status}
                </CoordinatorBadge>
              );
            return row[key];
          }}
        />
      </CoordinatorPanel>
      {confirm && (
        <ModalShell
          title="Finalize rankings?"
          onClose={() => setConfirm(false)}
          actions={
            <>
              <CoordinatorActionButton onClick={() => setConfirm(false)}>
                Cancel
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                onClick={() => {
                  setFinalized(true);
                  setConfirm(false);
                }}
              >
                Confirm Finalization
              </CoordinatorActionButton>
            </>
          }
        >
          <p className="text-sm text-slate-600">
            After finalization, rankings enter a locked state and should only be
            changed through an audited administrative action.
          </p>
        </ModalShell>
      )}
    </div>
  );
}
