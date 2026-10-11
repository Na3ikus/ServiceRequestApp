using System.Text.Json;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using ServiceDeskSystem.Components.Pages.Auth;

namespace ServiceDeskSystem.Tests.Frontend.Components.Auth;

[TestFixture]
public class AuthShellTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Renders_path_heading_and_child_content()
    {
        var cut = _host.Context.RenderComponent<AuthShell>(p => p
            .Add(x => x.Path, "login")
            .Add(x => x.Subtitle, "login.title")
            .Add(x => x.ChildContent, (RenderFragment)(builder => builder.AddContent(0, "child-marker"))));

        cut.Find("h1").TextContent.Should().Be("~/login");
        cut.Markup.Should().Contain("child-marker");
    }

    [Test]
    public void Typewriter_gets_the_tagline_first_and_every_translated_quote()
    {
        var cut = _host.Context.RenderComponent<AuthShell>();

        var phrases = JsonSerializer.Deserialize<string[]>(cut.Find("[data-typewriter]").GetAttribute("data-phrases")!);

        phrases.Should().Equal(
            "auth.tagline",
            "auth.quote.1", "auth.quote.2", "auth.quote.3", "auth.quote.4",
            "auth.quote.5", "auth.quote.6", "auth.quote.7", "auth.quote.8");
    }

    [Test]
    public void Screen_readers_get_the_static_tagline_not_the_typing()
    {
        var cut = _host.Context.RenderComponent<AuthShell>();

        cut.Find("[data-typewriter] .sr-only").TextContent.Should().Be("auth.tagline");
        cut.Find("[data-typewriter-text]").Closest("[aria-hidden=true]").Should().NotBeNull();
    }

    [Test]
    public void Typing_starts_after_render_and_stops_on_dispose()
    {
        _host.Context.RenderComponent<AuthShell>();

        _host.Context.JSInterop.VerifyInvoke("typewriter.start");

        _host.Context.DisposeComponents();

        _host.Context.JSInterop.VerifyInvoke("typewriter.stop");
    }
}
