using Bunit;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.UI.Primitives;

namespace ServiceDeskSystem.Tests.Frontend.Components.Primitives;

[TestFixture]
public class AvatarTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [TestCase("Nazar Hutsol", "NH")]
    [TestCase("admin", "AD")]
    [TestCase("Олена Коваль", "ОК")]
    [TestCase("j.doe", "JD")]
    [TestCase("A", "A")]
    [TestCase("  Nazar   Hutsol ", "NH")]
    [TestCase("", "?")]
    [TestCase("   ", "?")]
    [TestCase(null, "?")]
    public void Initials_cases(string? name, string expected)
    {
        Avatar.Initials(name).Should().Be(expected);
    }

    [TestCase(AvatarSize.Sm, "h-6")]
    [TestCase(AvatarSize.Md, "h-7")]
    [TestCase(AvatarSize.Lg, "h-10")]
    public void Size_classes(AvatarSize size, string expectedClass)
    {
        var cut = _host.Context.RenderComponent<Avatar>(p => p
            .Add(x => x.Name, "Nazar Hutsol")
            .Add(x => x.Size, size));

        cut.Find("[data-avatar]").GetAttribute("class").Should().Contain(expectedClass);
    }

    [Test]
    public void Hidden_from_screen_readers_by_default()
    {
        var cut = _host.Context.RenderComponent<Avatar>(p => p
            .Add(x => x.Name, "Nazar Hutsol"));

        var span = cut.Find("[data-avatar]");
        span.GetAttribute("aria-hidden").Should().Be("true");
        span.GetAttribute("title").Should().BeNull();
    }

    [Test]
    public void ShowTitle_exposes_name()
    {
        var cut = _host.Context.RenderComponent<Avatar>(p => p
            .Add(x => x.Name, "Nazar Hutsol")
            .Add(x => x.ShowTitle, true));

        var span = cut.Find("[data-avatar]");
        span.GetAttribute("title").Should().Be("Nazar Hutsol");
        span.GetAttribute("aria-label").Should().Be("Nazar Hutsol");
        span.GetAttribute("role").Should().Be("img");
        span.GetAttribute("aria-hidden").Should().BeNull();
    }
}
