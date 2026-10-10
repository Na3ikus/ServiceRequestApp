namespace ServiceDeskSystem.Application.Services.Preferences;

public interface IPreferencesService
{
    event EventHandler? Changed;

    UserPreferences Current { get; }

    Task<UserPreferences> LoadAsync();

    Task SetAsync(string name, string value);

    Task ResetAsync();

    string FormatDate(DateTime value);
}
