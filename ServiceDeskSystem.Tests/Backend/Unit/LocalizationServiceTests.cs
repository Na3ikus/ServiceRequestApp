using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Localization;

namespace ServiceDeskSystem.Tests.Backend.Unit;

[TestFixture]
public class LocalizationServiceTests
{
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        var service = new LocalizationService();
        // Wait until translations are loaded
        for (int i = 0; i < 100; i++)
        {
            service.SetLanguage("uk");
            if (service.Translate("nav.home") == "Головна")
            {
                break;
            }
            await Task.Delay(50);
        }
    }

    [Test]
    public void SupportedLanguages_ShouldContainAllExpectedLanguages()
    {
        var expectedCodes = new[] { "en", "uk", "es", "fr", "de" };
        var actualCodes = LocalizationConstants.SupportedLanguages.Select(l => l.Code).ToList();

        actualCodes.Should().Contain(expectedCodes);
    }

    [TestCase("en", "Home")]
    [TestCase("uk", "Головна")]
    [TestCase("es", "Inicio")]
    [TestCase("fr", "Accueil")]
    [TestCase("de", "Startseite")]
    public void Translate_ShouldReturnCorrectTranslation_ForSupportedLanguages(string langCode, string expectedNavHome)
    {
        var service = new LocalizationService();

        service.SetLanguage(langCode);
        service.CurrentLanguage.Should().Be(langCode);

        var result = service.Translate("nav.home");
        result.Should().Be(expectedNavHome);
    }

    [TestCase("es")]
    [TestCase("fr")]
    [TestCase("de")]
    [TestCase("uk")]
    [TestCase("en")]
    public void Translate_CommonKeys_ShouldBeNonEmptyAndNotEqualKeyName(string langCode)
    {
        var service = new LocalizationService();

        service.SetLanguage(langCode);

        var keysToTest = new[]
        {
            "login.signIn",
            "tickets.title",
            "dashboard.welcome",
            "table.status",
            "common.logout"
        };

        foreach (var key in keysToTest)
        {
            var translation = service.Translate(key);
            translation.Should().NotBeNullOrWhiteSpace();
            translation.Should().NotBe(key, $"Key '{key}' should have a translation for '{langCode}'");
        }
    }
}
