namespace ServiceDeskSystem.Api.Models;

public sealed record TagResponse(int Id, string Name, string Color, int TicketsCount);

public sealed record CreateTagRequest(string Name, string Color);

public sealed record UpdateTagRequest(string Name, string Color);
