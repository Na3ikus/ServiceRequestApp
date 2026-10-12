using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceDeskSystem.Components.Layout;
using ServiceDeskSystem.Components.UI.Primitives;

namespace ServiceDeskSystem.Tests.Frontend.Components.Layout;

[TestFixture]
public class RoutePathTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Segments_of_root_is_empty()
    {
        RoutePath.Segments("http://h/", "http://h/").Should().BeEmpty();
    }

    [Test]
    public void Single_segment_has_no_href()
    {
        RoutePath.Segments("http://h/tickets", "http://h/")
            .Should().Equal(new PathSegment("tickets"));
    }

    [Test]
    public void Query_and_fragment_are_dropped()
    {
        RoutePath.Segments("http://h/tickets/42?x=1#c", "http://h/")
            .Should().Equal(new PathSegment("tickets"), new PathSegment("42"));
    }

    [Test]
    public void Segments_are_lowercased_and_trailing_slash_ignored()
    {
        RoutePath.Segments("http://h/Admin/", "http://h/")
            .Should().Equal(new PathSegment("admin"));
    }

    [Test]
    public void Encoded_segment_is_decoded()
    {
        RoutePath.Segments("http://h/tags/my%20tag", "http://h/")
            .Should().Equal(new PathSegment("tags"), new PathSegment("my tag"));
    }

    [Test]
    public void Renders_tilde_for_root()
    {
        var cut = _host.Context.RenderComponent<RoutePath>();

        cut.Find("nav").TextContent.Trim().Should().Be("~");
        cut.Find("[aria-current=page]").TextContent.Should().Be("~");
    }

    [Test]
    public void Marks_last_segment_current_and_follows_navigation()
    {
        var nav = _host.Context.Services.GetRequiredService<FakeNavigationManager>();
        var cut = _host.Context.RenderComponent<RoutePath>();

        nav.NavigateTo("ticket/42");

        cut.WaitForAssertion(() =>
        {
            cut.Find("nav").TextContent.Replace(" ", string.Empty).Should().Be("~/ticket/42");
            cut.Find("[aria-current=page]").TextContent.Should().Be("42");
            cut.FindAll("nav a").Select(a => a.TextContent).Should().Equal("~");
        });
    }
}
