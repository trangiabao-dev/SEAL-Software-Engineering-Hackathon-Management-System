import { useState } from "react";
import { createPortal } from "react-dom";
import {
  Activity,
  Bell,
  CalendarDays,
  CheckCircle2,
  Clock,
  Download,
  Edit3,
  Eye,
  FileSpreadsheet,
  Filter,
  GitBranch,
  Handshake,
  LayoutDashboard,
  Lightbulb,
  Lock,
  Menu,
  MoreHorizontal,
  Plus,
  Scale,
  Search,
  ShieldCheck,
  SlidersHorizontal,
  Timer,
  Trash2,
  Trophy,
  Upload,
  UserCheck,
  UserRoundCog,
  Users,
  X,
} from "lucide-react";

export const icons = {
  Activity,
  Bell,
  CalendarDays,
  CheckCircle2,
  Clock,
  Download,
  Edit3,
  Eye,
  FileSpreadsheet,
  Filter,
  GitBranch,
  Handshake,
  LayoutDashboard,
  Lightbulb,
  Lock,
  Menu,
  MoreHorizontal,
  Plus,
  Scale,
  Search,
  ShieldCheck,
  SlidersHorizontal,
  Timer,
  Trash2,
  Trophy,
  Upload,
  UserCheck,
  UserRoundCog,
  Users,
  X,
};

export function CoordinatorBadge({ tone = "neutral", children }) {
  const tones = {
    success: "bg-emerald-50 text-emerald-700 border-emerald-200",
    warning: "bg-amber-50 text-amber-700 border-amber-200",
    danger: "bg-red-50 text-red-700 border-red-200",
    info: "bg-blue-50 text-blue-700 border-blue-200",
    orange: "bg-orange-50 text-orange-700 border-orange-200",
    purple: "bg-purple-50 text-purple-700 border-purple-200",
    neutral: "bg-slate-50 text-slate-600 border-slate-200",
  };

  return (
    <span
      className={`inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-semibold ${
        tones[tone] || tones.neutral
      }`}
    >
      {children}
    </span>
  );
}

export function CoordinatorActionButton({
  variant = "secondary",
  children,
  icon: Icon,
  onClick,
  className = "",
  disabled = false,
}) {
  const variants = {
    primary:
      "text-white border-transparent shadow-sm shadow-orange-200 bg-gradient-to-r from-[#F26F21] to-[#c9520e] hover:shadow-orange-300",
    secondary:
      "text-slate-700 bg-white border-slate-300 hover:bg-slate-50 hover:border-slate-400",
    ghost:
      "text-slate-600 bg-transparent border-transparent hover:bg-slate-100",
    danger: "text-red-700 bg-red-50 border-red-300 hover:bg-red-100",
  };

  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      className={`inline-flex items-center justify-center gap-2 rounded-xl border px-3.5 py-2 text-sm font-semibold transition-all ${
        disabled
          ? "cursor-not-allowed opacity-50 hover:scale-100"
          : "hover:scale-[1.02]"
      } ${variants[variant]} ${className}`}
    >
      {Icon && <Icon className="h-4 w-4" />}
      {children}
    </button>
  );
}

export function CoordinatorPanel({
  title,
  subtitle,
  icon: Icon,
  actions,
  children,
  className = "",
}) {
  return (
    <section
      className={`rounded-2xl border bg-white p-5 transition-all duration-300 hover:shadow-md hover:border-slate-300/50 animate-fade-in ${className}`}
      style={{
        borderColor: "#E5E7EB",
        boxShadow: "0 10px 30px rgba(0,0,0,0.02)",
      }}
    >
      {(title || actions) && (
        <div className="mb-5 flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div className="flex items-start gap-3">
            {Icon && (
              <div
                className="flex h-9 w-9 items-center justify-center rounded-lg"
                style={{
                  background: "rgba(242,111,33,0.1)",
                  border: "1px solid rgba(242,111,33,0.2)",
                }}
              >
                <Icon className="h-4 w-4" style={{ color: "#F26F21" }} />
              </div>
            )}
            <div>
              {title && <h3 className="font-bold text-slate-900">{title}</h3>}
              {subtitle && (
                <p className="mt-1 text-sm text-slate-500">{subtitle}</p>
              )}
            </div>
          </div>
          {actions && <div className="flex flex-wrap gap-2">{actions}</div>}
        </div>
      )}
      {children}
    </section>
  );
}

export function CoordinatorStatCard({
  label,
  value,
  icon: Icon,
  tone = "orange",
  delta,
  helper,
}) {
  const toneMap = {
    orange: ["rgba(242,111,33,0.1)", "rgba(242,111,33,0.2)", "#F26F21"],
    blue: ["rgba(37,99,235,0.08)", "rgba(37,99,235,0.18)", "#2563EB"],
    green: ["rgba(5,150,105,0.08)", "rgba(5,150,105,0.18)", "#059669"],
    amber: ["rgba(217,119,6,0.08)", "rgba(217,119,6,0.18)", "#D97706"],
    red: ["rgba(220,38,38,0.08)", "rgba(220,38,38,0.18)", "#DC2626"],
  };
  const [bg, border, color] = toneMap[tone] || toneMap.orange;

  return (
    <div
      className="rounded-2xl border bg-white p-5 transition-all duration-300 hover:scale-[1.015] hover:shadow-md hover:border-slate-300/80"
      style={{
        borderColor: "#E5E7EB",
        boxShadow: "0 10px 30px rgba(0,0,0,0.02)",
      }}
    >
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-sm font-semibold text-slate-500">{label}</p>
          <p className="mt-2 text-3xl font-bold text-slate-900">{value}</p>
        </div>
        {Icon && (
          <div
            className="flex h-11 w-11 items-center justify-center rounded-xl"
            style={{ background: bg, border: `1px solid ${border}` }}
          >
            <Icon className="h-5 w-5" style={{ color }} />
          </div>
        )}
      </div>
      {(delta || helper) && (
        <div className="mt-4 flex items-center justify-between gap-2 text-xs">
          {delta && (
            <span className="font-bold" style={{ color }}>
              {delta}
            </span>
          )}
          {helper && <span className="text-slate-500">{helper}</span>}
        </div>
      )}
    </div>
  );
}

export function CoordinatorProgressBar({
  value = 0,
  label,
  color = "#F26F21",
}) {
  return (
    <div>
      {(label || value !== undefined) && (
        <div className="mb-2 flex items-center justify-between text-sm">
          {label && (
            <span className="font-semibold text-slate-700">{label}</span>
          )}
          <span className="font-bold text-slate-900">{value}%</span>
        </div>
      )}
      <div className="h-2.5 overflow-hidden rounded-full bg-slate-100">
        <div
          className="h-full rounded-full transition-all"
          style={{
            width: `${Math.min(100, Math.max(0, value))}%`,
            background: color,
          }}
        />
      </div>
    </div>
  );
}

export function CoordinatorTable({
  columns,
  rows,
  renderCell,
  emptyMessage = "No records found",
}) {
  if (!rows?.length) {
    return (
      <div className="rounded-xl border border-dashed border-slate-200 p-8 text-center text-sm text-slate-500">
        {emptyMessage}
      </div>
    );
  }

  return (
    <div
      className="overflow-x-auto rounded-xl border"
      style={{ borderColor: "#E5E7EB" }}
    >
      <table className="min-w-full divide-y divide-slate-100 text-left text-sm">
        <thead className="bg-slate-50">
          <tr>
            {columns.map((column) => (
              <th
                key={column.key}
                className="whitespace-nowrap px-4 py-3 text-xs font-bold uppercase tracking-wider text-slate-500"
              >
                {column.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100 bg-white">
          {rows.map((row) => (
            <tr
              key={row.id}
              className="hover:bg-orange-50/30 transition-colors duration-150"
            >
              {columns.map((column) => (
                <td
                  key={column.key}
                  className="whitespace-nowrap px-4 py-3 text-slate-700"
                >
                  {renderCell ? renderCell(row, column.key) : row[column.key]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
export function ModalShell({ title, children, onClose, actions }) {
  return createPortal(
    <div className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-950/50 backdrop-blur-sm p-4 animate-fade-in">
      <div className="w-full max-w-xl rounded-2xl bg-white p-6 shadow-2xl animate-modal-scale">
        <div className="mb-5 flex items-start justify-between gap-4">
          <h3 className="text-lg font-bold text-slate-900">{title}</h3>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-1 text-slate-500 hover:bg-slate-100"
          >
            <X className="h-5 w-5" />
          </button>
        </div>
        {children}
        {actions && (
          <div className="mt-6 flex justify-end gap-2">{actions}</div>
        )}
      </div>
    </div>,
    document.body
  );
}
