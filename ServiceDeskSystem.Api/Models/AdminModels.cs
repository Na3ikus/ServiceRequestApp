namespace ServiceDeskSystem.Api.Models;

/// <summary>Request DTO for creating or updating a TechStack. Prevents mass assignment of domain entity properties.</summary>
public sealed record TechStackRequest(string Name, string Type);

/// <summary>Request DTO for creating or updating a Product. Prevents mass assignment of domain entity properties.</summary>
public sealed record ProductRequest(string Name, string Description, string CurrentVersion, int TechStackId);
