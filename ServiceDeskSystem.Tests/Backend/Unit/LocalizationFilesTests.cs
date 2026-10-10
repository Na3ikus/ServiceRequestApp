using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;

namespace ServiceDeskSystem.Tests.Backend.Unit;

/// <summary>
/// Guards the language pack files themselves: every key lives in exactly one file,
/// settings keys match across languages, and keys for removed settings stay gone.
/// </summary>
public class LocalizationFilesTests
{
    private static readonly string[] Languages = ["en", "uk", "de", "es", "fr"];

    private static readonly string[] ObsoleteSettingsPrefixes =
    [
        "settings.surface",
        "settings.cardStyle",
        "settings.card",
        "settings.hoverStyle",
        "settings.hover",
        "settings.bgTexture",
        "settings.bg",
        "settings.borderRadius",
        "settings.radius",
        "settings.fontFamily",
        "settings.fontSize",
        "settings.sidebarColor",
        "settings.pageTransition",
        "settings.transition",
        "settings.badgeStyle",
        "settings.badge",
        "settings.customAccent",
        "settings.animations",
        "settings.accentColor",
        "settings.contrast",
        "settings.notif",
        "settings.uiDensity",
        "settings.visualEffects",
        "settings.preview",
    ];

    private static string BasePath => Path.Combine(AppContext.BaseDirectory, "Localization", "LanguagePack");

    private static IEnumerable<string> FilesFor(string lang)
    {
        yield return Path.Combine(BasePath, $"{lang}.json");
        foreach (var file in Directory.GetFiles(Path.Combine(BasePath, lang), "*.json", SearchOption.AllDirectories)
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            yield return file;
        }
    }

    private static Dictionary<string, string> Read(string file) =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) ?? [];

    private static HashSet<string> AllKeys(string lang) =>
        FilesFor(lang).SelectMany(f => Read(f).Keys).ToHashSet(StringComparer.Ordinal);

    [TestCaseSource(nameof(Languages))]
    public void NoKeyDefinedInMoreThanOneFile(string lang)
    {
        var owners = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var file in FilesFor(lang))
        {
            foreach (var key in Read(file).Keys)
            {
                if (!owners.TryGetValue(key, out var list))
                {
                    owners[key] = list = [];
                }

                list.Add(Path.GetRelativePath(BasePath, file));
            }
        }

        var duplicated = owners.Where(o => o.Value.Count > 1)
            .Select(o => $"{o.Key} ({string.Join(", ", o.Value)})")
            .ToList();

        duplicated.Should().BeEmpty("each key must live in exactly one file for '{0}'", lang);
    }

    [Test]
    public void SettingsKeysHaveParityAcrossLanguages()
    {
        var english = AllKeys("en").Where(k => k.StartsWith("settings.", StringComparison.Ordinal)).ToHashSet();

        foreach (var lang in Languages.Skip(1))
        {
            var keys = AllKeys(lang).Where(k => k.StartsWith("settings.", StringComparison.Ordinal)).ToHashSet();
            keys.Except(english).Should().BeEmpty("'{0}' must not have settings keys missing from en", lang);
            english.Except(keys).Should().BeEmpty("'{0}' must define every en settings key", lang);
        }
    }

    [TestCaseSource(nameof(Languages))]
    public void NoObsoleteSettingsKeys(string lang)
    {
        var obsolete = AllKeys(lang)
            .Where(k => ObsoleteSettingsPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToList();

        obsolete.Should().BeEmpty();
    }
}
