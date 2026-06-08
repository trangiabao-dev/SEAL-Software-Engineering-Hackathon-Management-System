import { JudgeBadge } from "../shared/JudgeBadge";

export function RoundStatusBadge({ status }) {
  const tone = status === "Scoring" ? "purple" : status === "Active" ? "orange" : status === "Closed" ? "success" : "neutral";
  return <JudgeBadge tone={tone}>{status}</JudgeBadge>;
}
