using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceDeskSystem.Components.Pages;

namespace ServiceDeskSystem.Tests.Frontend.Components.Pages;

[TestFixture]
public class StatusPagesTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Not_found_shows_the_missing_path_and_a_way_home()
    {
        _host.Context.Services.GetRequiredService<FakeNavigationManager>().NavigateTo("tickets/nope?x=1#top");

        var cut = _host.Context.RenderComponent<NotFound>();

        cut.Find("[data-status-code]").TextContent.Should().Be("404");
        cut.Find("[data-status-message]").TextContent.Should().Be("~/tickets/nope: errors.notFound");
        var home = cut.Find("a[data-status-home]");
        home.GetAttribute("href").Should().Be("/");
        home.TextContent.Should().Be("$ cd ~");
    }

    [Test]
    public void Access_denied_is_a_localized_403()
    {
        var cut = _host.Context.RenderComponent<AccessDenied>();

        cut.Find("[data-status-code]").TextContent.Should().Be("403");
        cut.Find("[data-status-message]").TextContent.Should().Be("errors.accessDenied");
        cut.Find("a[data-status-home]").GetAttribute("href").Should().Be("/");
        cut.FindAll("svg").Should().BeEmpty();
    }
}
