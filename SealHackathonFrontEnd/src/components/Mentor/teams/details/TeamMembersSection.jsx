import { MentorBadge } from "../../shared/MentorBadge";
import { MentorPanel } from "../../shared/MentorPanel";
import { mentorIcons } from "../../shared/mentorIcons";

export function TeamMembersSection({ members }) {
  return (
    <MentorPanel title="Team Members" subtitle="Student information is read-only" icon={mentorIcons.Users}>
      <div className="overflow-x-auto rounded-xl border border-slate-100">
        <table className="min-w-full divide-y divide-slate-100 text-sm">
          <thead className="bg-slate-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-bold uppercase tracking-wider text-slate-500">FullName</th>
              <th className="px-4 py-3 text-left text-xs font-bold uppercase tracking-wider text-slate-500">StudentCode</th>
              <th className="px-4 py-3 text-left text-xs font-bold uppercase tracking-wider text-slate-500">IsFPTStudent</th>
              <th className="px-4 py-3 text-left text-xs font-bold uppercase tracking-wider text-slate-500">Role</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 bg-white">
            {members.map((member) => (
              <tr key={member.id}>
                <td className="whitespace-nowrap px-4 py-4 font-bold text-slate-900">{member.fullName}</td>
                <td className="whitespace-nowrap px-4 py-4 text-slate-600">{member.studentCode}</td>
                <td className="whitespace-nowrap px-4 py-4"><MentorBadge tone={member.isFPTStudent ? "orange" : "neutral"}>{member.isFPTStudent ? "FPT Student" : "External"}</MentorBadge></td>
                <td className="whitespace-nowrap px-4 py-4">{member.isLeader ? <MentorBadge tone="purple">Leader</MentorBadge> : <span className="text-slate-500">Member</span>}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </MentorPanel>
  );
}
