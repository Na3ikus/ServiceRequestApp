using Bunit;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Components.Layout;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Layout;

[TestFixture]
public class NavMenuTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _host.Localization.Setup(l => l.Translate(It.IsAny<string>())).Returns<string>(key => key switch
        {
            "nav.home" => "Home",
            "nav.admin" => "Administration",
            _ => key,
        });
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private IRenderedComponent<NavMenu> RenderAs(UserRole role)
    {
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 1, Login = "u", Role = role });
        return _host.Context.RenderComponent<NavMenu>();
    }

    private static IEnumerable<string> Hrefs(IRenderedComponent<NavMenu> cut) =>
        cut.FindAll("li a").Select(a => a.GetAttribute("href") ?? string.Empty);

    [Test]
    public void Admin_sees_admin_and_audit_items()
    {
        var cut = RenderAs(UserRole.Admin);

        Hrefs(cut).Should().Contain(["admin", "audit-logs", "tickets"]);
        Hrefs(cut).Should().NotContain("my-tickets");
    }

    [Test]
    public void User_sees_my_tickets_and_no_admin()
    {
        var cut = RenderAs(UserRole.User);

        Hrefs(cut).Should().Contain("my-tickets");
        Hrefs(cut).Should().NotContain(["admin", "audit-logs", "tickets"]);
    }

    [Test]
    public void Items_have_no_icons_and_lowercased_labels()
    {
        var cut = RenderAs(UserRole.Admin);

        cut.FindAll("li svg").Should().BeEmpty();
        var labels = cut.FindAll("li a").Select(a => a.TextContent.Replace("›", string.Empty).Trim()).ToList();
        labels.Should().Contain(["home", "administration"]);
    }

    [Test]
    public void Active_item_gets_an_accent_tint_in_light_theme()
    {
        var cut = RenderAs(UserRole.Admin);

        cut.FindAll("li a").Should().OnlyContain(a => a.ClassList.Contains("light:[&.active]:bg-accent/22"));
    }

    [Test]
    public void Has_logo_and_no_collapse_button()
    {
        var cut = RenderAs(UserRole.Admin);

        cut.FindAll("[data-logo]").Should().ContainSingle();
        cut.FindAll("button").Should().BeEmpty();
    }
}
