namespace ServiceDeskSystem.Services;

internal interface ILocalizationService
{
    string CurrentLanguage { get; }

    void SetLanguage(string language);

    string Translate(string key);

    event EventHandler? LanguageChanged;
}
