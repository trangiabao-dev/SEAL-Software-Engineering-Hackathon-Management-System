import { useState, useEffect, useCallback } from "react";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorTable,
  ModalShell,
  icons,
} from "../CoordinatorUI";
import { UserPlus, AlertCircle, Loader2, CheckCircle } from "lucide-react";
import axiosInstance from "../../../services/axiosInstance";

const extIcons = { ...icons, UserPlus, AlertCircle, Loader2, CheckCircle };

const STATUS_FILTERS = ["All", "Pending", "Approved", "Disqualified"];

// TODO: thay bằng GET /api/mentors khi BE có endpoint
const MOCK_MENTORS = [
  { id: "m1", name: "Linh Tran" },
  { id: "m2", name: "Huy Vo" },
  { id: "m3", name: "Mai Do" },
];

function FormError({ msg }) {
  if (!msg) return null;
  return (
    <div
      className="flex items-center gap-2 p-3 rounded-xl text-sm"
      style={{
        background: "rgba(239,68,68,0.06)",
        border: "1px solid rgba(239,68,68,0.2)",
        color: "#dc2626",
      }}
    >
      <AlertCircle className="w-4 h-4 flex-shrink-0" />
      {msg}
    </div>
  );
}

function FilterBar({ status, onStatus, onSearch, search }) {
  return (
    <div className="flex flex-wrap items-center gap-3 mb-4">
      <div className="relative flex-1 min-w-48">
        <icons.Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
        <input
          className="w-full rounded-xl border border-slate-200 py-2 pl-9 pr-3 text-sm outline-none"
          placeholder="Tìm tên team, trường..."
          value={search}
          onChange={(e) => onSearch(e.target.value)}
        />
      </div>
      <div className="flex gap-2 flex-wrap">
        {STATUS_FILTERS.map((s) => (
          <button
            key={s}
            onClick={() => onStatus(s)}
            className="px-3 py-1.5 rounded-lg text-xs font-semibold transition-all duration-150"
            style={{
              background: status === s ? "#F26F21" : "#F3F4F6",
              color: status === s ? "#fff" : "#374151",
              border: `1px solid ${status === s ? "#F26F21" : "#E5E7EB"}`,
            }}
          >
            {s}
          </button>
        ))}
      </div>
    </div>
  );
}

function Pagination({ page, totalPages, onPage }) {
  if (totalPages <= 1) return null;
  return (
    <div className="flex items-center justify-center gap-2 mt-4">
      <CoordinatorActionButton
        disabled={page === 1}
        onClick={() => onPage(page - 1)}
      >
        ← Prev
      </CoordinatorActionButton>
      <span className="text-sm text-slate-600 px-2">
        Trang {page} / {totalPages}
      </span>
      <CoordinatorActionButton
        disabled={page === totalPages}
        onClick={() => onPage(page + 1)}
      >
        Next →
      </CoordinatorActionButton>
    </div>
  );
}

export function TeamsManagement() {
  const [teams, setTeams] = useState([]);
  const [loading, setLoading] = useState(true);
  const [apiError, setApiError] = useState("");

  // Pagination
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);

  // Filters
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");

  // Modals
  const [detailTeam, setDetailTeam] = useState(null);
  const [disqualifyTeam, setDisqualifyTeam] = useState(null);
  const [assignMentorTeam, setAssignMentorTeam] = useState(null);

  // Action states
  const [actionLoading, setActionLoading] = useState("");
  const [actionError, setActionError] = useState("");
  const [disqualifyReason, setDisqualifyReason] = useState("");
  const [selectedMentorId, setSelectedMentorId] = useState("");

  // ---------------------------------------------------------------------------
  const fetchTeams = useCallback(
    async (p = 1) => {
      setLoading(true);
      setApiError("");
      try {
        const params = new URLSearchParams();
        params.append("pageNumber", p);
        params.append("pageSize", pageSize);
        if (statusFilter !== "All") params.append("status", statusFilter);
        // TODO: thêm trackId filter khi có track selector

        const res = await axiosInstance.get(
          `/api/admin/teams?${params.toString()}`,
        );
        const data = res.data?.data;
        setTeams(data?.items || []);
        setTotalPages(data?.totalPages || 1);
        setPage(data?.pageNumber || 1);
      } catch (err) {
        setApiError(
          err?.response?.data?.message || "Không thể tải danh sách team.",
        );
      } finally {
        setLoading(false);
      }
    },
    [statusFilter, pageSize],
  );

  useEffect(() => {
    fetchTeams(1);
  }, [fetchTeams]);

  const handlePageChange = (p) => fetchTeams(p);

  const handleStatusFilter = (s) => {
    setStatusFilter(s);
    setPage(1);
  };

  // ---------------------------------------------------------------------------
  const handleApprove = async (team) => {
    setActionLoading(team.id);
    setActionError("");
    try {
      await axiosInstance.put(`/api/teams/${team.id}/approve`);
      setTeams((prev) =>
        prev.map((t) => (t.id === team.id ? { ...t, status: "Approved" } : t)),
      );
    } catch (err) {
      setActionError(err?.response?.data?.message || "Approve thất bại.");
    } finally {
      setActionLoading("");
    }
  };

  const handleDisqualifyConfirm = async () => {
    setActionLoading(disqualifyTeam.id);
    setActionError("");
    try {
      await axiosInstance.put(`/api/teams/${disqualifyTeam.id}/disqualify`, {
        reason: disqualifyReason.trim() || undefined,
      });
      setTeams((prev) =>
        prev.map((t) =>
          t.id === disqualifyTeam.id ? { ...t, status: "Disqualified" } : t,
        ),
      );
      setDisqualifyTeam(null);
      setDisqualifyReason("");
    } catch (err) {
      setActionError(err?.response?.data?.message || "Disqualify thất bại.");
    } finally {
      setActionLoading("");
    }
  };

  const handleAssignMentorConfirm = async () => {
    setActionLoading(assignMentorTeam.id);
    setActionError("");
    try {
      await axiosInstance.put(`/api/teams/${assignMentorTeam.id}/mentor`, {
        mentorId: selectedMentorId,
      });
      const mentor = MOCK_MENTORS.find((m) => m.id === selectedMentorId);
      setTeams((prev) =>
        prev.map((t) =>
          t.id === assignMentorTeam.id
            ? { ...t, mentorId: selectedMentorId, mentorName: mentor?.name }
            : t,
        ),
      );
      setAssignMentorTeam(null);
      setSelectedMentorId("");
    } catch (err) {
      setActionError(err?.response?.data?.message || "Assign mentor thất bại.");
    } finally {
      setActionLoading("");
    }
  };

  // ---------------------------------------------------------------------------
  // Client-side search filter (trên data đã fetch)
  const filtered = teams.filter(
    (t) =>
      t.teamName?.toLowerCase().includes(search.toLowerCase()) ||
      t.university?.toLowerCase().includes(search.toLowerCase()),
  );

  const columns = [
    { key: "name", label: "Team" },
    { key: "track", label: "Track" },
    { key: "mentor", label: "Mentor" },
    { key: "status", label: "Status" },
    { key: "actions", label: "Actions" },
  ];

  const renderCell = (row, key) => {
    const isActioning = actionLoading === row.id;

    if (key === "name")
      return (
        <div>
          <p className="font-bold text-slate-900">{row.teamName}</p>
          <p className="text-xs text-slate-500 truncate max-w-48">
            {row.university} • {row.members?.length ?? 0} members
          </p>
        </div>
      );

    if (key === "track")
      return (
        <span className="text-sm text-slate-600">
          {row.trackId ? `Track #${row.trackId}` : "—"}
          {/* TODO: map trackId → trackName khi có GET /api/tracks */}
        </span>
      );

    if (key === "mentor")
      return (
        <button
          className="text-sm transition-colors duration-150"
          style={{ color: row.mentorId ? "#374151" : "#F26F21" }}
          onClick={() => {
            setAssignMentorTeam(row);
            setSelectedMentorId("");
            setActionError("");
          }}
        >
          {row.mentorId ? (
            <span>
              {row.mentorName || `Mentor #${row.mentorId.slice(0, 8)}...`}
            </span>
          ) : (
            <span className="flex items-center gap-1 text-xs font-semibold">
              <UserPlus className="w-3.5 h-3.5" /> Assign
            </span>
          )}
        </button>
      );

    if (key === "status")
      return (
        <CoordinatorBadge
          tone={
            row.status === "Approved"
              ? "success"
              : row.status === "Disqualified"
                ? "danger"
                : "warning"
          }
        >
          {row.status}
        </CoordinatorBadge>
      );

    if (key === "actions")
      return (
        <div className="flex gap-2 flex-wrap">
          <CoordinatorActionButton
            icon={icons.Eye}
            onClick={() => {
              setDetailTeam(row);
              setActionError("");
            }}
          >
            Details
          </CoordinatorActionButton>
          {row.status === "Pending" && (
            <CoordinatorActionButton
              variant="primary"
              disabled={isActioning}
              onClick={() => handleApprove(row)}
            >
              {isActioning ? "..." : "Approve"}
            </CoordinatorActionButton>
          )}
          {row.status === "Approved" && (
            <CoordinatorActionButton
              variant="danger"
              disabled={isActioning}
              onClick={() => {
                setDisqualifyTeam(row);
                setDisqualifyReason("");
                setActionError("");
              }}
            >
              Disqualify
            </CoordinatorActionButton>
          )}
        </div>
      );

    return row[key] ?? "—";
  };

  // ---------------------------------------------------------------------------
  return (
    <div className="space-y-6">
      {actionError && (
        <div
          className="flex items-center gap-2 px-4 py-3 rounded-xl text-sm"
          style={{
            background: "rgba(239,68,68,0.06)",
            border: "1px solid rgba(239,68,68,0.2)",
            color: "#dc2626",
          }}
        >
          <AlertCircle className="w-4 h-4 flex-shrink-0" />
          {actionError}
        </div>
      )}

      <CoordinatorPanel
        title="Team list"
        subtitle="Approve, inspect, or disqualify participating teams"
        icon={icons.UserRoundCog}
      >
        <FilterBar
          status={statusFilter}
          onStatus={handleStatusFilter}
          search={search}
          onSearch={setSearch}
        />

        {loading ? (
          <div className="flex items-center justify-center py-14 gap-2 text-sm text-slate-400">
            <Loader2
              className="w-4 h-4 animate-spin"
              style={{ color: "#F26F21" }}
            />
            Đang tải...
          </div>
        ) : apiError ? (
          <div className="flex flex-col items-center py-10 gap-3">
            <p className="text-sm text-red-500">{apiError}</p>
            <CoordinatorActionButton onClick={() => fetchTeams(1)}>
              Thử lại
            </CoordinatorActionButton>
          </div>
        ) : filtered.length === 0 ? (
          <div className="py-14 text-center text-sm text-slate-400">
            Chưa có team nào.
          </div>
        ) : (
          <>
            <CoordinatorTable
              columns={columns}
              rows={filtered}
              renderCell={renderCell}
            />
            <Pagination
              page={page}
              totalPages={totalPages}
              onPage={handlePageChange}
            />
          </>
        )}
      </CoordinatorPanel>

      {/* Modal: Detail */}
      {detailTeam && (
        <ModalShell
          title={`Chi tiết: ${detailTeam.teamName}`}
          onClose={() => setDetailTeam(null)}
          actions={
            <CoordinatorActionButton
              variant="primary"
              onClick={() => setDetailTeam(null)}
            >
              Đóng
            </CoordinatorActionButton>
          }
        >
          <div className="space-y-2 text-sm">
            <InfoRow label="Tên team" value={detailTeam.teamName} />
            <InfoRow label="Trường" value={detailTeam.university} />
            <InfoRow
              label="Track"
              value={detailTeam.trackId ? `#${detailTeam.trackId}` : "—"}
            />
            <InfoRow
              label="GitHub"
              value={detailTeam.githubRepoLink}
              isLink={!!detailTeam.githubRepoLink}
            />
            <InfoRow
              label="Thành viên"
              value={`${detailTeam.members?.length ?? 0} người`}
            />
            <InfoRow
              label="Status"
              value={
                <CoordinatorBadge
                  tone={
                    detailTeam.status === "Approved"
                      ? "success"
                      : detailTeam.status === "Disqualified"
                        ? "danger"
                        : "warning"
                  }
                >
                  {detailTeam.status}
                </CoordinatorBadge>
              }
            />
          </div>
        </ModalShell>
      )}

      {/* Modal: Disqualify */}
      {disqualifyTeam && (
        <ModalShell
          title={`Disqualify: ${disqualifyTeam.teamName}?`}
          onClose={() => {
            setDisqualifyTeam(null);
            setActionError("");
          }}
          actions={
            <>
              <CoordinatorActionButton
                onClick={() => setDisqualifyTeam(null)}
                disabled={actionLoading === disqualifyTeam.id}
              >
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="danger"
                disabled={actionLoading === disqualifyTeam.id}
                onClick={handleDisqualifyConfirm}
              >
                {actionLoading === disqualifyTeam.id
                  ? "Đang xử lý..."
                  : "Xác nhận loại"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <p className="text-sm text-slate-600">
              Bạn có chắc muốn loại team{" "}
              <strong>{disqualifyTeam.teamName}</strong>?
            </p>
            <textarea
              className="min-h-24 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              placeholder="Lý do loại team (tuỳ chọn)"
              value={disqualifyReason}
              onChange={(e) => setDisqualifyReason(e.target.value)}
            />
            <FormError msg={actionError} />
          </div>
        </ModalShell>
      )}

      {/* Modal: Assign Mentor */}
      {assignMentorTeam && (
        <ModalShell
          title={`Assign Mentor: ${assignMentorTeam.teamName}`}
          onClose={() => {
            setAssignMentorTeam(null);
            setActionError("");
          }}
          actions={
            <>
              <CoordinatorActionButton
                onClick={() => setAssignMentorTeam(null)}
              >
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={
                  !selectedMentorId || actionLoading === assignMentorTeam.id
                }
                onClick={handleAssignMentorConfirm}
              >
                {actionLoading === assignMentorTeam.id
                  ? "Đang xử lý..."
                  : "Assign"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <p className="text-sm text-slate-500">
              Chọn mentor cho team{" "}
              <strong className="text-slate-800">
                {assignMentorTeam.teamName}
              </strong>
              :
            </p>
            {/* TODO: thay MOCK_MENTORS bằng GET /api/mentors */}
            <div className="space-y-2">
              {MOCK_MENTORS.map((m) => (
                <button
                  key={m.id}
                  onClick={() => setSelectedMentorId(m.id)}
                  className="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm transition-all duration-150 text-left"
                  style={{
                    background:
                      selectedMentorId === m.id
                        ? "rgba(242,111,33,0.08)"
                        : "#F9FAFB",
                    border: `1px solid ${selectedMentorId === m.id ? "#F26F21" : "#E5E7EB"}`,
                  }}
                >
                  <div
                    className="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold text-white flex-shrink-0"
                    style={{ background: "#F26F21" }}
                  >
                    {m.name.charAt(0)}
                  </div>
                  <span className="font-semibold text-slate-700">{m.name}</span>
                  {selectedMentorId === m.id && (
                    <CheckCircle
                      className="w-4 h-4 ml-auto"
                      style={{ color: "#F26F21" }}
                    />
                  )}
                </button>
              ))}
            </div>
            <FormError msg={actionError} />
          </div>
        </ModalShell>
      )}
    </div>
  );
}

function InfoRow({ label, value, isLink }) {
  return (
    <div className="flex items-start justify-between gap-4">
      <span className="text-xs text-slate-400 flex-shrink-0 w-24">{label}</span>
      {isLink ? (
        <a
          href={value}
          target="_blank"
          rel="noopener noreferrer"
          className="text-xs font-medium truncate"
          style={{ color: "#F26F21" }}
        >
          {value}
        </a>
      ) : (
        <span className="text-xs font-semibold text-slate-800 text-right">
          {typeof value === "string" ? value : value}
        </span>
      )}
    </div>
  );
}
