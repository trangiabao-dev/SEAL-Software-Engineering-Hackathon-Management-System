namespace SealHackathon.Domain.Constants;

public static class AuditActionConstants
{
    public static class EventAudit
    {
        public const string Create = "Event.Create";
        public const string Update = "Event.Update";
        public const string Delete = "Event.Delete";
    }

    public static class EventStaffAudit
    {
        public const string Assign = "EventStaff.Assign";
        public const string Activate = "EventStaff.Activate";
        public const string Deactivate = "EventStaff.Deactivate";
    }

    public static class TrackAudit
    {
        public const string Create = "Track.Create";
        public const string Update = "Track.Update";
        public const string AssignMentor = "Track.AssignMentor";
    }

    public static class RoundAudit
    {
        public const string Create = "Round.Create";
        public const string Update = "Round.Update";
        public const string UpdateStatus = "Round.UpdateStatus";
        public const string AssignJudge = "Round.AssignJudge";
    }

    public static class TopicAudit
    {
        public const string Create = "Topic.Create";
        public const string Update = "Topic.Update";
        public const string Delete = "Topic.Delete";
    }

    public static class CriterionTemplateAudit
    {
        public const string Create = "CriterionTemplate.Create";
        public const string Update = "CriterionTemplate.Update";
        public const string Delete = "CriterionTemplate.Delete";
    }

    public static class CriterionAudit
    {
        public const string Create = "Criterion.Create";
        public const string Update = "Criterion.Update";
        public const string Delete = "Criterion.Delete";
        public const string ImportFromTemplate = "Criterion.ImportFromTemplate";
    }

    public static class TeamAudit
    {
        public const string Create = "Team.Create";
        public const string Update = "Team.Update";
        public const string Approve = "Team.Approve";
        public const string Disqualify = "Team.Disqualify";
        public const string AssignMentor = "Team.AssignMentor";
    }

    public static class TeamMemberAudit
    {
        public const string Add = "TeamMember.Add";
        public const string Update = "TeamMember.Update";
        public const string Delete = "TeamMember.Delete";
    }

    public static class SubmissionAudit
    {
        public const string Create = "Submission.Create";
        public const string Update = "Submission.Update";
        public const string Disqualify = "Submission.Disqualify";
    }

    public static class ScoreAudit
    {
        public const string Create = "Score.Create";
        public const string Update = "Score.Update";
    }

    public static class RankingAudit
    {
        public const string Calculate = "Ranking.Calculate";
    }

    public static class PrizeAudit
    {
        public const string Create = "Prize.Create";
        public const string Update = "Prize.Update";
        public const string Award = "Prize.Award";
    }
}