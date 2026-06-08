import { MentorBadge } from "../../shared/MentorBadge";
import { MentorPanel } from "../../shared/MentorPanel";
import { mentorIcons } from "../../shared/mentorIcons";

function LinkCard({ label, url }) {
  const { ExternalLink, FileText } = mentorIcons;

  if (!url) {
    return (
      <div className="rounded-xl border border-slate-100 bg-slate-50 p-4 text-sm text-slate-500">
        {label} is not available yet.
      </div>
    );
  }

  return (
    <a
      href={url}
      target="_blank"
      rel="noreferrer"
      className="flex items-center justify-between gap-3 rounded-xl border border-slate-100 p-4 transition hover:border-orange-200 hover:bg-orange-50"
    >
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-orange-50 text-orange-600">
          <FileText className="h-5 w-5" />
        </div>
        <div>
          <p className="font-bold text-slate-900">{label}</p>
          <p className="max-w-xs truncate text-sm text-slate-500">{url}</p>
        </div>
      </div>
      <ExternalLink className="h-4 w-4 text-slate-400" />
    </a>
  );
}

export function SubmissionSection({ submission }) {
  const statusTone = submission.status === "Submitted" ? "success" : submission.status === "Missing" ? "danger" : "warning";

  return (
    <MentorPanel title="Submission" subtitle="Read-only project materials for the current round" icon={mentorIcons.FileText}>
      <div className="mb-4 grid gap-3 md:grid-cols-3">
        <div className="rounded-xl bg-slate-50 p-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Submission status</p>
          <div className="mt-2"><MentorBadge tone={statusTone}>{submission.status}</MentorBadge></div>
        </div>
        <div className="rounded-xl bg-slate-50 p-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Submission time</p>
          <p className="mt-2 font-bold text-slate-900">{submission.submittedAt}</p>
        </div>
        <div className="rounded-xl bg-slate-50 p-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">Round information</p>
          <p className="mt-2 font-bold text-slate-900">{submission.round}</p>
        </div>
      </div>
      <div className="grid gap-3 md:grid-cols-2">
        <LinkCard label="DemoUrl" url={submission.demoUrl} />
        <LinkCard label="ReportUrl" url={submission.reportUrl} />
      </div>
    </MentorPanel>
  );
}
