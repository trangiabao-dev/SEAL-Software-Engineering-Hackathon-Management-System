import { MentorPanel } from "../../shared/MentorPanel";
import { mentorIcons } from "../../shared/mentorIcons";

export function TopicSection({ topic }) {
  const { ExternalLink } = mentorIcons;

  return (
    <MentorPanel title="Assigned Topic" subtitle="Topic requirements and reference material" icon={mentorIcons.Lightbulb}>
      <div className="rounded-xl border border-slate-100 bg-slate-50/70 p-5">
        <h4 className="text-lg font-bold text-slate-900">{topic.title}</h4>
        <p className="mt-3 leading-6 text-slate-600">{topic.description}</p>

        <div className="mt-5">
          <p className="text-sm font-bold uppercase tracking-wide text-slate-500">Requirements</p>
          <ul className="mt-3 space-y-2">
            {topic.requirements.map((requirement) => (
              <li key={requirement} className="flex gap-2 text-sm text-slate-700">
                <span className="mt-2 h-1.5 w-1.5 rounded-full bg-orange-500" />
                <span>{requirement}</span>
              </li>
            ))}
          </ul>
        </div>

        <div className="mt-5">
          <p className="text-sm font-bold uppercase tracking-wide text-slate-500">Attachment links</p>
          {topic.attachments.length > 0 ? (
            <div className="mt-3 flex flex-wrap gap-2">
              {topic.attachments.map((attachment) => (
                <a
                  key={attachment.label}
                  href={attachment.url}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-2 rounded-xl border border-slate-300 bg-white px-3 py-2 text-sm font-bold text-slate-700 transition hover:border-orange-400 hover:bg-orange-50 hover:text-orange-700"
                >
                  {attachment.label}
                  <ExternalLink className="h-4 w-4" />
                </a>
              ))}
            </div>
          ) : (
            <p className="mt-2 text-sm text-slate-500">No attachments provided for this topic.</p>
          )}
        </div>
      </div>
    </MentorPanel>
  );
}
