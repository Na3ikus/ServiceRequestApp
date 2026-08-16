namespace ServiceDeskSystem.Domain.Constants;

public static class TicketConstants
{
    public static class Priorities
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Critical = "Critical";

        public static readonly string[] All = [Low, Medium, High, Critical];
    }

    public static class Types
    {
        public const string Support = "Support";
        public const string Bug = "Bug";
        public const string Project = "Project";
        public const string Consultation = "Consultation";

        public static readonly string[] All = [Support, Bug, Project, Consultation];
    }

    public static class Statuses
    {
        public const string Open = "Open";
        public const string InProgress = "In Progress";
        public const string Resolved = "Resolved";
        public const string Closed = "Closed";
        public const string New = "New";
        public const string Testing = "Testing";
        public const string CodeReview = "Code Review";
        public const string Done = "Done";
    }
}
