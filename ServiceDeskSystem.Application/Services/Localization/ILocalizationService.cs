namespace ServiceDeskSystem.Application.Services.Localization;

public sealed record LanguageInfo(string Code, string Name, string NativeName, string? Culture = null)
{
    public string CultureName => !string.IsNullOrWhiteSpace(this.Culture) ? this.Culture : this.Code;
}

public interface ILocalizationService
{
    event EventHandler? LanguageChanged;

    string CurrentLanguage { get; }

    IReadOnlyList<LanguageInfo> SupportedLanguages { get; }

    void SetLanguage(string language);

    string Translate(string key);
}

