namespace ServiceDeskSystem.Application.Services.Localization;

public sealed record LanguageInfo(string Code, string Name, string NativeName);

public interface ILocalizationService
{
    event EventHandler? LanguageChanged;

    string CurrentLanguage { get; }

    IReadOnlyList<LanguageInfo> SupportedLanguages { get; }

    void SetLanguage(string language);

    string Translate(string key);
}

