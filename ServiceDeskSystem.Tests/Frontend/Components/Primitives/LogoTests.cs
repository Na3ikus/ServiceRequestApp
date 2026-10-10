using Bunit;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.UI.Primitives;

namespace ServiceDeskSystem.Tests.Frontend.Components.Primitives;

[TestFixture]
public class LogoTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Renders_sd_and_cursor()
    {
        var cut = _host.Context.RenderComponent<Logo>();

        cut.Find("[data-logo]").TextContent.Should().Be("sd_");
    }

    [Test]
    public void Aria_label_uses_service_desk_translation()
    {
        var cut = _host.Context.RenderComponent<Logo>();

        var span = cut.Find("[data-logo]");
        span.GetAttribute("role").Should().Be("img");
        span.GetAttribute("aria-label").Should().Be("nav.serviceDesk");
    }

    [TestCase(LogoSize.Md, "text-20")]
    [TestCase(LogoSize.Lg, "text-40")]
    public void Size_classes(LogoSize size, string expectedClass)
    {
        var cut = _host.Context.RenderComponent<Logo>(p => p.Add(x => x.Size, size));

        cut.Find("[data-logo]").GetAttribute("class").Should().Contain(expectedClass);
    }
}
