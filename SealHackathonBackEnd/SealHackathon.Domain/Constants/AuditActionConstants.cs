namespace SealHackathon.Domain.Constants;

public static class AuditActionConstants
{
    public static class Team
    {
        public const string Create = "Team.Create";
        public const string Update = "Team.Update";
        public const string Approve = "Team.Approve";
        public const string Disqualify = "Team.Disqualify";
        public const string AssignMentor = "Team.AssignMentor";
    }

    public static class TeamMember
    {
        public const string Add = "TeamMember.Add";
        public const string Update = "TeamMember.Update";
        public const string Delete = "TeamMember.Delete";
    }

    public static class Submission
    {
        public const string Create = "Submission.Create";
        public const string Update = "Submission.Update";
        public const string Disqualify = "Submission.Disqualify";
    }

    public static class Score
    {
        public const string Create = "Score.Create";
        public const string Update = "Score.Update";
    }

    public static class Ranking
    {
        public const string Calculate = "Ranking.Calculate";
    }

    public static class Event
    {
        public const string Create = "Event.Create";
        public const string Update = "Event.Update";
        public const string Delete = "Event.Delete";
    }

    public static class EventStaff
    {
        public const string Assign = "EventStaff.Assign";
        public const string Activate = "EventStaff.Activate";
        public const string Deactivate = "EventStaff.Deactivate";
    }
}