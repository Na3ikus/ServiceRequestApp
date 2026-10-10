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

    [Test]
    public void Text_sizes_use_rem_tokens_not_pixels()
    {
        // text-[13px] ignores the interface-scale setting; use text-12/13/15/20/40 (rem) instead.
        // 10px/11px on legacy pages are left for Stage 3.
        var repoRoot = FindRepoRoot();
        var componentsRoot = Path.Combine(repoRoot, "ServiceDeskSystem", "Components");
        var inputCss = Path.Combine(repoRoot, "ServiceDeskSystem", "Styles", "input.css");
        var pixelSize = new System.Text.RegularExpressions.Regex(@"text-\[(1[2-9]|[2-9]\d)px\]");

        var violations = Directory.EnumerateFiles(componentsRoot, "*.razor", SearchOption.AllDirectories)
            .Append(inputCss)
            .Where(IsNotExcluded)
            .SelectMany(file => pixelSize.Matches(File.ReadAllText(file)).Select(m => $"{file}: {m.Value}"))
            .ToList();

        violations.Should().BeEmpty(because: string.Join(Environment.NewLine, violations));
    }

    [Test]
    public void Logo_cursor_uses_a_valid_step_timing()
    {
        // steps(1, jump-none) is invalid CSS and silently drops the whole animation.
        var css = File.ReadAllText(Path.Combine(FindRepoRoot(), "ServiceDeskSystem", "Components", "UI", "Primitives", "Logo.razor.css"));

        css.Should().Contain("animation: logo-blink").And.NotContain("jump-none");
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
