import { useState, useEffect, useCallback } from "react";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorProgressBar,
  ModalShell,
  icons,
} from "./CoordinatorUI";
import { AlertCircle, Loader2, ChevronDown, ChevronRight } from "lucide-react";
import axiosInstance from "../../services/axiosInstance";

// ---------------------------------------------------------------------------
// Constants & helpers (preserved from originals)
// ---------------------------------------------------------------------------
const EVENT_STATUS_OPTIONS = ["Draft", "Open", "Ongoing", "Closed"];
const ROUND_STATUS_OPTIONS = ["Upcoming", "Active", "Scoring", "Completed"];

const eventStatusTone = (s) =>
  s === "Ongoing"
    ? "success"
    : s === "Open"
      ? "orange"
      : s === "Draft"
        ? "warning"
        : "neutral";

const roundStatusTone = (s) =>
  s === "Active"
    ? "orange"
    : s === "Scoring"
      ? "purple"
      : s === "Completed"
        ? "success"
        : "neutral";

const EVENT_EMPTY = { name: "", description: "", startDate: "", endDate: "", status: "Draft" };
const TRACK_EMPTY = { name: "", description: "", maxTeams: "", eventId: "" };
const ROUND_EMPTY = {
  trackId: "",
  name: "",
  orderIndex: "",
  startTime: "",
  endTime: "",
  advancingSlots: "",
};

function formatDate(iso) {
  if (!iso) return "—";
  return iso.split("T")[0];
}

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

// ---------------------------------------------------------------------------
// CompetitionSetup — unified 3-level accordion: Event → Track → Round
// ---------------------------------------------------------------------------
export function CompetitionSetup() {
  // === EVENTS ===
  const [events, setEvents] = useState([]);
  const [eventsLoading, setEventsLoading] = useState(true);
  const [eventsError, setEventsError] = useState("");

  // === ACCORDION ===
  const [expandedEvents, setExpandedEvents] = useState(new Set());
  const [expandedTracks, setExpandedTracks] = useState(new Set());

  // === TRACKS (per event, lazy) ===
  const [tracksByEvent, setTracksByEvent] = useState({});
  // shape: { [eventId]: { data: [], loading: bool, error: "" } }

  // === ROUNDS (per track, lazy) ===
  const [roundsByTrack, setRoundsByTrack] = useState({});
  // shape: { [trackId]: { data: [], loading: bool, error: "" } }

  // === EVENT MODAL ===
  const [eventModal, setEventModal] = useState(null);
  const [selectedEvent, setSelectedEvent] = useState(null);
  const [eventForm, setEventForm] = useState(EVENT_EMPTY);
  const [eventFormError, setEventFormError] = useState("");
  const [eventSaving, setEventSaving] = useState(false);

  // === TRACK MODAL ===
  const [trackModal, setTrackModal] = useState(null);
  const [selectedTrack, setSelectedTrack] = useState(null);
  const [trackForm, setTrackForm] = useState(TRACK_EMPTY);
  const [trackFormError, setTrackFormError] = useState("");
  const [trackSaving, setTrackSaving] = useState(false);

  // === ROUND MODAL ===
  const [roundModal, setRoundModal] = useState(null);
  const [selectedRound, setSelectedRound] = useState(null);
  const [roundForm, setRoundForm] = useState(ROUND_EMPTY);
  const [roundFormError, setRoundFormError] = useState("");
  const [roundSaving, setRoundSaving] = useState(false);
  const [roundStatusValue, setRoundStatusValue] = useState("");

  // =========================================================================
  // FETCH — same API endpoints as originals
  // =========================================================================
  const fetchEvents = useCallback(async () => {
    setEventsLoading(true);
    setEventsError("");
    try {
      const res = await axiosInstance.get("/api/events");
      setEvents(res.data?.data || []);
    } catch (err) {
      setEventsError(
        err?.response?.data?.message || "Không thể tải danh sách sự kiện.",
      );
    } finally {
      setEventsLoading(false);
    }
  }, []);

  const fetchTracksForEvent = useCallback(async (eventId) => {
    setTracksByEvent((prev) => ({
      ...prev,
      [eventId]: { data: [], loading: true, error: "" },
    }));
    try {
      const res = await axiosInstance.get(`/api/events/${eventId}/tracks`);
      setTracksByEvent((prev) => ({
        ...prev,
        [eventId]: { data: res.data?.data || [], loading: false, error: "" },
      }));
    } catch (err) {
      setTracksByEvent((prev) => ({
        ...prev,
        [eventId]: {
          data: [],
          loading: false,
          error: err?.response?.data?.message || "Không thể tải danh sách track.",
        },
      }));
    }
  }, []);

  const fetchRoundsForTrack = useCallback(async (trackId) => {
    setRoundsByTrack((prev) => ({
      ...prev,
      [trackId]: { data: [], loading: true, error: "" },
    }));
    try {
      const res = await axiosInstance.get("/api/tracks/rounds");
      const allTracks = res.data?.data || [];
      const match = allTracks.find(
        (t) => String(t.trackId) === String(trackId),
      );
      setRoundsByTrack((prev) => ({
        ...prev,
        [trackId]: { data: match?.rounds || [], loading: false, error: "" },
      }));
    } catch (err) {
      setRoundsByTrack((prev) => ({
        ...prev,
        [trackId]: {
          data: [],
          loading: false,
          error:
            err?.response?.data?.message || "Không thể tải danh sách rounds.",
        },
      }));
    }
  }, []);

  useEffect(() => {
    fetchEvents();
  }, [fetchEvents]);

  // =========================================================================
  // ACCORDION TOGGLE — lazy-loads children on first expand
  // =========================================================================
  const toggleEvent = (eventId) => {
    setExpandedEvents((prev) => {
      const next = new Set(prev);
      if (next.has(eventId)) {
        next.delete(eventId);
      } else {
        next.add(eventId);
        if (!tracksByEvent[eventId]) fetchTracksForEvent(eventId);
      }
      return next;
    });
  };

  const toggleTrack = (trackId) => {
    setExpandedTracks((prev) => {
      const next = new Set(prev);
      if (next.has(trackId)) {
        next.delete(trackId);
      } else {
        next.add(trackId);
        if (!roundsByTrack[trackId]) fetchRoundsForTrack(trackId);
      }
      return next;
    });
  };

  // =========================================================================
  // EVENT HANDLERS (preserved from EventsManagement)
  // =========================================================================
  const openCreateEvent = () => {
    setEventForm(EVENT_EMPTY);
    setEventFormError("");
    setEventModal("create");
  };
  const openEditEvent = (ev) => {
    setSelectedEvent(ev);
    setEventForm({
      name: ev.name,
      description: ev.description || "",
      startDate: formatDate(ev.startDate),
      endDate: formatDate(ev.endDate),
      status: ev.status,
    });
    setEventFormError("");
    setEventModal("edit");
  };
  const openDeleteEvent = (ev) => {
    setSelectedEvent(ev);
    setEventFormError("");
    setEventModal("delete");
  };
  const closeEventModal = () => {
    setEventModal(null);
    setSelectedEvent(null);
    setEventFormError("");
  };

  const handleEventFormChange = (field, value) => {
    setEventForm((p) => ({ ...p, [field]: value }));
    setEventFormError("");
  };

  const validateEventForm = () => {
    if (!eventForm.name.trim()) return "Tên sự kiện không được để trống.";
    if (!eventForm.startDate) return "Vui lòng chọn ngày bắt đầu.";
    if (!eventForm.endDate) return "Vui lòng chọn ngày kết thúc.";
    if (eventForm.endDate < eventForm.startDate)
      return "Ngày kết thúc phải sau ngày bắt đầu.";
    return "";
  };

  // CREATE event
  const handleCreateEvent = async () => {
    const err = validateEventForm();
    if (err) {
      setEventFormError(err);
      return;
    }
    setEventSaving(true);
    try {
      await axiosInstance.post("/api/events", {
        name: eventForm.name.trim(),
        description: eventForm.description.trim(),
        startDate: eventForm.startDate,
        endDate: eventForm.endDate,
        status: eventForm.status,
      });
      await fetchEvents();
      closeEventModal();
    } catch (err) {
      setEventFormError(
        err?.response?.data?.message || "Tạo sự kiện thất bại.",
      );
    } finally {
      setEventSaving(false);
    }
  };

  // EDIT event
  const handleEditEvent = async () => {
    const err = validateEventForm();
    if (err) {
      setEventFormError(err);
      return;
    }
    setEventSaving(true);
    try {
      await axiosInstance.put(`/api/events/${selectedEvent.id}`, {
        name: eventForm.name.trim(),
        description: eventForm.description.trim(),
        startDate: eventForm.startDate,
        endDate: eventForm.endDate,
        status: eventForm.status,
      });
      await fetchEvents();
      closeEventModal();
    } catch (err) {
      setEventFormError(
        err?.response?.data?.message || "Cập nhật thất bại.",
      );
    } finally {
      setEventSaving(false);
    }
  };

  // DELETE event
  const handleDeleteEvent = async () => {
    setEventSaving(true);
    try {
      await axiosInstance.delete(`/api/events/${selectedEvent.id}`);
      await fetchEvents();
      closeEventModal();
    } catch (err) {
      setEventFormError(
        err?.response?.data?.message || "Xóa thất bại.",
      );
    } finally {
      setEventSaving(false);
    }
  };

  // =========================================================================
  // TRACK HANDLERS (preserved from TracksManagement)
  // =========================================================================
  const openCreateTrack = (eventId) => {
    setTrackForm({ ...TRACK_EMPTY, eventId: String(eventId) });
    setTrackFormError("");
    setTrackModal("create");
  };
  const openEditTrack = (track) => {
    setSelectedTrack(track);
    setTrackForm({
      name: track.name,
      description: track.description || "",
      maxTeams: String(track.maxTeams),
      eventId: String(track.eventId),
    });
    setTrackFormError("");
    setTrackModal("edit");
  };
  const closeTrackModal = () => {
    setTrackModal(null);
    setSelectedTrack(null);
    setTrackFormError("");
  };

  const handleTrackFormChange = (field, value) => {
    setTrackForm((p) => ({ ...p, [field]: value }));
    setTrackFormError("");
  };

  const validateTrackForm = () => {
    if (!trackForm.name.trim()) return "Tên track không được để trống.";
    if (
      !trackForm.maxTeams ||
      isNaN(trackForm.maxTeams) ||
      Number(trackForm.maxTeams) < 1
    )
      return "Số đội tối đa phải là số nguyên dương.";
    if (!trackForm.eventId) return "Vui lòng chọn sự kiện.";
    return "";
  };

  // CREATE track
  const handleCreateTrack = async () => {
    const err = validateTrackForm();
    if (err) {
      setTrackFormError(err);
      return;
    }
    setTrackSaving(true);
    try {
      await axiosInstance.post("/tracks", {
        name: trackForm.name.trim(),
        description: trackForm.description.trim(),
        maxTeams: Number(trackForm.maxTeams),
        eventId: Number(trackForm.eventId),
      });
      await fetchTracksForEvent(trackForm.eventId);
      closeTrackModal();
    } catch (err) {
      setTrackFormError(
        err?.response?.data?.message || "Tạo track thất bại.",
      );
    } finally {
      setTrackSaving(false);
    }
  };

  // EDIT track
  const handleEditTrack = async () => {
    const err = validateTrackForm();
    if (err) {
      setTrackFormError(err);
      return;
    }
    setTrackSaving(true);
    try {
      await axiosInstance.put(`/tracks/${selectedTrack.id}`, {
        name: trackForm.name.trim(),
        description: trackForm.description.trim(),
        maxTeams: Number(trackForm.maxTeams),
        eventId: Number(trackForm.eventId),
      });
      await fetchTracksForEvent(trackForm.eventId);
      closeTrackModal();
    } catch (err) {
      setTrackFormError(
        err?.response?.data?.message || "Cập nhật track thất bại.",
      );
    } finally {
      setTrackSaving(false);
    }
  };

  // =========================================================================
  // ROUND HANDLERS (preserved from RoundsManagement)
  // =========================================================================
  const openCreateRound = (trackId) => {
    setRoundForm({ ...ROUND_EMPTY, trackId: String(trackId) });
    setRoundFormError("");
    setRoundModal("create");
  };
  const openEditRound = (round, trackId) => {
    setSelectedRound({ ...round, trackId });
    setRoundForm({
      trackId: String(trackId),
      name: round.name,
      orderIndex: String(round.orderIndex ?? ""),
      startTime: round.startTime?.slice(0, 16) || "",
      endTime: round.endTime?.slice(0, 16) || "",
      advancingSlots: String(round.advancingSlots),
    });
    setRoundFormError("");
    setRoundModal("edit");
  };
  const openRoundStatus = (round, trackId) => {
    setSelectedRound({ ...round, trackId });
    setRoundStatusValue(round.status);
    setRoundFormError("");
    setRoundModal("status");
  };
  const closeRoundModal = () => {
    setRoundModal(null);
    setSelectedRound(null);
    setRoundFormError("");
  };

  const handleRoundFormChange = (field, value) => {
    setRoundForm((p) => ({ ...p, [field]: value }));
    setRoundFormError("");
  };

  const validateRoundForm = () => {
    if (!roundForm.name.trim()) return "Tên vòng không được để trống.";
    if (!roundForm.trackId) return "Vui lòng chọn track.";
    if (!roundForm.startTime) return "Vui lòng chọn thời gian bắt đầu.";
    if (!roundForm.endTime) return "Vui lòng chọn thời gian kết thúc.";
    if (roundForm.endTime <= roundForm.startTime)
      return "Thời gian kết thúc phải sau bắt đầu.";
    if (!roundForm.advancingSlots || Number(roundForm.advancingSlots) < 1)
      return "Số suất đi tiếp phải lớn hơn 0.";
    return "";
  };

  // CREATE round
  const handleCreateRound = async () => {
    const err = validateRoundForm();
    if (err) {
      setRoundFormError(err);
      return;
    }
    setRoundSaving(true);
    try {
      await axiosInstance.post("/api/rounds", {
        trackId: Number(roundForm.trackId),
        name: roundForm.name.trim(),
        orderIndex: Number(roundForm.orderIndex) || 1,
        startTime: new Date(roundForm.startTime).toISOString(),
        endTime: new Date(roundForm.endTime).toISOString(),
        advancingSlots: Number(roundForm.advancingSlots),
      });
      await fetchRoundsForTrack(roundForm.trackId);
      closeRoundModal();
    } catch (err) {
      setRoundFormError(
        err?.response?.data?.message || "Tạo round thất bại.",
      );
    } finally {
      setRoundSaving(false);
    }
  };

  // EDIT round
  const handleEditRound = async () => {
    const err = validateRoundForm();
    if (err) {
      setRoundFormError(err);
      return;
    }
    setRoundSaving(true);
    try {
      await axiosInstance.put(`/api/rounds/${selectedRound.roundId}`, {
        name: roundForm.name.trim(),
        orderIndex: Number(roundForm.orderIndex) || 1,
        startTime: new Date(roundForm.startTime).toISOString(),
        endTime: new Date(roundForm.endTime).toISOString(),
        advancingSlots: Number(roundForm.advancingSlots),
      });
      await fetchRoundsForTrack(selectedRound.trackId);
      closeRoundModal();
    } catch (err) {
      setRoundFormError(
        err?.response?.data?.message || "Cập nhật round thất bại.",
      );
    } finally {
      setRoundSaving(false);
    }
  };

  // STATUS update round
  const handleRoundStatusUpdate = async () => {
    if (!roundStatusValue) return;
    setRoundSaving(true);
    try {
      await axiosInstance.put(`/api/rounds/${selectedRound.roundId}/status`, {
        status: roundStatusValue,
      });
      await fetchRoundsForTrack(selectedRound.trackId);
      closeRoundModal();
    } catch (err) {
      setRoundFormError(
        err?.response?.data?.message || "Cập nhật trạng thái thất bại.",
      );
    } finally {
      setRoundSaving(false);
    }
  };

  // =========================================================================
  // RENDER
  // =========================================================================
  return (
    <div className="space-y-4">
      {/* ─── Header panel ─── */}
      <CoordinatorPanel
        title="Competition Setup"
        subtitle="Manage events, tracks, and rounds in a unified tree view"
        icon={icons.CalendarDays}
        actions={
          <CoordinatorActionButton
            variant="primary"
            icon={icons.Plus}
            onClick={openCreateEvent}
          >
            Create Event
          </CoordinatorActionButton>
        }
      />

      {/* ─── Event list ─── */}
      {eventsLoading ? (
        <div className="flex items-center justify-center py-12 gap-2 text-sm text-slate-400">
          <Loader2
            className="w-4 h-4 animate-spin"
            style={{ color: "#F26F21" }}
          />
          Đang tải sự kiện...
        </div>
      ) : eventsError ? (
        <div className="flex flex-col items-center py-10 gap-3">
          <p className="text-sm text-red-500">{eventsError}</p>
          <CoordinatorActionButton onClick={fetchEvents}>
            Thử lại
          </CoordinatorActionButton>
        </div>
      ) : events.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-slate-200 py-14 text-center text-sm text-slate-400">
          Chưa có sự kiện nào. Nhấn &quot;Create Event&quot; để bắt đầu.
        </div>
      ) : (
        <div className="space-y-3">
          {events.map((event) => {
            const isExpanded = expandedEvents.has(event.id);
            const tracksState = tracksByEvent[event.id];

            return (
              <div
                key={event.id}
                className="rounded-2xl border bg-white transition-all duration-300 hover:shadow-md"
                style={{
                  borderColor: isExpanded ? "rgba(242,111,33,0.3)" : "#E5E7EB",
                  boxShadow: "0 10px 30px rgba(0,0,0,0.02)",
                }}
              >
                {/* ── Event header row ── */}
                <div
                  className="flex items-center gap-3 p-4 cursor-pointer select-none"
                  onClick={() => toggleEvent(event.id)}
                >
                  <button
                    type="button"
                    className="flex-shrink-0 rounded-lg p-1 hover:bg-slate-100 transition-colors"
                  >
                    {isExpanded ? (
                      <ChevronDown className="w-4 h-4 text-[#F26F21]" />
                    ) : (
                      <ChevronRight className="w-4 h-4 text-slate-400" />
                    )}
                  </button>

                  <div
                    className="flex h-9 w-9 items-center justify-center rounded-lg flex-shrink-0"
                    style={{
                      background: "rgba(242,111,33,0.1)",
                      border: "1px solid rgba(242,111,33,0.2)",
                    }}
                  >
                    <icons.CalendarDays
                      className="h-4 w-4"
                      style={{ color: "#F26F21" }}
                    />
                  </div>

                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <h3 className="font-bold text-slate-900 truncate">
                        {event.name}
                      </h3>
                      <CoordinatorBadge tone={eventStatusTone(event.status)}>
                        {event.status}
                      </CoordinatorBadge>
                    </div>
                    <p className="text-xs text-slate-500 mt-0.5 truncate">
                      {formatDate(event.startDate)} →{" "}
                      {formatDate(event.endDate)}
                      {event.description && ` • ${event.description}`}
                    </p>
                  </div>

                  <div
                    className="flex items-center gap-2 flex-shrink-0"
                    onClick={(e) => e.stopPropagation()}
                  >
                    <CoordinatorActionButton
                      icon={icons.Edit3}
                      onClick={() => openEditEvent(event)}
                    >
                      Edit
                    </CoordinatorActionButton>
                    <CoordinatorActionButton
                      variant="danger"
                      icon={icons.Trash2}
                      onClick={() => openDeleteEvent(event)}
                    >
                      Delete
                    </CoordinatorActionButton>
                    <CoordinatorActionButton
                      icon={icons.Plus}
                      onClick={() => openCreateTrack(event.id)}
                    >
                      Add Track
                    </CoordinatorActionButton>
                  </div>
                </div>

                {/* ── Expanded: Tracks inside event ── */}
                {isExpanded && (
                  <div
                    className="border-t px-4 pb-4 pt-3 animate-fade-in"
                    style={{
                      borderColor: "#F0F0F0",
                      background: "rgba(249,250,251,0.5)",
                    }}
                  >
                    {!tracksState || tracksState.loading ? (
                      <div className="flex items-center justify-center py-8 gap-2 text-sm text-slate-400">
                        <Loader2
                          className="w-4 h-4 animate-spin"
                          style={{ color: "#F26F21" }}
                        />
                        Đang tải track...
                      </div>
                    ) : tracksState.error ? (
                      <div className="flex flex-col items-center py-6 gap-3">
                        <p className="text-sm text-red-500">
                          {tracksState.error}
                        </p>
                        <CoordinatorActionButton
                          onClick={() => fetchTracksForEvent(event.id)}
                        >
                          Thử lại
                        </CoordinatorActionButton>
                      </div>
                    ) : tracksState.data.length === 0 ? (
                      <div className="rounded-xl border border-dashed border-slate-200 py-8 text-center text-sm text-slate-400">
                        Chưa có track nào trong sự kiện này.
                      </div>
                    ) : (
                      <div className="space-y-2">
                        {tracksState.data.map((track) => {
                          const isTrackExpanded = expandedTracks.has(track.id);
                          const roundsState = roundsByTrack[track.id];
                          const teamPercent = track.maxTeams
                            ? Math.round(
                                ((track.currentTeams ?? 0) / track.maxTeams) *
                                  100,
                              )
                            : 0;

                          return (
                            <div
                              key={track.id}
                              className="rounded-xl border bg-white transition-all duration-200"
                              style={{
                                borderColor: isTrackExpanded
                                  ? "rgba(242,111,33,0.2)"
                                  : "#E5E7EB",
                              }}
                            >
                              {/* ── Track header row ── */}
                              <div
                                className="flex items-center gap-3 p-3 cursor-pointer select-none"
                                onClick={() => toggleTrack(track.id)}
                              >
                                <button
                                  type="button"
                                  className="flex-shrink-0 rounded p-0.5 hover:bg-slate-100 transition-colors"
                                >
                                  {isTrackExpanded ? (
                                    <ChevronDown className="w-3.5 h-3.5 text-[#F26F21]" />
                                  ) : (
                                    <ChevronRight className="w-3.5 h-3.5 text-slate-400" />
                                  )}
                                </button>

                                <icons.GitBranch
                                  className="w-4 h-4 flex-shrink-0"
                                  style={{ color: "#F26F21" }}
                                />

                                <div className="flex-1 min-w-0">
                                  <div className="flex items-center gap-2 flex-wrap">
                                    <p className="font-bold text-slate-800 text-sm truncate">
                                      {track.name}
                                    </p>
                                    <CoordinatorBadge tone="info">
                                      #{track.id}
                                    </CoordinatorBadge>
                                  </div>
                                  <p className="text-xs text-slate-500 mt-0.5">
                                    {track.description || "—"} • Max teams:{" "}
                                    {track.maxTeams}
                                  </p>
                                </div>

                                <div className="w-32 flex-shrink-0 hidden sm:block">
                                  <CoordinatorProgressBar
                                    label={`${track.currentTeams ?? 0}/${track.maxTeams}`}
                                    value={teamPercent}
                                  />
                                </div>

                                <div
                                  className="flex items-center gap-2 flex-shrink-0"
                                  onClick={(e) => e.stopPropagation()}
                                >
                                  <CoordinatorActionButton
                                    icon={icons.Edit3}
                                    onClick={() => openEditTrack(track)}
                                  >
                                    Edit
                                  </CoordinatorActionButton>
                                  <CoordinatorActionButton
                                    icon={icons.Plus}
                                    onClick={() => openCreateRound(track.id)}
                                  >
                                    Add Round
                                  </CoordinatorActionButton>
                                </div>
                              </div>

                              {/* ── Expanded: Rounds inside track ── */}
                              {isTrackExpanded && (
                                <div
                                  className="border-t px-3 pb-3 pt-2 animate-fade-in"
                                  style={{ borderColor: "#F0F0F0" }}
                                >
                                  {!roundsState || roundsState.loading ? (
                                    <div className="flex items-center justify-center py-6 gap-2 text-sm text-slate-400">
                                      <Loader2
                                        className="w-4 h-4 animate-spin"
                                        style={{ color: "#F26F21" }}
                                      />
                                      Đang tải rounds...
                                    </div>
                                  ) : roundsState.error ? (
                                    <div className="flex flex-col items-center py-4 gap-3">
                                      <p className="text-sm text-red-500">
                                        {roundsState.error}
                                      </p>
                                      <CoordinatorActionButton
                                        onClick={() =>
                                          fetchRoundsForTrack(track.id)
                                        }
                                      >
                                        Thử lại
                                      </CoordinatorActionButton>
                                    </div>
                                  ) : roundsState.data.length === 0 ? (
                                    <div className="rounded-lg border border-dashed border-slate-200 py-6 text-center text-sm text-slate-400">
                                      Chưa có round nào trong track này.
                                    </div>
                                  ) : (
                                    <div className="space-y-2">
                                      {roundsState.data.map((round, idx) => (
                                        <div
                                          key={round.roundId}
                                          className="grid gap-3 rounded-xl border border-slate-100 p-3 lg:grid-cols-[auto_1fr_160px_auto]"
                                          style={{
                                            background:
                                              round.status === "Active"
                                                ? "rgba(242,111,33,0.02)"
                                                : "#fff",
                                          }}
                                        >
                                          {/* Index + status */}
                                          <div className="flex items-center gap-2">
                                            <div
                                              className="flex h-8 w-8 items-center justify-center rounded-lg text-xs font-bold text-white flex-shrink-0"
                                              style={{
                                                background:
                                                  round.status === "Active"
                                                    ? "#F26F21"
                                                    : "#94A3B8",
                                              }}
                                            >
                                              {idx + 1}
                                            </div>
                                            <CoordinatorBadge
                                              tone={roundStatusTone(
                                                round.status,
                                              )}
                                            >
                                              {round.status}
                                            </CoordinatorBadge>
                                          </div>

                                          {/* Info */}
                                          <div>
                                            <h4 className="font-bold text-slate-900 text-sm">
                                              {round.name}
                                            </h4>
                                            <p className="text-xs text-slate-500 mt-0.5">
                                              {formatDateTime(
                                                round.startTime,
                                              )}{" "}
                                              →{" "}
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
                                            value={
                                              round.progressPercentage ?? 0
                                            }
                                            color={
                                              round.status === "Active"
                                                ? "#F26F21"
                                                : "#64748B"
                                            }
                                          />

                                          {/* Actions */}
                                          <div className="flex flex-col gap-1.5 justify-center">
                                            <CoordinatorActionButton
                                              icon={icons.Edit3}
                                              onClick={() =>
                                                openEditRound(
                                                  round,
                                                  track.id,
                                                )
                                              }
                                            >
                                              Edit
                                            </CoordinatorActionButton>
                                            <CoordinatorActionButton
                                              icon={icons.SlidersHorizontal}
                                              onClick={() =>
                                                openRoundStatus(
                                                  round,
                                                  track.id,
                                                )
                                              }
                                            >
                                              Status
                                            </CoordinatorActionButton>
                                          </div>
                                        </div>
                                      ))}
                                    </div>
                                  )}
                                </div>
                              )}
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* =============================================================== */}
      {/* EVENT MODALS (preserved from EventsManagement)                  */}
      {/* =============================================================== */}
      {(eventModal === "create" || eventModal === "edit") && (
        <ModalShell
          title={
            eventModal === "create"
              ? "Tạo sự kiện mới"
              : `Chỉnh sửa: ${selectedEvent?.name}`
          }
          onClose={closeEventModal}
          actions={
            <>
              <CoordinatorActionButton
                onClick={closeEventModal}
                disabled={eventSaving}
              >
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={eventSaving}
                onClick={
                  eventModal === "create" ? handleCreateEvent : handleEditEvent
                }
              >
                {eventSaving
                  ? "Đang lưu..."
                  : eventModal === "create"
                    ? "Tạo sự kiện"
                    : "Lưu thay đổi"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <FormError msg={eventFormError} />
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Tên sự kiện <span className="text-orange-500">*</span>
              </label>
              <input
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                placeholder="VD: FPT Hackathon 2026"
                value={eventForm.name}
                onChange={(e) => handleEventFormChange("name", e.target.value)}
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Mô tả
              </label>
              <textarea
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none min-h-20"
                placeholder="Mô tả sự kiện"
                value={eventForm.description}
                onChange={(e) =>
                  handleEventFormChange("description", e.target.value)
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
                  value={eventForm.startDate}
                  onChange={(e) =>
                    handleEventFormChange("startDate", e.target.value)
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
                  value={eventForm.endDate}
                  onChange={(e) =>
                    handleEventFormChange("endDate", e.target.value)
                  }
                />
              </div>
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Trạng thái
              </label>
              <select
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                value={eventForm.status}
                onChange={(e) =>
                  handleEventFormChange("status", e.target.value)
                }
              >
                {EVENT_STATUS_OPTIONS.map((s) => (
                  <option key={s}>{s}</option>
                ))}
              </select>
            </div>
          </div>
        </ModalShell>
      )}

      {eventModal === "delete" && (
        <ModalShell
          title={`Xóa sự kiện: ${selectedEvent?.name}?`}
          onClose={closeEventModal}
          actions={
            <>
              <CoordinatorActionButton
                onClick={closeEventModal}
                disabled={eventSaving}
              >
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="danger"
                disabled={eventSaving}
                onClick={handleDeleteEvent}
              >
                {eventSaving ? "Đang xóa..." : "Xác nhận xóa"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <p className="text-sm text-slate-600">
              Sự kiện sẽ bị ẩn khỏi hệ thống nhưng dữ liệu vẫn được giữ lại
              (soft delete).
            </p>
            <FormError msg={eventFormError} />
          </div>
        </ModalShell>
      )}

      {/* =============================================================== */}
      {/* TRACK MODALS (preserved from TracksManagement)                  */}
      {/* =============================================================== */}
      {(trackModal === "create" || trackModal === "edit") && (
        <ModalShell
          title={
            trackModal === "create"
              ? "Tạo Track mới"
              : `Chỉnh sửa: ${selectedTrack?.name}`
          }
          onClose={closeTrackModal}
          actions={
            <>
              <CoordinatorActionButton
                onClick={closeTrackModal}
                disabled={trackSaving}
              >
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={trackSaving}
                onClick={
                  trackModal === "create" ? handleCreateTrack : handleEditTrack
                }
              >
                {trackSaving
                  ? "Đang lưu..."
                  : trackModal === "create"
                    ? "Tạo Track"
                    : "Lưu thay đổi"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <FormError msg={trackFormError} />
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Sự kiện <span className="text-orange-500">*</span>
              </label>
              <select
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                value={trackForm.eventId}
                onChange={(e) =>
                  handleTrackFormChange("eventId", e.target.value)
                }
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
                value={trackForm.name}
                onChange={(e) => handleTrackFormChange("name", e.target.value)}
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Mô tả
              </label>
              <textarea
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none min-h-16"
                placeholder="Mô tả track"
                value={trackForm.description}
                onChange={(e) =>
                  handleTrackFormChange("description", e.target.value)
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
                value={trackForm.maxTeams}
                onChange={(e) =>
                  handleTrackFormChange("maxTeams", e.target.value)
                }
              />
            </div>
          </div>
        </ModalShell>
      )}

      {/* =============================================================== */}
      {/* ROUND MODALS (preserved from RoundsManagement)                  */}
      {/* =============================================================== */}
      {(roundModal === "create" || roundModal === "edit") && (
        <ModalShell
          title={
            roundModal === "create"
              ? "Tạo Round mới"
              : `Chỉnh sửa: ${selectedRound?.name}`
          }
          onClose={closeRoundModal}
          actions={
            <>
              <CoordinatorActionButton
                onClick={closeRoundModal}
                disabled={roundSaving}
              >
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={roundSaving}
                onClick={
                  roundModal === "create" ? handleCreateRound : handleEditRound
                }
              >
                {roundSaving
                  ? "Đang lưu..."
                  : roundModal === "create"
                    ? "Tạo Round"
                    : "Lưu thay đổi"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <FormError msg={roundFormError} />
            <div>
              <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                Tên Round <span className="text-orange-500">*</span>
              </label>
              <input
                className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                placeholder="VD: Vòng Sơ Loại"
                value={roundForm.name}
                onChange={(e) => handleRoundFormChange("name", e.target.value)}
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
                  value={roundForm.orderIndex}
                  onChange={(e) =>
                    handleRoundFormChange("orderIndex", e.target.value)
                  }
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
                  value={roundForm.advancingSlots}
                  onChange={(e) =>
                    handleRoundFormChange("advancingSlots", e.target.value)
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
                  value={roundForm.startTime}
                  onChange={(e) =>
                    handleRoundFormChange("startTime", e.target.value)
                  }
                />
              </div>
              <div>
                <label className="block text-xs font-semibold text-slate-600 mb-1 uppercase tracking-wider">
                  Kết thúc <span className="text-orange-500">*</span>
                </label>
                <input
                  type="datetime-local"
                  className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
                  value={roundForm.endTime}
                  onChange={(e) =>
                    handleRoundFormChange("endTime", e.target.value)
                  }
                />
              </div>
            </div>
          </div>
        </ModalShell>
      )}

      {roundModal === "status" && (
        <ModalShell
          title={`Đổi trạng thái: ${selectedRound?.name}`}
          onClose={closeRoundModal}
          actions={
            <>
              <CoordinatorActionButton
                onClick={closeRoundModal}
                disabled={roundSaving}
              >
                Huỷ
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={roundSaving}
                onClick={handleRoundStatusUpdate}
              >
                {roundSaving ? "Đang lưu..." : "Cập nhật"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <p className="text-sm text-slate-500">
              Trạng thái hiện tại:{" "}
              <CoordinatorBadge tone={roundStatusTone(selectedRound?.status)}>
                {selectedRound?.status}
              </CoordinatorBadge>
            </p>
            <select
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              value={roundStatusValue}
              onChange={(e) => setRoundStatusValue(e.target.value)}
            >
              {ROUND_STATUS_OPTIONS.map((s) => (
                <option key={s}>{s}</option>
              ))}
            </select>
            <FormError msg={roundFormError} />
          </div>
        </ModalShell>
      )}
    </div>
  );
}
