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
}
