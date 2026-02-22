using System.Text.Json;

namespace ServiceDeskSystem.Application.Services.Localization;

public sealed class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> translations = new();
    private string currentLanguage = "en";
    private bool isLoaded;

    public LocalizationService()
    {
        _ = Task.Run(async () => await this.LoadTranslationsAsync());
    }

    public event EventHandler? LanguageChanged;

    public string CurrentLanguage => this.currentLanguage;

    public void SetLanguage(string language)
    {
        if (this.translations.ContainsKey(language) && this.currentLanguage != language)
        {
            this.currentLanguage = language;
            this.LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string Translate(string key)
    {
        if (!this.isLoaded)
        {
            return key;
        }

        if (this.translations.TryGetValue(this.currentLanguage, out var langDict) &&
            langDict.TryGetValue(key, out var value))
        {
            return value;
        }

        if (this.translations.TryGetValue("en", out var enDict) &&
            enDict.TryGetValue(key, out var enValue))
        {
            return enValue;
        }

        return key;
    }

    private async Task LoadTranslationsAsync()
    {
        var languages = new[] { "en", "uk" };

        foreach (var lang in languages)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Localization", "LanguagePack", $"{lang}.json");
            try
            {
                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        Console.WriteLine($"Warning: Localization file '{filePath}' is empty. Skipping...");
                        continue;
                    }

                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null)
                    {
                        this.translations[lang] = dict;
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Localization file '{filePath}' not found.");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing localization file '{filePath}': {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading localization file '{filePath}': {ex.Message}");
            }
        }

        this.isLoaded = true;
    }
}
