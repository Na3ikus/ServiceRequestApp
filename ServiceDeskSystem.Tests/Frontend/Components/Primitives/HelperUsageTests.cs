using System.IO;
using System.Linq;
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
        var wwwrootRoot = Path.Combine(repoRoot, "ServiceDeskSystem", "wwwroot");
        var appCssPath = Path.Combine(wwwrootRoot, "css", "app.css");

        var files = Directory.EnumerateFiles(componentsRoot, "*.razor", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(componentsRoot, "*.cs", SearchOption.AllDirectories))
            .Concat(Directory.EnumerateFiles(componentsRoot, "*.css", SearchOption.AllDirectories))
            .Concat(Directory.EnumerateFiles(wwwrootRoot, "*.css", SearchOption.AllDirectories))
            .Where(IsNotExcluded)
            .Where(file => !string.Equals(Path.GetFullPath(file), Path.GetFullPath(appCssPath), StringComparison.OrdinalIgnoreCase))
            .Distinct()
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

    [Test]
    public void Ticket_status_css_is_gone()
    {
        var repoRoot = FindRepoRoot();
        var cssPath = Path.Combine(repoRoot, "ServiceDeskSystem", "wwwroot", "css", "ticket-status.css");
        var appRazorPath = Path.Combine(repoRoot, "ServiceDeskSystem", "Components", "App.razor");

        File.Exists(cssPath).Should().BeFalse(because: $"{cssPath} should have been deleted");
        File.ReadAllText(appRazorPath).Should().NotContain("ticket-status.css");
    }

    private static bool IsNotExcluded(string file)
    {
        var normalized = file.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var segments = normalized.Split(Path.DirectorySeparatorChar);

        return !segments.Contains("bin") && !segments.Contains("obj") && !segments.Contains("node_modules");
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
