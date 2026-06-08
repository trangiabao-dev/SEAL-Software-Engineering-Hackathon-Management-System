import { useState } from "react";
import { auditLogs } from "../coordinatorMockData";
import {
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorTable,
  icons,
} from "../CoordinatorUI";

export function AuditLog() {
  const [expanded, setExpanded] = useState("lg1");
  const columns = [
    { key: "timestamp", label: "Timestamp" },
    { key: "user", label: "User" },
    { key: "action", label: "Action" },
    { key: "entity", label: "Entity" },
    { key: "severity", label: "Severity" },
  ];
  return (
    <div className="space-y-6">
      <CoordinatorPanel
        title="Audit filters"
        subtitle="Filter by action, entity, and date range"
        icon={icons.Filter}
      >
        <div className="grid gap-3 md:grid-cols-4">
          <select className="rounded-xl border border-slate-200 px-3 py-2.5">
            <option>All actions</option>
          </select>
          <select className="rounded-xl border border-slate-200 px-3 py-2.5">
            <option>All entities</option>
          </select>
          <input
            type="date"
            className="rounded-xl border border-slate-200 px-3 py-2.5"
          />
          <input
            type="date"
            className="rounded-xl border border-slate-200 px-3 py-2.5"
          />
        </div>
      </CoordinatorPanel>
      <CoordinatorPanel
        title="Audit log table"
        subtitle="Click a row in the timeline below to inspect value changes"
        icon={icons.ShieldCheck}
      >
        <CoordinatorTable
          columns={columns}
          rows={auditLogs}
          renderCell={(row, key) =>
            key === "severity" ? (
              <CoordinatorBadge
                tone={
                  row.severity === "critical"
                    ? "danger"
                    : row.severity === "warning"
                      ? "warning"
                      : "info"
                }
              >
                {row.severity}
              </CoordinatorBadge>
            ) : (
              row[key]
            )
          }
        />
      </CoordinatorPanel>
      <CoordinatorPanel
        title="Timeline details"
        subtitle="Expandable old/new value history"
        icon={icons.Activity}
      >
        <div className="space-y-3">
          {auditLogs.map((log) => (
            <button
              key={log.id}
              onClick={() => setExpanded(expanded === log.id ? null : log.id)}
              className="w-full rounded-xl border border-slate-100 p-4 text-left hover:bg-orange-50/40"
            >
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="font-bold text-slate-900">{log.action}</p>
                  <p className="text-sm text-slate-500">
                    {log.user} • {log.timestamp}
                  </p>
                </div>
                <CoordinatorBadge
                  tone={
                    log.severity === "critical"
                      ? "danger"
                      : log.severity === "warning"
                        ? "warning"
                        : "info"
                  }
                >
                  {log.entity}
                </CoordinatorBadge>
              </div>
              {expanded === log.id && (
                <div className="mt-4 grid gap-3 text-sm md:grid-cols-2">
                  <div className="rounded-xl bg-slate-50 p-3">
                    <p className="font-bold text-slate-700">Old values</p>
                    <p className="mt-1 text-slate-500">{log.oldValues}</p>
                  </div>
                  <div className="rounded-xl bg-orange-50 p-3">
                    <p className="font-bold text-slate-700">New values</p>
                    <p className="mt-1 text-slate-500">{log.newValues}</p>
                  </div>
                </div>
              )}
            </button>
          ))}
        </div>
      </CoordinatorPanel>
    </div>
  );
}
