namespace ServiceDeskSystem.Application.Services.Localization;

public interface ILocalizationService
{
    event EventHandler? LanguageChanged;

    string CurrentLanguage { get; }

    void SetLanguage(string language);

    string Translate(string key);
}
