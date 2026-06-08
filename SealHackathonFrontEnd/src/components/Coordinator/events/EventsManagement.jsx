import { useState, useEffect, useCallback } from "react";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorTable,
  ModalShell,
  icons,
} from "../CoordinatorUI";
import axiosInstance from "../../../services/axiosInstance";

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
const STATUS_OPTIONS = ["Draft", "Open", "Ongoing", "Closed"];

const statusTone = (s) =>
  s === "Ongoing"
    ? "success"
    : s === "Open"
      ? "orange"
      : s === "Draft"
        ? "warning"
        : "neutral";

const EMPTY_FORM = {
  name: "",
  description: "",
  startDate: "",
  endDate: "",
  status: "Draft",
};

function formatDate(iso) {
  if (!iso) return "—";
  return iso.split("T")[0];
}

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
      <icons.X className="w-4 h-4 flex-shrink-0" />
      {msg}
    </div>
  );
}

// ---------------------------------------------------------------------------
// EventsManagement
// ---------------------------------------------------------------------------
export function EventsManagement() {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [apiError, setApiError] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");

  // Modals: null | "create" | "edit" | "delete"
  const [modal, setModal] = useState(null);
  const [selectedEvent, setSelectedEvent] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [saving, setSaving] = useState(false);

  // ---------------------------------------------------------------------------
  const fetchEvents = useCallback(async () => {
    setLoading(true);
    setApiError("");
    try {
      const res = await axiosInstance.get("/api/events");
      setEvents(res.data?.data || []);
    } catch (err) {
      setApiError(
        err?.response?.data?.message || "Không thể tải danh sách sự kiện.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchEvents();
  }, [fetchEvents]);

  // ---------------------------------------------------------------------------
  const openCreate = () => {
    setForm(EMPTY_FORM);
    setFormError("");
    setModal("create");
  };
  const openEdit = (ev) => {
    setSelectedEvent(ev);
    setForm({
      name: ev.name,
      description: ev.description || "",
      startDate: formatDate(ev.startDate),
      endDate: formatDate(ev.endDate),
      status: ev.status,
    });
    setFormError("");
    setModal("edit");
  };
  const openDelete = (ev) => {
    setSelectedEvent(ev);
    setModal("delete");
  };
  const closeModal = () => {
    setModal(null);
    setSelectedEvent(null);
    setFormError("");
  };

  const handleFormChange = (field, value) => {
    setForm((p) => ({ ...p, [field]: value }));
    setFormError("");
  };

  const validateForm = () => {
    if (!form.name.trim()) return "Tên sự kiện không được để trống.";
    if (!form.startDate) return "Vui lòng chọn ngày bắt đầu.";
    if (!form.endDate) return "Vui lòng chọn ngày kết thúc.";
    if (form.endDate < form.startDate)
      return "Ngày kết thúc phải sau ngày bắt đầu.";
    return "";
  };

  // CREATE
  const handleCreate = async () => {
    const err = validateForm();
    if (err) {
      setFormError(err);
      return;
    }
    setSaving(true);
    try {
      await axiosInstance.post("/api/events", {
        name: form.name.trim(),
        description: form.description.trim(),
        startDate: form.startDate,
        endDate: form.endDate,
        status: form.status,
      });
      await fetchEvents();
      closeModal();
    } catch (err) {
      setFormError(err?.response?.data?.message || "Tạo sự kiện thất bại.");
    } finally {
      setSaving(false);
    }
  };

  // EDIT
  const handleEdit = async () => {
    const err = validateForm();
    if (err) {
      setFormError(err);
      return;
    }
    setSaving(true);
    try {
      await axiosInstance.put(`/api/events/${selectedEvent.id}`, {
        name: form.name.trim(),
        description: form.description.trim(),
        startDate: form.startDate,
        endDate: form.endDate,
        status: form.status,
      });
      await fetchEvents();
      closeModal();
    } catch (err) {
      setFormError(err?.response?.data?.message || "Cập nhật thất bại.");
    } finally {
      setSaving(false);
    }
  };

  // DELETE
  const handleDelete = async () => {
    setSaving(true);
    try {
      await axiosInstance.delete(`/api/events/${selectedEvent.id}`);
      await fetchEvents();
      closeModal();
    } catch (err) {
      setFormError(err?.response?.data?.message || "Xóa thất bại.");
    } finally {
      setSaving(false);
    }
  };

  // ---------------------------------------------------------------------------
  const filtered = events.filter((ev) => {
    const matchSearch = ev.name.toLowerCase().includes(search.toLowerCase());
    const matchStatus = statusFilter === "All" || ev.status === statusFilter;
    return matchSearch && matchStatus;
  });

  const columns = [
    { key: "name", label: "Name" },
    { key: "dates", label: "Dates" },
    { key: "status", label: "Status" },
    { key: "actions", label: "Actions" },
  ];

  const renderCell = (row, key) => {
    if (key === "name")
      return (
        <div>
          <p className="font-bold text-slate-900">{row.name}</p>
          <p className="text-xs text-slate-500">{row.description}</p>
        </div>
      );
    if (key === "dates")
      return `${formatDate(row.startDate)} → ${formatDate(row.endDate)}`;
    if (key === "status")
      return (
        <CoordinatorBadge tone={statusTone(row.status)}>
          {row.status}
        </CoordinatorBadge>
      );
    if (key === "actions")
      return (
        <div className="flex gap-2">
          <CoordinatorActionButton
            icon={icons.Edit3}
            onClick={() => openEdit(row)}
          >
            Edit
          </CoordinatorActionButton>
          <CoordinatorActionButton
            variant="danger"
            icon={icons.Trash2}
            onClick={() => openDelete(row)}
          >
            Delete
          </CoordinatorActionButton>
        </div>
      );
    return row[key] ?? "—";
  };

  // ---------------------------------------------------------------------------
  return (
    <div className="space-y-6">
      {/* Filter panel */}
      <CoordinatorPanel
        title="Event controls"
        subtitle="Search and filter hackathon events"
        icon={icons.Filter}
        actions={
          <CoordinatorActionButton
            variant="primary"
            icon={icons.Plus}
            onClick={openCreate}
          >
            Create Event
          </CoordinatorActionButton>
        }
      >
        <div className="grid gap-3 md:grid-cols-3">
          <div className="relative md:col-span-2">
            <icons.Search className="absolute left-3 top-3 h-4 w-4 text-slate-400" />
            <input
              className="w-full rounded-xl border border-slate-200 py-2.5 pl-10 pr-3 text-sm outline-none"
              placeholder="Search events"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <select
            className="rounded-xl border border-slate-200 px-3 py-2.5 text-sm text-slate-600 outline-none"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="All">All statuses</option>
            {STATUS_OPTIONS.map((s) => (
              <option key={s}>{s}</option>
            ))}
          </select>
        </div>
      </CoordinatorPanel>

      {/* Table panel */}
      <CoordinatorPanel
        title="Events"
        subtitle="Only one event can be active at a time"
        icon={icons.CalendarDays}
      >
        {loading ? (
          <div className="flex items-center justify-center py-12 gap-2 text-sm text-slate-400">
            <icons.Clock
              className="w-4 h-4 animate-spin"
              style={{ color: "#F26F21" }}
            />
            Đang tải...
          </div>
        ) : apiError ? (
          <div className="flex flex-col items-center py-10 gap-3">
            <p className="text-sm text-red-500">{apiError}</p>
            <CoordinatorActionButton onClick={fetchEvents}>
              Thử lại
            </CoordinatorActionButton>
          </div>
        ) : filtered.length === 0 ? (
          <div className="py-12 text-center text-sm text-slate-400">
            Không có sự kiện nào.
          </div>
        ) : (
          <CoordinatorTable
            columns={columns}
            rows={filtered}
            renderCell={renderCell}
          />
        )}
      </CoordinatorPanel>

      {/* Modal: Create / Edit */}
      {(modal === "create" || modal === "edit") && (
        <ModalShell
          title={
            modal === "create"
              ? "Tạo sự kiện mới"
              : `Chỉnh sửa: ${selectedEvent?.name}`
          }
          onClose={closeModal}
          actions={
            <>
              <CoordinatorActionButton onClick={closeModal} disabled={saving}>
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={saving}
                onClick={modal === "create" ? handleCreate : handleEdit}
              >
                {saving
                  ? "Đang lưu..."
                  : modal === "create"
                    ? "Tạo sự kiện"
                    : "Lưu thay đổi"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <FormError msg={formError} />
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Tên sự kiện <span className="text-orange-500">*</span>
              </label>
              <input
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                placeholder="VD: FPT Hackathon 2026"
                value={form.name}
                onChange={(e) => handleFormChange("name", e.target.value)}
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Mô tả
              </label>
              <textarea
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none min-h-20"
                placeholder="Mô tả sự kiện"
                value={form.description}
                onChange={(e) =>
                  handleFormChange("description", e.target.value)
                }
              />
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              <div>
                <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                  Ngày bắt đầu <span className="text-orange-500">*</span>
                </label>
                <input
                  type="date"
                  className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                  value={form.startDate}
                  onChange={(e) =>
                    handleFormChange("startDate", e.target.value)
                  }
                />
              </div>
              <div>
                <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                  Ngày kết thúc <span className="text-orange-500">*</span>
                </label>
                <input
                  type="date"
                  className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                  value={form.endDate}
                  onChange={(e) => handleFormChange("endDate", e.target.value)}
                />
              </div>
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Trạng thái
              </label>
              <select
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                value={form.status}
                onChange={(e) => handleFormChange("status", e.target.value)}
              >
                {STATUS_OPTIONS.map((s) => (
                  <option key={s}>{s}</option>
                ))}
              </select>
            </div>
          </div>
        </ModalShell>
      )}

      {/* Modal: Delete */}
      {modal === "delete" && (
        <ModalShell
          title={`Xóa sự kiện: ${selectedEvent?.name}?`}
          onClose={closeModal}
          actions={
            <>
              <CoordinatorActionButton onClick={closeModal} disabled={saving}>
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="danger"
                disabled={saving}
                onClick={handleDelete}
              >
                {saving ? "Đang xóa..." : "Xác nhận xóa"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <p className="text-sm text-slate-600">
              Sự kiện sẽ bị ẩn khỏi hệ thống nhưng dữ liệu vẫn được giữ lại
              (soft delete).
            </p>
            <FormError msg={formError} />
          </div>
        </ModalShell>
      )}
    </div>
  );
}
