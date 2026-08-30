namespace ServiceDeskSystemApp.Models;

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum TicketType
{
    Bug,
    FeatureRequest,
    Support,
    Other
}

public enum UserRole
{
    User,
    Agent,
    Admin
}
