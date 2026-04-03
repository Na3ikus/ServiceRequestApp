using System.Text.Json;
using ServiceDeskSystem.Application.Services.Localization.Interfaces;

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
            var directoryPath = Path.Combine(AppContext.BaseDirectory, "Localization", "LanguagePack", lang);
            this.translations[lang] = new Dictionary<string, string>();

            try
            {
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);
                    
                    foreach (var filePath in files)
                    {
                        var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
                        if (string.IsNullOrWhiteSpace(json))
                        {
                            continue;
                        }

                        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                        if (dict != null)
                        {
                            foreach (var kvp in dict)
                            {
                                this.translations[lang][kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Localization directory '{directoryPath}' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading localization for '{lang}': {ex.Message}");
            }
        }

        this.isLoaded = true;
    }
}
