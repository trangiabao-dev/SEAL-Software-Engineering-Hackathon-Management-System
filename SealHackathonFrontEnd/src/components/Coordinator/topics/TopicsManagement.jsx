import { useState, useEffect, useCallback } from "react";
import {
  CoordinatorActionButton,
  CoordinatorPanel,
  ModalShell,
  icons,
} from "../CoordinatorUI";
import eventService from "../../../services/eventService";
import trackService from "../../../services/trackService";
import roundService from "../../../services/roundService";
import topicService from "../../../services/topicService";
import {
  FormError,
  LoadingState,
  ApiErrorState,
  getApiMessage,
  SetupRequiredBanner,
  validateRoundSelection,
} from "../coordinatorHelpers";

const EMPTY_FORM = {
  title: "",
  description: "",
  requirements: "",
  attachmentUrl: "",
};

export function TopicsManagement() {
  const [events, setEvents] = useState([]);
  const [tracks, setTracks] = useState([]);
  const [rounds, setRounds] = useState([]);
  const [selectedEventId, setSelectedEventId] = useState("");
  const [selectedTrackId, setSelectedTrackId] = useState("");
  const [selectedRoundId, setSelectedRoundId] = useState("");

  const [topics, setTopics] = useState([]);
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState("");

  const [selected, setSelected] = useState(null);
  const [modal, setModal] = useState(false);
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState("");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    eventService
      .getAll()
      .then((res) => {
        const list = res.data?.data || [];
        setEvents(list);
        if (list.length > 0) setSelectedEventId(String(list[0].id));
      })
      .catch(() => {});
  }, []);

  useEffect(() => {
    if (!selectedEventId) {
      setTracks([]);
      setSelectedTrackId("");
      return;
    }
    setTracks([]);
    setSelectedTrackId("");
    trackService
      .getByEvent(selectedEventId)
      .then((res) => {
        const list = res.data?.data || [];
        setTracks(list);
        setSelectedTrackId(list.length > 0 ? String(list[0].id) : "");
      })
      .catch(() => setTracks([]));
  }, [selectedEventId]);

  useEffect(() => {
    if (!selectedTrackId) {
      setRounds([]);
      setSelectedRoundId("");
      return;
    }
    setRounds([]);
    setSelectedRoundId("");
    roundService
      .getByTrack(selectedTrackId)
      .then((res) => {
        const list = res.data?.data || [];
        setRounds(list);
        setSelectedRoundId(list.length > 0 ? String(list[0].id) : "");
      })
      .catch(() => setRounds([]));
  }, [selectedTrackId]);

  const fetchTopics = useCallback(async () => {
    if (!selectedRoundId) {
      setTopics([]);
      return;
    }
    setLoading(true);
    setApiError("");
    try {
      const res = await topicService.getByRound(selectedRoundId);
      setTopics(res.data?.data || []);
    } catch (err) {
      setApiError(getApiMessage(err, "Không thể tải đề tài."));
    } finally {
      setLoading(false);
    }
  }, [selectedRoundId]);

  useEffect(() => {
    fetchTopics();
  }, [fetchTopics]);

  const roundCheck = validateRoundSelection({
    selectedEventId,
    selectedTrackId,
    selectedRoundId,
    rounds,
    tracks,
    events,
  });

  const handleCreate = async () => {
    const check = validateRoundSelection({
      selectedEventId,
      selectedTrackId,
      selectedRoundId,
      rounds,
      tracks,
      events,
    });
    if (!check.roundId) {
      setFormError(check.error);
      return;
    }
    if (!form.title.trim()) {
      setFormError("Tiêu đề đề tài không được để trống.");
      return;
    }
    setSaving(true);
    try {
      await topicService.create(check.roundId, {
        title: form.title.trim(),
        description: form.description.trim() || null,
        requirements: form.requirements.trim() || null,
        attachmentUrl: form.attachmentUrl.trim() || null,
      });
      await fetchTopics();
      setModal(false);
      setForm(EMPTY_FORM);
      setFormError("");
    } catch (err) {
      setFormError(getApiMessage(err, "Tạo đề tài thất bại."));
    } finally {
      setSaving(false);
    }
  };

  const selectedTrack = tracks.find((t) => String(t.id) === selectedTrackId);
  const selectedRound = rounds.find((r) => String(r.id) === selectedRoundId);

  return (
    <div className="space-y-6">
      <CoordinatorPanel
        title="Topic filters"
        subtitle="Filter by event, track, and round"
        icon={icons.Filter}
        actions={
          <CoordinatorActionButton
            variant="primary"
            icon={icons.Plus}
            disabled={!roundCheck.roundId}
            onClick={() => {
              if (!roundCheck.roundId) return;
              setForm(EMPTY_FORM);
              setFormError("");
              setModal(true);
            }}
          >
            Add Topic
          </CoordinatorActionButton>
        }
      >
        <div className="grid gap-3 md:grid-cols-3">
          <select
            className="rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
            value={selectedEventId}
            onChange={(e) => setSelectedEventId(e.target.value)}
          >
            {events.length === 0 ? (
              <option value="">Chưa có sự kiện</option>
            ) : (
              events.map((ev) => (
                <option key={ev.id} value={ev.id}>
                  {ev.name}
                </option>
              ))
            )}
          </select>
          <select
            className="rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
            value={selectedTrackId}
            onChange={(e) => setSelectedTrackId(e.target.value)}
            disabled={!tracks.length}
          >
            {tracks.length === 0 ? (
              <option value="">Chưa có Track</option>
            ) : (
              tracks.map((tr) => (
                <option key={tr.id} value={tr.id}>
                  {tr.name}
                </option>
              ))
            )}
          </select>
          <select
            className="rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
            value={selectedRoundId}
            onChange={(e) => setSelectedRoundId(e.target.value)}
            disabled={!rounds.length}
          >
            {rounds.length === 0 ? (
              <option value="">Chưa có Round — tạo ở mục Rounds</option>
            ) : (
              rounds.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.name}
                </option>
              ))
            )}
          </select>
        </div>
      </CoordinatorPanel>

      {roundCheck.error && (
        <SetupRequiredBanner
          title={roundCheck.error}
          hint="Thứ tự: Event → Track → Round → Topic"
        />
      )}

      {loading ? (
        <LoadingState />
      ) : apiError ? (
        <ApiErrorState message={apiError} onRetry={fetchTopics} />
      ) : !roundCheck.roundId ? null : topics.length === 0 ? (
        <p className="py-12 text-center text-sm text-slate-400">
          Chưa có đề tài cho vòng này.
        </p>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {topics.map((topic) => (
            <button
              key={topic.id}
              type="button"
              onClick={() => setSelected(topic)}
              className="rounded-2xl border bg-white p-5 text-left transition hover:-translate-y-0.5 hover:shadow-lg"
              style={{ borderColor: "#E5E7EB" }}
            >
              <h3 className="font-bold text-slate-900">{topic.title}</h3>
              <p className="mt-1 text-sm text-slate-500">
                {selectedTrack?.name} • {selectedRound?.name}
              </p>
              {topic.description && (
                <p className="mt-2 text-xs text-slate-500 line-clamp-2">
                  {topic.description}
                </p>
              )}
            </button>
          ))}
        </div>
      )}

      {selected && (
        <ModalShell
          title="Topic details"
          onClose={() => setSelected(null)}
          actions={
            <CoordinatorActionButton
              variant="primary"
              onClick={() => setSelected(null)}
            >
              Done
            </CoordinatorActionButton>
          }
        >
          <div className="space-y-3 text-sm text-slate-600">
            <p>
              <span className="font-bold text-slate-900">Title:</span>{" "}
              {selected.title}
            </p>
            <p>
              <span className="font-bold text-slate-900">Description:</span>{" "}
              {selected.description || "—"}
            </p>
            <p>
              <span className="font-bold text-slate-900">Requirements:</span>{" "}
              {selected.requirements || "—"}
            </p>
            {selected.attachmentUrl && (
              <p>
                <span className="font-bold text-slate-900">Attachment:</span>{" "}
                <a
                  href={selected.attachmentUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-[#F26F21] hover:underline"
                >
                  {selected.attachmentUrl}
                </a>
              </p>
            )}
          </div>
        </ModalShell>
      )}

      {modal && (
        <ModalShell
          title="Add Topic"
          onClose={() => setModal(false)}
          actions={
            <>
              <CoordinatorActionButton onClick={() => setModal(false)}>
                Cancel
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={saving}
                onClick={handleCreate}
              >
                {saving ? "Saving..." : "Save"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <FormError msg={formError} />
            <input
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              placeholder="Title *"
              value={form.title}
              onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))}
            />
            <textarea
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none min-h-20"
              placeholder="Description"
              value={form.description}
              onChange={(e) =>
                setForm((p) => ({ ...p, description: e.target.value }))
              }
            />
            <textarea
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none min-h-20"
              placeholder="Requirements"
              value={form.requirements}
              onChange={(e) =>
                setForm((p) => ({ ...p, requirements: e.target.value }))
              }
            />
            <input
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              placeholder="Attachment URL"
              value={form.attachmentUrl}
              onChange={(e) =>
                setForm((p) => ({ ...p, attachmentUrl: e.target.value }))
              }
            />
          </div>
        </ModalShell>
      )}
    </div>
  );
}
