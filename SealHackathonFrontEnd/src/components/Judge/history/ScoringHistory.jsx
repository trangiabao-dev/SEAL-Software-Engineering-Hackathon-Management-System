import { HistoryPanel } from "./HistoryTable";

export function ScoringHistory({ submissions }) {
  const records = submissions.filter((item) => item.status === "Scored" || item.status === "Locked");
  return <HistoryPanel records={records} />;
}
