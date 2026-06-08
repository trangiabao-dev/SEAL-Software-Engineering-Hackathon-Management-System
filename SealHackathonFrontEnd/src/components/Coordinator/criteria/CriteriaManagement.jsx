import { useMemo, useState, useEffect, useCallback } from "react";
import {
  CoordinatorActionButton,
  CoordinatorBadge,
  CoordinatorPanel,
  CoordinatorProgressBar,
  CoordinatorTable,
  ModalShell,
  icons,
} from "../CoordinatorUI";
import eventService from "../../../services/eventService";
import trackService from "../../../services/trackService";
import roundService from "../../../services/roundService";
import criterionService from "../../../services/criterionService";
import {
  FormError,
  LoadingState,
  ApiErrorState,
  getApiMessage,
  SetupRequiredBanner,
  validateRoundSelection,
} from "../coordinatorHelpers";

const EMPTY_CRITERION = {
  name: "",
  description: "",
  maxScore: 10,
  weight: 0.1,
};

export function CriteriaManagement() {
  const [events, setEvents] = useState([]);
  const [tracks, setTracks] = useState([]);
  const [rounds, setRounds] = useState([]);
  const [selectedEventId, setSelectedEventId] = useState("");
  const [selectedTrackId, setSelectedTrackId] = useState("");
  const [selectedRoundId, setSelectedRoundId] = useState("");

  const [criteria, setCriteria] = useState([]);
  const [templates, setTemplates] = useState([]);
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState("");

  const [modal, setModal] = useState(null);
  const [form, setForm] = useState(EMPTY_CRITERION);
  const [selectedTemplateId, setSelectedTemplateId] = useState("");
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
    criterionService
      .getTemplates()
      .then((res) => setTemplates(res.data?.data || []))
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

  const fetchCriteria = useCallback(async () => {
    if (!selectedRoundId) {
      setCriteria([]);
      return;
    }
    setLoading(true);
    setApiError("");
    try {
      const res = await criterionService.getByRound(selectedRoundId);
      setCriteria(res.data?.data || []);
    } catch (err) {
      setApiError(getApiMessage(err, "Không thể tải tiêu chí."));
    } finally {
      setLoading(false);
    }
  }, [selectedRoundId]);

  useEffect(() => {
    fetchCriteria();
  }, [fetchCriteria]);

  const totalWeight = useMemo(
    () => criteria.reduce((sum, item) => sum + (item.weight || 0), 0),
    [criteria],
  );
  const valid = totalWeight <= 1.001;

  const roundCheck = validateRoundSelection({
    selectedEventId,
    selectedTrackId,
    selectedRoundId,
    rounds,
    tracks,
    events,
  });

  const columns = [
    { key: "name", label: "Criterion" },
    { key: "maxScore", label: "Max score" },
    { key: "weight", label: "Weight" },
    { key: "description", label: "Description" },
  ];

  const handleAddCriterion = async () => {
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
    if (!form.name.trim()) {
      setFormError("Tên tiêu chí không được để trống.");
      return;
    }
    setSaving(true);
    try {
      await criterionService.create(check.roundId, {
        name: form.name.trim(),
        description: form.description.trim() || null,
        maxScore: Number(form.maxScore),
        weight: Number(form.weight),
      });
      await fetchCriteria();
      setModal(null);
      setForm(EMPTY_CRITERION);
      setFormError("");
    } catch (err) {
      setFormError(getApiMessage(err, "Thêm tiêu chí thất bại."));
    } finally {
      setSaving(false);
    }
  };

  const handleImport = async () => {
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
    if (!selectedTemplateId) {
      setFormError("Chọn template để import.");
      return;
    }
    setSaving(true);
    try {
      await criterionService.importTemplate(
        check.roundId,
        Number(selectedTemplateId),
      );
      await fetchCriteria();
      setModal(null);
      setFormError("");
    } catch (err) {
      setFormError(getApiMessage(err, "Import template thất bại."));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="space-y-6">
      <CoordinatorPanel
        title="Round selector"
        subtitle="Chọn vòng thi để cấu hình rubric"
        icon={icons.Filter}
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
          hint="Thứ tự: Event → Track → Round → Criteria"
        />
      )}

      <CoordinatorPanel
        title="Weight validation"
        subtitle="Total rubric weight must be ≤ 1.0"
        icon={icons.SlidersHorizontal}
        actions={
          <>
            <CoordinatorActionButton
              icon={icons.Upload}
              disabled={!roundCheck.roundId}
              onClick={() => {
                if (!roundCheck.roundId) return;
                setFormError("");
                setModal("import");
              }}
            >
              Import template
            </CoordinatorActionButton>
            <CoordinatorActionButton
              variant="primary"
              icon={icons.Plus}
              disabled={!roundCheck.roundId}
              onClick={() => {
                if (!roundCheck.roundId) return;
                setForm(EMPTY_CRITERION);
                setFormError("");
                setModal("criterion");
              }}
            >
              Add Criterion
            </CoordinatorActionButton>
          </>
        }
      >
        <div
          className={`rounded-xl border p-4 ${valid ? "border-emerald-200 bg-emerald-50" : "border-amber-200 bg-amber-50"}`}
        >
          <div className="mb-3 flex items-center justify-between">
            <p className="font-bold text-slate-900">
              Total weight: {totalWeight.toFixed(2)}
            </p>
            <CoordinatorBadge tone={valid ? "success" : "warning"}>
              {valid ? "Valid" : "Warning"}
            </CoordinatorBadge>
          </div>
          <CoordinatorProgressBar
            value={Math.round(totalWeight * 100)}
            color={valid ? "#059669" : "#D97706"}
          />
        </div>
      </CoordinatorPanel>

      <CoordinatorPanel
        title="Criteria table"
        subtitle="Add and review scoring rubrics (no edit/delete on BE)"
        icon={icons.Scale}
      >
        {loading ? (
          <LoadingState />
        ) : apiError ? (
          <ApiErrorState message={apiError} onRetry={fetchCriteria} />
        ) : !roundCheck.roundId ? null : criteria.length === 0 ? (
          <p className="py-10 text-center text-sm text-slate-400">
            Chưa có tiêu chí cho vòng này.
          </p>
        ) : (
          <CoordinatorTable
            columns={columns}
            rows={criteria}
            renderCell={(row, key) => {
              if (key === "weight")
                return (
                  <span className="font-bold text-slate-900">
                    {Number(row.weight).toFixed(2)}
                  </span>
                );
              return row[key] ?? "—";
            }}
          />
        )}
      </CoordinatorPanel>

      {modal === "criterion" && (
        <ModalShell
          title="Add Criterion"
          onClose={() => setModal(null)}
          actions={
            <>
              <CoordinatorActionButton onClick={() => setModal(null)}>
                Cancel
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={saving}
                onClick={handleAddCriterion}
              >
                {saving ? "Saving..." : "Save"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="grid gap-3">
            <FormError msg={formError} />
            <input
              className="rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              placeholder="Name *"
              value={form.name}
              onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
            />
            <textarea
              className="rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none min-h-16"
              placeholder="Description"
              value={form.description}
              onChange={(e) =>
                setForm((p) => ({ ...p, description: e.target.value }))
              }
            />
            <input
              type="number"
              className="rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              placeholder="Max score"
              value={form.maxScore}
              onChange={(e) =>
                setForm((p) => ({ ...p, maxScore: e.target.value }))
              }
            />
            <input
              type="number"
              step="0.05"
              className="rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              placeholder="Weight, e.g. 0.30"
              value={form.weight}
              onChange={(e) =>
                setForm((p) => ({ ...p, weight: e.target.value }))
              }
            />
          </div>
        </ModalShell>
      )}

      {modal === "import" && (
        <ModalShell
          title="Import criterion template"
          onClose={() => setModal(null)}
          actions={
            <>
              <CoordinatorActionButton onClick={() => setModal(null)}>
                Cancel
              </CoordinatorActionButton>
              <CoordinatorActionButton
                variant="primary"
                disabled={saving}
                onClick={handleImport}
              >
                {saving ? "Importing..." : "Import"}
              </CoordinatorActionButton>
            </>
          }
        >
          <div className="space-y-3">
            <FormError msg={formError} />
            <select
              className="w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm outline-none"
              value={selectedTemplateId}
              onChange={(e) => setSelectedTemplateId(e.target.value)}
            >
              <option value="">Chọn template</option>
              {templates.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name} ({t.items?.length ?? 0} items)
                </option>
              ))}
            </select>
          </div>
        </ModalShell>
      )}
    </div>
  );
}
