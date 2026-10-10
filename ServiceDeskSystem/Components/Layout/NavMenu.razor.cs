using System.Globalization;
using Microsoft.AspNetCore.Components.Routing;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Layout;

/// <summary>
/// Sidebar navigation component with role-aware sections.
/// </summary>
public partial class NavMenu : ServiceDeskSystem.Components.UI.Base.BaseComponent
{
    private List<MenuSection> sections = [];

    protected override void OnParametersSet()
    {
        this.BuildMenu();
    }

    protected override void OnStateChanged(object? sender, EventArgs e)
    {
        this.BuildMenu();
        base.OnStateChanged(sender, e);
    }

    private void BuildMenu()
    {
        var role = this.AuthService.CurrentUser?.Role;

        var general = new MenuSection("general", this.L.Translate("nav.section.general"))
        {
            Items =
            {
                new MenuItem("home", this.L.Translate("nav.home"), string.Empty, NavLinkMatch.All),
                new MenuItem("settings", this.L.Translate("nav.settings"), "settings"),
            },
        };

        var workspace = new MenuSection("workspace", this.L.Translate("nav.section.workspace"));

        if (role is UserRole.Admin or UserRole.Developer)
        {
            workspace.Items.Add(new MenuItem("tickets", this.L.Translate("nav.allTickets"), "tickets", NavLinkMatch.All));
        }

        if (role == UserRole.User)
        {
            workspace.Items.Add(new MenuItem("myTickets", this.L.Translate("nav.myTickets"), "my-tickets", NavLinkMatch.All));
        }

        workspace.Items.Add(new MenuItem("calendar", this.L.Translate("nav.calendar") ?? "Календар", "calendar"));

        var management = new MenuSection("management", this.L.Translate("nav.section.management"));

        if (role == UserRole.Developer)
        {
            management.Items.Add(new MenuItem("developer", this.L.Translate("nav.developer"), "developer"));
            management.Items.Add(new MenuItem("statistics", this.L.Translate("nav.statistics"), "statistics"));
        }

        if (role == UserRole.Admin)
        {
            management.Items.Add(new MenuItem("developer", this.L.Translate("nav.myTickets"), "developer"));
            management.Items.Add(new MenuItem("statistics", this.L.Translate("nav.statistics"), "statistics"));
            management.Items.Add(new MenuItem("admin", this.L.Translate("nav.admin"), "admin"));
            management.Items.Add(new MenuItem("audit", this.L.Translate("audit.title") ?? "Audit Logs", "audit-logs"));
        }

        this.sections = new List<MenuSection> { general, workspace, management }
            .Where(s => s.Items.Count > 0)
            .ToList();
    }

    private sealed class MenuSection(string id, string title)
    {
        public string Id { get; } = id;

        public string Title { get; } = title.ToLower(CultureInfo.CurrentCulture);

        public IList<MenuItem> Items { get; } = [];
    }

    private sealed class MenuItem(string id, string title, string url, NavLinkMatch match = NavLinkMatch.Prefix)
    {
        public string Id { get; } = id;

        public string Title { get; } = title.ToLower(CultureInfo.CurrentCulture);

        public string Url { get; } = url;

        public NavLinkMatch Match { get; } = match;
    }
}
