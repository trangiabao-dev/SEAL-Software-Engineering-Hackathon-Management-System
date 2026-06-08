import { useState, useEffect, useCallback } from "react";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorProgressBar,
  ModalShell,
  icons,
} from "../CoordinatorUI";
import axiosInstance from "../../../services/axiosInstance";

const EMPTY_FORM = { name: "", description: "", maxTeams: "", eventId: "" };

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

export function TracksManagement() {
  const [events, setEvents] = useState([]);
  const [selectedEventId, setSelectedEventId] = useState("");
  const [tracks, setTracks] = useState([]);
  const [loadingEvents, setLoadingEvents] = useState(true);
  const [loadingTracks, setLoadingTracks] = useState(false);
  const [apiError, setApiError] = useState("");

  const [modal, setModal] = useState(null); // null | "create" | "edit"
  const [selectedTrack, setSelectedTrack] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [saving, setSaving] = useState(false);

  // ---------------------------------------------------------------------------
  // Fetch events for dropdown
  const fetchEvents = useCallback(async () => {
    setLoadingEvents(true);
    try {
      const res = await axiosInstance.get("/api/events");
      const data = res.data?.data || [];
      setEvents(data);
      if (data.length > 0) setSelectedEventId(String(data[0].id));
    } catch {
      setApiError("Không thể tải danh sách sự kiện.");
    } finally {
      setLoadingEvents(false);
    }
  }, []);

  // Fetch tracks by eventId
  const fetchTracks = useCallback(async (eventId) => {
    if (!eventId) return;
    setLoadingTracks(true);
    setApiError("");
    try {
      const res = await axiosInstance.get(`/api/events/${eventId}/tracks`);
      setTracks(res.data?.data || []);
    } catch {
      setApiError("Không thể tải danh sách track.");
    } finally {
      setLoadingTracks(false);
    }
  }, []);

  useEffect(() => {
    fetchEvents();
  }, [fetchEvents]);
  useEffect(() => {
    if (selectedEventId) fetchTracks(selectedEventId);
  }, [selectedEventId, fetchTracks]);

  // ---------------------------------------------------------------------------
  const openCreate = () => {
    setForm({ ...EMPTY_FORM, eventId: selectedEventId });
    setFormError("");
    setModal("create");
  };

  const openEdit = (track) => {
    setSelectedTrack(track);
    setForm({
      name: track.name,
      description: track.description || "",
      maxTeams: String(track.maxTeams),
      eventId: String(track.eventId),
    });
    setFormError("");
    setModal("edit");
  };

  const closeModal = () => {
    setModal(null);
    setSelectedTrack(null);
    setFormError("");
  };

  const handleFormChange = (field, value) => {
    setForm((p) => ({ ...p, [field]: value }));
    setFormError("");
  };

  const validateForm = () => {
    if (!form.name.trim()) return "Tên track không được để trống.";
    if (!form.maxTeams || isNaN(form.maxTeams) || Number(form.maxTeams) < 1)
      return "Số đội tối đa phải là số nguyên dương.";
    if (!form.eventId) return "Vui lòng chọn sự kiện.";
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
      await axiosInstance.post("/tracks", {
        name: form.name.trim(),
        description: form.description.trim(),
        maxTeams: Number(form.maxTeams),
        eventId: Number(form.eventId),
      });
      await fetchTracks(selectedEventId);
      closeModal();
    } catch (err) {
      setFormError(err?.response?.data?.message || "Tạo track thất bại.");
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
      await axiosInstance.put(`/tracks/${selectedTrack.id}`, {
        name: form.name.trim(),
        description: form.description.trim(),
        maxTeams: Number(form.maxTeams),
        eventId: Number(form.eventId),
      });
      await fetchTracks(selectedEventId);
      closeModal();
    } catch (err) {
      setFormError(err?.response?.data?.message || "Cập nhật track thất bại.");
    } finally {
      setSaving(false);
    }
  };

  // ---------------------------------------------------------------------------
  const selectedEventName =
    events.find((e) => String(e.id) === String(selectedEventId))?.name || "";

  return (
    <div className="space-y-6">
      {/* Event selector */}
      <CoordinatorPanel
        title="Chọn sự kiện"
        subtitle="Xem và quản lý track theo từng sự kiện"
        icon={icons.CalendarDays}
        actions={
          <CoordinatorActionButton
            variant="primary"
            icon={icons.Plus}
            onClick={openCreate}
            disabled={!selectedEventId}
          >
            Add Track
          </CoordinatorActionButton>
        }
      >
        {loadingEvents ? (
          <p className="text-sm text-slate-400">Đang tải sự kiện...</p>
        ) : (
          <select
            className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm text-slate-700 outline-none"
            value={selectedEventId}
            onChange={(e) => setSelectedEventId(e.target.value)}
          >
            {events.map((ev) => (
              <option key={ev.id} value={ev.id}>
                {ev.name}
              </option>
            ))}
          </select>
        )}
      </CoordinatorPanel>

      {/* Track cards */}
      {loadingTracks ? (
        <div className="flex items-center justify-center py-12 gap-2 text-sm text-slate-400">
          <icons.Clock
            className="w-4 h-4 animate-spin"
            style={{ color: "#F26F21" }}
          />
          Đang tải track...
        </div>
      ) : apiError ? (
        <div className="flex flex-col items-center py-10 gap-3">
          <p className="text-sm text-red-500">{apiError}</p>
          <CoordinatorActionButton onClick={() => fetchTracks(selectedEventId)}>
            Thử lại
          </CoordinatorActionButton>
        </div>
      ) : tracks.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-slate-200 py-14 text-center text-sm text-slate-400">
          Chưa có track nào trong sự kiện này.
        </div>
      ) : (
        <>
          {/* Cards */}
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
            {tracks.map((track) => (
              <div
                key={track.id}
                className="rounded-2xl border bg-white p-5"
                style={{
                  borderColor: "#E5E7EB",
                  boxShadow: "0 10px 30px rgba(0,0,0,0.02)",
                }}
              >
                <div className="mb-4 flex items-start justify-between">
                  <div>
                    <h3 className="font-bold text-slate-900">{track.name}</h3>
                    <p className="text-xs text-slate-500 mt-0.5">
                      {track.description || "—"}
                    </p>
                  </div>
                  <CoordinatorBadge tone="info">#{track.id}</CoordinatorBadge>
                </div>
                <CoordinatorProgressBar
                  label={`${track.currentTeams ?? 0}/${track.maxTeams} teams`}
                  value={Math.round(
                    ((track.currentTeams ?? 0) / track.maxTeams) * 100,
                  )}
                />
                <div className="mt-4 flex justify-end">
                  <CoordinatorActionButton
                    icon={icons.Edit3}
                    onClick={() => openEdit(track)}
                  >
                    Edit
                  </CoordinatorActionButton>
                </div>
              </div>
            ))}
          </div>

          {/* List panel */}
          <CoordinatorPanel
            title={`Tracks — ${selectedEventName}`}
            subtitle="Danh sách chi tiết các track"
            icon={icons.GitBranch}
          >
            <div className="space-y-3">
              {tracks.map((track) => (
                <div
                  key={track.id}
                  className="flex flex-col gap-3 rounded-xl border border-slate-100 p-4 sm:flex-row sm:items-center sm:justify-between"
                >
                  <div>
                    <p className="font-bold text-slate-900">{track.name}</p>
                    <p className="text-sm text-slate-500">
                      {track.description || "—"} • Max teams: {track.maxTeams}
                    </p>
                  </div>
                  <CoordinatorActionButton
                    icon={icons.Edit3}
                    onClick={() => openEdit(track)}
                  >
                    Edit
                  </CoordinatorActionButton>
                </div>
              ))}
            </div>
          </CoordinatorPanel>
        </>
      )}

      {/* Modal: Create / Edit */}
      {(modal === "create" || modal === "edit") && (
        <ModalShell
          title={
            modal === "create"
              ? "Tạo Track mới"
              : `Chỉnh sửa: ${selectedTrack?.name}`
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
                    ? "Tạo Track"
                    : "Lưu thay đổi"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <FormError msg={formError} />
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Sự kiện <span className="text-orange-500">*</span>
              </label>
              <select
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                value={form.eventId}
                onChange={(e) => handleFormChange("eventId", e.target.value)}
              >
                <option value="">-- Chọn sự kiện --</option>
                {events.map((ev) => (
                  <option key={ev.id} value={ev.id}>
                    {ev.name}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Tên Track <span className="text-orange-500">*</span>
              </label>
              <input
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                placeholder="VD: AI & Data Science"
                value={form.name}
                onChange={(e) => handleFormChange("name", e.target.value)}
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Mô tả
              </label>
              <textarea
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none min-h-16"
                placeholder="Mô tả track"
                value={form.description}
                onChange={(e) =>
                  handleFormChange("description", e.target.value)
                }
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Số đội tối đa <span className="text-orange-500">*</span>
              </label>
              <input
                type="number"
                min="1"
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                placeholder="VD: 20"
                value={form.maxTeams}
                onChange={(e) => handleFormChange("maxTeams", e.target.value)}
              />
            </div>
          </div>
        </ModalShell>
      )}
    </div>
  );
}
