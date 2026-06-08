import { useState } from "react";
import { judges } from "../coordinatorMockData";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorProgressBar,
  CoordinatorTable,
  ModalShell,
  icons,
} from "../CoordinatorUI";

export function JudgesManagement() {
  const [guestModal, setGuestModal] = useState(false);
  const columns = [
    { key: "name", label: "Judge" },
    { key: "type", label: "Type" },
    { key: "rounds", label: "Rounds" },
    { key: "status", label: "Status" },
    { key: "progress", label: "Scoring" },
    { key: "actions", label: "Actions" },
  ];
  return (
    <div className="space-y-6">
      <CoordinatorPanel
        title="Judge assignment table"
        subtitle="Assign judges to rounds and monitor scoring progress"
        icon={icons.Scale}
        actions={
          <CoordinatorActionButton
            variant="primary"
            icon={icons.Plus}
            onClick={() => setGuestModal(true)}
          >
            Create Guest Judge
          </CoordinatorActionButton>
        }
      >
        <CoordinatorTable
          columns={columns}
          rows={judges}
          renderCell={(row, key) => {
            if (key === "rounds") return row.rounds.join(", ");
            if (key === "status")
              return (
                <CoordinatorBadge
                  tone={row.status === "Active" ? "success" : "warning"}
                >
                  {row.status}
                </CoordinatorBadge>
              );
            if (key === "progress")
              return (
                <div className="w-40">
                  <CoordinatorProgressBar
                    value={Math.round(
                      (row.completedScores / row.assignedTeams) * 100,
                    )}
                  />
                </div>
              );
            if (key === "actions")
              return (
                <div className="flex gap-2">
                  <CoordinatorActionButton>
                    Assign Round
                  </CoordinatorActionButton>
                  <CoordinatorActionButton>Reminder</CoordinatorActionButton>
                </div>
              );
            return row[key];
          }}
        />
      </CoordinatorPanel>
      {guestModal && (
        <ModalShell
          title="Guest judge creation"
          onClose={() => setGuestModal(false)}
          actions={
            <>
              <CoordinatorActionButton onClick={() => setGuestModal(false)}>
                Cancel
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                onClick={() => setGuestModal(false)}
              >
                Create Invite
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="grid gap-3">
            <input
              className="rounded-xl border border-slate-200 px-3 py-2.5"
              placeholder="Judge name"
            />
            <input
              className="rounded-xl border border-slate-200 px-3 py-2.5"
              placeholder="Email"
            />
            <select className="rounded-xl border border-slate-200 px-3 py-2.5">
              <option>Final Scoring</option>
              <option>Idea Pitch</option>
            </select>
          </div>
        </ModalShell>
      )}
    </div>
  );
}
