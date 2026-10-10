using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace ServiceDeskSystem.Tests.Frontend.Components.Primitives;

[TestFixture]
public class HelperUsageTests
{
    private static readonly string[] BannedTokens =
    [
        "GetStatusBadgeClass(",
        "GetPriorityBadgeClass(",
        "GetTicketTypeBadgeClass(",
        "GetPriorityColorClass(",
        "GetStatusDotColor(",
        "GetAvatarGradient(",
        "avatar-admin",
        "avatar-dev",
        "avatar-user",
    ];

    [Test]
    public void No_page_uses_color_helpers()
    {
        var repoRoot = FindRepoRoot();
        var componentsRoot = Path.Combine(repoRoot, "ServiceDeskSystem", "Components");
        var excludedFile = Path.Combine(componentsRoot, "UI", "Base", "BaseComponent.cs");

        var files = Directory.EnumerateFiles(componentsRoot, "*.razor", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(componentsRoot, "*.razor.cs", SearchOption.AllDirectories))
            .Where(f => !string.Equals(Path.GetFullPath(f), Path.GetFullPath(excludedFile), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var violations = new List<string>();

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            foreach (var token in BannedTokens)
            {
                if (content.Contains(token, StringComparison.Ordinal))
                {
                    violations.Add($"{file}: {token}");
                }
            }
        }

        violations.Should().BeEmpty(because: string.Join(Environment.NewLine, violations));
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ServiceDeskSystem.sln")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new DirectoryNotFoundException("Could not locate repo root containing ServiceDeskSystem.sln.");
        }

        return dir.FullName;
    }
}
