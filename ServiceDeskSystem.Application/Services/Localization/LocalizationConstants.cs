namespace ServiceDeskSystem.Application.Services.Localization;

public static class LocalizationConstants
{

    public const string DefaultLanguage = "en";

    public static readonly IReadOnlyList<LanguageInfo> SupportedLanguages = new List<LanguageInfo>
    {
        new("en", "English", "English", "en-US"),
        new("uk", "Ukrainian", "Українська", "uk-UA"),
        //new("es", "Spanish", "Español", "es-ES"), Test test test
    };

    public static LanguageInfo? GetLanguage(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return SupportedLanguages.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));
    }
}
