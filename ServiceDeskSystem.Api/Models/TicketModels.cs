namespace ServiceDeskSystem.Api.Models;

public sealed record UpdateStatusRequest(string Status);

public sealed record AssignDeveloperRequest(int DeveloperId);

public sealed record CreateCommentRequest(string Message, int AuthorId);

public sealed record UpdateCommentRequest(string Message);

public sealed record TicketStatsDto(int Total, int Open, int Critical);
