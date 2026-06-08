import { exportHistory } from "../coordinatorMockData";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorTable,
  icons,
} from "../CoordinatorUI";

export function ExportManagement() {
  const exports = [
    {
      title: "Teams CSV",
      icon: icons.Download,
      description: "All approved and pending teams",
    },
    {
      title: "Excel workbook",
      icon: icons.FileSpreadsheet,
      description: "Events, tracks, teams, and scores",
    },
    {
      title: "Anonymous RBL data",
      icon: icons.ShieldCheck,
      description: "De-identified research export",
    },
  ];
  const columns = [
    { key: "file", label: "File" },
    { key: "type", label: "Type" },
    { key: "createdBy", label: "Created by" },
    { key: "createdAt", label: "Created at" },
    { key: "status", label: "Status" },
  ];
  return (
    <div className="space-y-6">
      <div className="grid gap-4 md:grid-cols-3">
        {exports.map((item) => {
          const Icon = item.icon;
          return (
            <CoordinatorPanel
              key={item.title}
              title={item.title}
              subtitle={item.description}
              icon={Icon}
            >
              <CoordinatorActionButton variant="primary" icon={icons.Download}>
                Export
              </CoordinatorActionButton>
            </CoordinatorPanel>
          );
        })}
      </div>
      <CoordinatorPanel
        title="Export history"
        subtitle="Recently generated coordinator exports"
        icon={icons.Activity}
      >
        <CoordinatorTable
          columns={columns}
          rows={exportHistory}
          renderCell={(row, key) =>
            key === "status" ? (
              <CoordinatorBadge
                tone={
                  row.status === "Ready"
                    ? "success"
                    : row.status === "Processing"
                      ? "warning"
                      : "danger"
                }
              >
                {row.status}
              </CoordinatorBadge>
            ) : (
              row[key]
            )
          }
        />
      </CoordinatorPanel>
    </div>
  );
}
