using Bunit;
using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Components.UI.Primitives;

namespace ServiceDeskSystem.Tests.Frontend.Components.Primitives;

[TestFixture]
public class PrimitivesTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Kbd_renders_kbd_element()
    {
        var cut = _host.Context.RenderComponent<Kbd>(p => p.AddChildContent("Ctrl+B"));

        cut.Find("kbd").TextContent.Should().Be("Ctrl+B");
    }
}
