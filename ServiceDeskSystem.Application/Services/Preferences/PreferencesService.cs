using System.Globalization;
using Microsoft.JSInterop;

namespace ServiceDeskSystem.Application.Services.Preferences;

/// <summary>Typed C# wrapper over <c>window.prefs</c> (<c>wwwroot/js/prefs.js</c>).</summary>
public sealed class PreferencesService : IPreferencesService
{
    private readonly IJSRuntime jsRuntime;

    public PreferencesService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public event EventHandler? Changed;

    public UserPreferences Current { get; private set; } = UserPreferences.Defaults;

    public async Task<UserPreferences> LoadAsync()
    {
        Dictionary<string, string>? raw;
        try
        {
            raw = await this.jsRuntime.InvokeAsync<Dictionary<string, string>>("prefs.getAll");
        }
        catch
        {
            // Prerendering or disconnected circuit: JS is unavailable.
            raw = null;
        }

        var result = UserPreferences.Defaults;
        if (raw != null)
        {
            foreach (var name in PreferenceSchema.Allowed.Keys)
            {
                if (raw.TryGetValue(name, out var value) && PreferenceSchema.IsAllowed(name, value))
                {
                    result = result.With(name, value);
                }
            }
        }

        this.Current = result;
        return result;
    }

    public async Task SetAsync(string name, string value)
    {
        if (!PreferenceSchema.Allowed.ContainsKey(name))
        {
            throw new ArgumentException($"Unknown preference '{name}'.", nameof(name));
        }

        if (!PreferenceSchema.IsAllowed(name, value))
        {
            throw new ArgumentException($"Value '{value}' is not allowed for '{name}'.", nameof(value));
        }

        try
        {
            await this.jsRuntime.InvokeVoidAsync("prefs.set", name, value);
        }
        catch (Exception ex) when (ex is JSException or InvalidOperationException or JSDisconnectedException or TaskCanceledException)
        {
            // Ignore JS interop errors during prerendering or disconnects.
        }

        this.Current = this.Current.With(name, value);
        this.Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task ResetAsync()
    {
        try
        {
            await this.jsRuntime.InvokeVoidAsync("prefs.reset");
        }
        catch (Exception ex) when (ex is JSException or InvalidOperationException or JSDisconnectedException or TaskCanceledException)
        {
            // Ignore JS interop errors during prerendering or disconnects.
        }

        this.Current = UserPreferences.Defaults;
        this.Changed?.Invoke(this, EventArgs.Empty);
    }

    public string FormatDate(DateTime value)
    {
        var pattern = this.Current.DateFormat switch
        {
            "MM/DD/YYYY" => "MM/dd/yyyy",
            "YYYY-MM-DD" => "yyyy-MM-dd",
            _ => "dd/MM/yyyy",
        };

        return value.ToString(pattern, CultureInfo.InvariantCulture);
    }
}
