import { useState } from "react";
import { users } from "../coordinatorMockData";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorTable,
  ModalShell,
  icons,
} from "../CoordinatorUI";

export function UsersManagement() {
  const [rejecting, setRejecting] = useState(null);
  const pending = users.filter((user) => user.status === "Pending");
  const columns = [
    { key: "name", label: "Leader" },
    { key: "team", label: "Team" },
    { key: "status", label: "Status" },
    { key: "submittedAt", label: "Submitted" },
    { key: "actions", label: "Actions" },
  ];
  return (
    <div className="space-y-6">
      <CoordinatorPanel
        title="Pending leader approvals"
        subtitle={`${pending.length} leader requests require review`}
        icon={icons.UserCheck}
      >
        <div className="mb-4 grid gap-3 md:grid-cols-3">
          <div className="relative md:col-span-2">
            <icons.Search className="absolute left-3 top-3 h-4 w-4 text-slate-400" />
            <input
              className="w-full rounded-xl border border-slate-200 py-2.5 pl-10 pr-3"
              placeholder="Search leaders"
            />
          </div>
          <select className="rounded-xl border border-slate-200 px-3 py-2.5">
            <option>All statuses</option>
          </select>
        </div>
        <CoordinatorTable
          columns={columns}
          rows={users}
          renderCell={(row, key) => {
            if (key === "name")
              return (
                <div>
                  <p className="font-bold text-slate-900">{row.name}</p>
                  <p className="text-xs text-slate-500">{row.email}</p>
                </div>
              );
            if (key === "status")
              return (
                <CoordinatorBadge
                  tone={
                    row.status === "Approved"
                      ? "success"
                      : row.status === "Rejected"
                        ? "danger"
                        : "warning"
                  }
                >
                  {row.status}
                </CoordinatorBadge>
              );
            if (key === "actions")
              return (
                <div className="flex gap-2">
                  <CoordinatorActionButton
                    variant="primary"
                    icon={icons.CheckCircle2}
                  >
                    Approve
                  </CoordinatorActionButton>
                  <CoordinatorActionButton
                    variant="danger"
                    icon={icons.X}
                    onClick={() => setRejecting(row)}
                  >
                    Reject
                  </CoordinatorActionButton>
                </div>
              );
            return row[key];
          }}
        />
      </CoordinatorPanel>
      {rejecting && (
        <ModalShell
          title={`Reject ${rejecting.name}?`}
          onClose={() => setRejecting(null)}
          actions={
            <>
              <CoordinatorActionButton onClick={() => setRejecting(null)}>
                Cancel
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="danger"
                onClick={() => setRejecting(null)}
              >
                Reject Leader
              </CoordinatorActionButton>
            </>
          }
        >
          <textarea
            className="min-h-28 w-full rounded-xl border border-slate-200 px-3 py-2.5"
            placeholder="Reject reason"
          />
        </ModalShell>
      )}
    </div>
  );
}
