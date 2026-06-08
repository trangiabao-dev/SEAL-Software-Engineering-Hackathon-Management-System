import { MentorBadge } from "../shared/MentorBadge";

export function TeamStatusBadge({ status }) {
  const tone = status === "Approved" ? "success" : status === "Disqualified" ? "danger" : "warning";
  return <MentorBadge tone={tone}>{status}</MentorBadge>;
}
