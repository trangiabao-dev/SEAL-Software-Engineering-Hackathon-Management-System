import { useState, useEffect, useCallback } from "react";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorProgressBar,
  ModalShell,
  icons,
} from "../CoordinatorUI";
import { AlertCircle, Loader2 } from "lucide-react";
import axiosInstance from "../../../services/axiosInstance";

// ---------------------------------------------------------------------------
const STATUS_OPTIONS = ["Upcoming", "Active", "Scoring", "Completed"];

const statusTone = (s) =>
  s === "Active"
    ? "orange"
    : s === "Scoring"
      ? "purple"
      : s === "Completed"
        ? "success"
        : "neutral";

function formatDateTime(iso) {
  if (!iso) return "—";
  return iso.replace("T", " ").slice(0, 16);
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
      <AlertCircle className="w-4 h-4 flex-shrink-0" />
      {msg}
    </div>
  );
}

const EMPTY_FORM = {
  trackId: "",
  name: "",
  orderIndex: "",
  startTime: "",
  endTime: "",
  advancingSlots: "",
};

// ---------------------------------------------------------------------------
export function RoundsManagement() {
  const [tracks, setTracks] = useState([]); // [{trackId, trackName, eventName, rounds:[]}]
  const [loading, setLoading] = useState(true);
  const [apiError, setApiError] = useState("");

  const [modal, setModal] = useState(null); // null | "create" | "edit" | "status"
  const [selectedRound, setSelectedRound] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [statusValue, setStatusValue] = useState("");
  const [formError, setFormError] = useState("");
  const [saving, setSaving] = useState(false);

  // ---------------------------------------------------------------------------
  const fetchRounds = useCallback(async () => {
    setLoading(true);
    setApiError("");
    try {
      const res = await axiosInstance.get("/api/tracks/rounds");
      setTracks(res.data?.data || []);
    } catch (err) {
      setApiError(
        err?.response?.data?.message || "Không thể tải danh sách rounds.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchRounds();
  }, [fetchRounds]);

  // ---------------------------------------------------------------------------
  const openCreate = (trackId) => {
    setForm({ ...EMPTY_FORM, trackId: String(trackId) });
    setFormError("");
    setModal("create");
  };

  const openEdit = (round, trackId) => {
    setSelectedRound({ ...round, trackId });
    setForm({
      trackId: String(trackId),
      name: round.name,
      orderIndex: String(round.orderIndex ?? ""),
      startTime: round.startTime?.slice(0, 16) || "",
      endTime: round.endTime?.slice(0, 16) || "",
      advancingSlots: String(round.advancingSlots),
    });
    setFormError("");
    setModal("edit");
  };

  const openStatus = (round) => {
    setSelectedRound(round);
    setStatusValue(round.status);
    setFormError("");
    setModal("status");
  };

  const closeModal = () => {
    setModal(null);
    setSelectedRound(null);
    setFormError("");
  };

  const handleFormChange = (field, value) => {
    setForm((p) => ({ ...p, [field]: value }));
    setFormError("");
  };

  const validateForm = () => {
    if (!form.name.trim()) return "Tên vòng không được để trống.";
    if (!form.trackId) return "Vui lòng chọn track.";
    if (!form.startTime) return "Vui lòng chọn thời gian bắt đầu.";
    if (!form.endTime) return "Vui lòng chọn thời gian kết thúc.";
    if (form.endTime <= form.startTime)
      return "Thời gian kết thúc phải sau bắt đầu.";
    if (!form.advancingSlots || Number(form.advancingSlots) < 1)
      return "Số suất đi tiếp phải lớn hơn 0.";
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
      await axiosInstance.post("/api/rounds", {
        trackId: Number(form.trackId),
        name: form.name.trim(),
        orderIndex: Number(form.orderIndex) || 1,
        startTime: new Date(form.startTime).toISOString(),
        endTime: new Date(form.endTime).toISOString(),
        advancingSlots: Number(form.advancingSlots),
      });
      await fetchRounds();
      closeModal();
    } catch (err) {
      setFormError(err?.response?.data?.message || "Tạo round thất bại.");
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
      await axiosInstance.put(`/api/rounds/${selectedRound.roundId}`, {
        name: form.name.trim(),
        orderIndex: Number(form.orderIndex) || 1,
        startTime: new Date(form.startTime).toISOString(),
        endTime: new Date(form.endTime).toISOString(),
        advancingSlots: Number(form.advancingSlots),
      });
      await fetchRounds();
      closeModal();
    } catch (err) {
      setFormError(err?.response?.data?.message || "Cập nhật round thất bại.");
    } finally {
      setSaving(false);
    }
  };

  // STATUS
  const handleStatusUpdate = async () => {
    if (!statusValue) return;
    setSaving(true);
    try {
      await axiosInstance.put(`/api/rounds/${selectedRound.roundId}/status`, {
        status: statusValue,
      });
      await fetchRounds();
      closeModal();
    } catch (err) {
      setFormError(
        err?.response?.data?.message || "Cập nhật trạng thái thất bại.",
      );
    } finally {
      setSaving(false);
    }
  };

  // ---------------------------------------------------------------------------
  // Round form modal (shared create/edit)
  const RoundFormModal = (
    <ModalShell
      title={
        modal === "create"
          ? "Tạo Round mới"
          : `Chỉnh sửa: ${selectedRound?.name}`
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
                ? "Tạo Round"
                : "Lưu thay đổi"}
          </CoordinatorActionButton>
        </>
      }
    >
      <div className="space-y-3">
        <FormError msg={formError} />
        {/* Track selector — chỉ hiện khi create */}
        {modal === "create" && (
          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
              Track <span className="text-orange-500">*</span>
            </label>
            <select
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              value={form.trackId}
              onChange={(e) => handleFormChange("trackId", e.target.value)}
            >
              <option value="">-- Chọn track --</option>
              {tracks.map((t) => (
                <option key={t.trackId} value={t.trackId}>
                  {t.trackName} — {t.eventName}
                </option>
              ))}
            </select>
          </div>
        )}
        <div>
          <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
            Tên Round <span className="text-orange-500">*</span>
          </label>
          <input
            className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
            placeholder="VD: Vòng Sơ Loại"
            value={form.name}
            onChange={(e) => handleFormChange("name", e.target.value)}
          />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
              Order
            </label>
            <input
              type="number"
              min="1"
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              placeholder="1"
              value={form.orderIndex}
              onChange={(e) => handleFormChange("orderIndex", e.target.value)}
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
              Suất đi tiếp <span className="text-orange-500">*</span>
            </label>
            <input
              type="number"
              min="1"
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              placeholder="10"
              value={form.advancingSlots}
              onChange={(e) =>
                handleFormChange("advancingSlots", e.target.value)
              }
            />
          </div>
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
              Bắt đầu <span className="text-orange-500">*</span>
            </label>
            <input
              type="datetime-local"
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              value={form.startTime}
              onChange={(e) => handleFormChange("startTime", e.target.value)}
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
              Kết thúc <span className="text-orange-500">*</span>
            </label>
            <input
              type="datetime-local"
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              value={form.endTime}
              onChange={(e) => handleFormChange("endTime", e.target.value)}
            />
          </div>
        </div>
      </div>
    </ModalShell>
  );

  // ---------------------------------------------------------------------------
  return (
    <div className="space-y-6">
      <CoordinatorPanel
        title="Round Management"
        subtitle="Quản lý vòng thi theo từng track và sự kiện"
        icon={icons.Timer}
        actions={
          <CoordinatorActionButton
            variant="primary"
            icon={icons.Plus}
            onClick={() => openCreate("")}
          >
            Create Round
          </CoordinatorActionButton>
        }
      >
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
            <CoordinatorActionButton onClick={fetchRounds}>
              Thử lại
            </CoordinatorActionButton>
          </div>
        ) : tracks.length === 0 ? (
          <div className="py-14 text-center text-sm text-slate-400">
            Chưa có round nào.
          </div>
        ) : (
          <div className="space-y-8">
            {tracks.map((track) => (
              <div key={track.trackId}>
                {/* Track header */}
                <div className="flex items-center justify-between mb-3">
                  <div>
                    <h3 className="font-bold text-slate-800 flex items-center gap-2">
                      <icons.GitBranch
                        className="w-4 h-4"
                        style={{ color: "#F26F21" }}
                      />
                      {track.trackName}
                    </h3>
                    <p className="text-xs text-slate-500 mt-0.5">
                      {track.eventName}
                    </p>
                  </div>
                  <CoordinatorActionButton
                    icon={icons.Plus}
                    onClick={() => openCreate(track.trackId)}
                  >
                    Add Round
                  </CoordinatorActionButton>
                </div>

                {track.rounds.length === 0 ? (
                  <div className="rounded-xl border border-dashed border-slate-200 py-8 text-center text-sm text-slate-400">
                    Chưa có round nào trong track này.
                  </div>
                ) : (
                  <div className="space-y-3">
                    {track.rounds.map((round, idx) => (
                      <div
                        key={round.roundId}
                        className="grid gap-4 rounded-2xl border border-slate-100 p-4 lg:grid-cols-[auto_1fr_180px_auto]"
                        style={{
                          background:
                            round.status === "Active"
                              ? "rgba(242,111,33,0.02)"
                              : "#fff",
                        }}
                      >
                        {/* Index + status */}
                        <div className="flex items-center gap-3">
                          <div
                            className="flex h-10 w-10 items-center justify-center rounded-xl text-sm font-bold text-white flex-shrink-0"
                            style={{
                              background:
                                round.status === "Active"
                                  ? "#F26F21"
                                  : "#94A3B8",
                            }}
                          >
                            {idx + 1}
                          </div>
                          <CoordinatorBadge tone={statusTone(round.status)}>
                            {round.status}
                          </CoordinatorBadge>
                        </div>

                        {/* Info */}
                        <div>
                          <h4 className="font-bold text-slate-900">
                            {round.name}
                          </h4>
                          <p className="text-xs text-slate-500 mt-0.5">
                            {formatDateTime(round.startTime)} →{" "}
                            {formatDateTime(round.endTime)}
                          </p>
                          <p className="text-xs text-slate-500 mt-0.5">
                            Suất đi tiếp:{" "}
                            <span className="font-bold text-slate-700">
                              {round.advancingSlots}
                            </span>
                          </p>
                        </div>

                        {/* Progress */}
                        <CoordinatorProgressBar
                          label="Progress"
                          value={round.progressPercentage ?? 0}
                          color={
                            round.status === "Active" ? "#F26F21" : "#64748B"
                          }
                        />

                        {/* Actions */}
                        <div className="flex flex-col gap-2 justify-center">
                          <CoordinatorActionButton
                            icon={icons.Edit3}
                            onClick={() => openEdit(round, track.trackId)}
                          >
                            Edit
                          </CoordinatorActionButton>
                          <CoordinatorActionButton
                            icon={icons.SlidersHorizontal}
                            onClick={() => openStatus(round)}
                          >
                            Status
                          </CoordinatorActionButton>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </CoordinatorPanel>

      {/* Modals */}
      {(modal === "create" || modal === "edit") && RoundFormModal}

      {modal === "status" && (
        <ModalShell
          title={`Đổi trạng thái: ${selectedRound?.name}`}
          onClose={closeModal}
          actions={
            <>
              <CoordinatorActionButton onClick={closeModal} disabled={saving}>
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={saving}
                onClick={handleStatusUpdate}
              >
                {saving ? "Đang lưu..." : "Cập nhật"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <p className="text-sm text-slate-500">
              Trạng thái hiện tại:{" "}
              <CoordinatorBadge tone={statusTone(selectedRound?.status)}>
                {selectedRound?.status}
              </CoordinatorBadge>
            </p>
            <select
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              value={statusValue}
              onChange={(e) => setStatusValue(e.target.value)}
            >
              {STATUS_OPTIONS.map((s) => (
                <option key={s}>{s}</option>
              ))}
            </select>
            <FormError msg={formError} />
          </div>
        </ModalShell>
      )}
    </div>
  );
}
