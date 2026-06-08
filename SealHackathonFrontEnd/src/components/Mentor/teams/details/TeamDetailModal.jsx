import { createPortal } from "react-dom";
import { MentorBadge } from "../../shared/MentorBadge";
import { MentorProgressBar } from "../../shared/MentorProgressBar";
import { mentorIcons } from "../../shared/mentorIcons";
import { TeamStatusBadge } from "../TeamStatusBadge";
import { SubmissionSection } from "./SubmissionSection";
import { TeamMembersSection } from "./TeamMembersSection";
import { TopicSection } from "./TopicSection";

export function TeamDetailModal({ team, onClose }) {
  const { X } = mentorIcons;

  if (!team) return null;
  return createPortal(
    <div className="fixed inset-0 z-[100] flex items-center justify-center bg-slate-950/50 backdrop-blur-sm p-4 animate-fade-in">
      <div className="max-h-[90vh] w-full max-w-5xl overflow-y-auto rounded-2xl bg-white shadow-2xl animate-modal-scale">
        <div className="sticky top-0 z-10 flex items-start justify-between gap-4 border-b bg-white px-5 py-4" style={{ borderColor: "#E5E7EB" }}>
          <div>
            <p className="text-xs font-semibold uppercase tracking-widest text-slate-500">Team detail view</p>
            <h3 className="mt-1 text-xl font-bold text-slate-900">{team.teamName}</h3>
          </div>
          <button className="rounded-xl border border-slate-200 p-2 text-slate-500 transition hover:bg-slate-50" onClick={onClose} aria-label="Close team details">
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="space-y-5 p-5">
          <section className="rounded-2xl border bg-white p-5" style={{ borderColor: "#E5E7EB" }}>
            <div className="grid gap-4 md:grid-cols-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">University</p>
                <p className="mt-1 font-bold text-slate-900">{team.university}</p>
              </div>
              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Track</p>
                <p className="mt-1 font-bold text-slate-900">{team.track}</p>
              </div>
              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Team Status</p>
                <div className="mt-1"><TeamStatusBadge status={team.status} /></div>
              </div>
              <div>
                <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Submission</p>
                <div className="mt-1"><MentorBadge tone={team.submission.status === "Submitted" ? "success" : team.submission.status === "Missing" ? "danger" : "warning"}>{team.submission.status}</MentorBadge></div>
              </div>
            </div>
            <div className="mt-5">
              <MentorProgressBar value={team.progress} label="Project readiness" />
            </div>
          </section>

          <TeamMembersSection members={team.members} />
          <SubmissionSection submission={team.submission} />
          <TopicSection topic={team.topic} />
        </div>
      </div>
    </div>,
    document.body
  );
}
