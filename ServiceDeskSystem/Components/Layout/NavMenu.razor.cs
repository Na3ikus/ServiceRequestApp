using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Localization.Interfaces;
using ServiceDeskSystem.Application.Services.Theme.Interfaces;

namespace ServiceDeskSystem.Components.Layout;

/// <summary>
/// Sidebar navigation component with role-aware sections and collapsible mode.
/// </summary>
public partial class NavMenu : ComponentBase, IDisposable
{
    private List<MenuSection> sections = [];

    [Parameter]
    public bool IsCollapsed { get; set; }

    [Parameter]
    public EventCallback ToggleCollapseRequested { get; set; }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        this.L.LanguageChanged += this.OnStateChanged;
        this.Theme.ThemeChanged += this.OnStateChanged;
    }

    protected override void OnParametersSet()
    {
        this.BuildMenu();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.L.LanguageChanged -= this.OnStateChanged;
            this.Theme.ThemeChanged -= this.OnStateChanged;
        }
    }

    private static string GetIconPath(string key) => key switch
    {
        "home" => "M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6",
        "tickets" => "M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01",
        "user" => "M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z",
        "code" => "M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4",
        "chart" => "M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z",
        "settings" => "M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z",
        _ => "M5 12h14"
    };

    private async Task ToggleCollapseAsync(MouseEventArgs mouseEventArgs)
    {
        ArgumentNullException.ThrowIfNull(mouseEventArgs);
        if (this.ToggleCollapseRequested.HasDelegate)
        {
            await this.ToggleCollapseRequested.InvokeAsync();
        }
    }

    private void BuildMenu()
    {
        var role = this.AuthService.CurrentUser?.Role;

        var general = new MenuSection("general", "General")
        {
            Items =
            {
                new MenuItem("home", this.L.Translate("nav.home"), string.Empty, "home", NavLinkMatch.All),
            },
        };

        var workspace = new MenuSection("workspace", "Workspace");

        if (role is "Admin" or "Developer")
        {
            workspace.Items.Add(new MenuItem("tickets", this.L.Translate("nav.allTickets"), "tickets", "tickets", NavLinkMatch.All));
        }

        if (role == "User")
        {
            workspace.Items.Add(new MenuItem("myTickets", this.L.Translate("nav.myTickets"), "my-tickets", "user", NavLinkMatch.All));
        }

        var management = new MenuSection("management", "Administration");

        if (role == "Developer")
        {
            management.Items.Add(new MenuItem("developer", this.L.Translate("nav.developer"), "developer", "code"));
            management.Items.Add(new MenuItem("statistics", this.L.Translate("nav.statistics"), "statistics", "chart"));
        }

        if (role == "Admin")
        {
            management.Items.Add(new MenuItem("developer", this.L.Translate("nav.myTickets"), "developer", "tickets"));
            management.Items.Add(new MenuItem("statistics", this.L.Translate("nav.statistics"), "statistics", "chart"));
            management.Items.Add(new MenuItem("admin", this.L.Translate("nav.admin"), "admin", "settings"));
        }

        this.sections = new List<MenuSection> { general, workspace, management }
            .Where(s => s.Items.Count > 0)
            .ToList();
    }

    private void ToggleSection(string sectionId)
    {
        var section = this.sections.FirstOrDefault(s => s.Id == sectionId);
        if (section is null)
        {
            return;
        }

        section.IsExpanded = !section.IsExpanded;
    }

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

    private sealed class MenuSection(string id, string title)
    {
        public string Id { get; } = id;

        public string Title { get; } = title;

        public List<MenuItem> Items { get; } = [];

        public bool IsExpanded { get; set; } = true;
    }

    private sealed class MenuItem(string id, string title, string url, string iconKey, NavLinkMatch match = NavLinkMatch.Prefix)
    {
        public string Id { get; } = id;

        public string Title { get; } = title;

        public string Url { get; } = url;

        public string IconKey { get; } = iconKey;

        public NavLinkMatch Match { get; } = match;
    }
}
