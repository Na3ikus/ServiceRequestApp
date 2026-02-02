using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Services.Localization;
using ServiceDeskSystem.Services.Theme;

namespace ServiceDeskSystem.Components.Common;

/// <summary>
/// Base component class with common functionality for localization, theming, and disposal.
/// </summary>
internal abstract class BaseComponent : ComponentBase, IDisposable
{
    protected bool disposed;

    [Inject]
    protected ILocalizationService L { get; set; } = null!;

    [Inject]
    protected IThemeService Theme { get; set; } = null!;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        this.L.LanguageChanged += this.OnStateChanged;
        this.Theme.ThemeChanged += this.OnStateChanged;
        base.OnInitialized();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing)
        {
            this.L.LanguageChanged -= this.OnStateChanged;
            this.Theme.ThemeChanged -= this.OnStateChanged;
        }

        this.disposed = true;
    }

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);
}
