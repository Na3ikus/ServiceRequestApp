using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using AdminPage = ServiceDeskSystem.Components.Pages.Admin.Admin;

namespace ServiceDeskSystem.Tests.Frontend.Components.Admin;

[TestFixture]
public class AdminPageTests
{
    private ComponentTestHost _host;
    private Mock<IAdminService> _admin;
    private Mock<IEmailSender> _email;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();

        _admin = new Mock<IAdminService>();
        _admin.Setup(a => a.GetAllTechStacksAsync()).ReturnsAsync([new TechStack { Id = 1, Name = ".NET", Type = "Backend" }]);
        _admin.Setup(a => a.GetAllProductsAsync()).ReturnsAsync(
        [
            new Product { Id = 1, Name = "Billing Portal", CurrentVersion = "2.4.1", TechStackId = 1 },
            new Product { Id = 2, Name = "CRM Core", CurrentVersion = "5.0.0", TechStackId = 1 },
        ]);
        _admin.Setup(a => a.GetAllUsersAsync()).ReturnsAsync(
        [
            new User { Id = 1, Login = "admin", Role = UserRole.Admin, IsActive = true, Person = new Person { FirstName = "Nazar", LastName = "Hutsol" } },
            new User { Id = 2, Login = "o.kovalenko", Role = UserRole.Developer, IsActive = true, Person = new Person { FirstName = "Oleh", LastName = "Kovalenko" } },
            new User { Id = 3, Login = "m.shevchenko", Role = UserRole.User, IsActive = true, Person = new Person { FirstName = "Maria", LastName = "Shevchenko" } },
        ]);

        var tags = new Mock<ITagService>();
        tags.Setup(t => t.GetAllTagsAsync()).ReturnsAsync([new Tag { Id = 1, Name = "ux", Color = "#64748B" }]);
        _host.SlaPolicyService.Setup(s => s.GetAllPoliciesAsync()).ReturnsAsync([]);

        _email = new Mock<IEmailSender>();
        _email.Setup(e => e.CheckConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync((true, "SMTP connection and authentication succeeded."));

        _host.Context.Services.AddSingleton(_admin.Object);
        _host.Context.Services.AddSingleton(tags.Object);
        _host.Context.Services.AddSingleton(_email.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private IRenderedComponent<AdminPage> Render(string? section) =>
        _host.Context.RenderComponent<AdminPage>(p => p.Add(c => c.Section, section));

    private static string ActiveTab(IRenderedComponent<AdminPage> cut) =>
        cut.Find("[data-admin-tabs] a[aria-current=page]").GetAttribute("data-tab")!;

    [Test]
    public void Users_route_renders_the_users_section()
    {
        var cut = Render("users");

        ActiveTab(cut).Should().Be("users");
        cut.FindAll("[data-admin-tabs] a[aria-current]").Should().ContainSingle();
        cut.Find("[data-register-note]").Should().NotBeNull();
        cut.Markup.Should().Contain("@o.kovalenko");
        cut.Find("h1").TextContent.Replace(" ", string.Empty).Should().Be("~/admin/users");
    }

    [TestCase(null)]
    [TestCase("nope")]
    [TestCase("PRODUCTS")]
    public void Root_and_unknown_sections_render_products(string? section)
    {
        var cut = Render(section);

        ActiveTab(cut).Should().Be("products");
        cut.FindAll("tr[data-row]").Should().HaveCount(2);
        cut.Markup.Should().Contain("Billing Portal");
    }

    [Test]
    public void Tabs_link_to_each_section_with_counts()
    {
        var cut = Render(null);

        cut.FindAll("[data-admin-tabs] a").Select(a => a.GetAttribute("href"))
            .Should().Equal("/admin/products", "/admin/techstacks", "/admin/users", "/admin/tags", "/admin/sla", "/admin/smtp");
        cut.Find("[data-tab=products]").TextContent.Replace(" ", string.Empty).Should().Contain("products2");
        cut.Find("[data-tab=users]").TextContent.Replace(" ", string.Empty).Should().Contain("users3");
    }

    [Test]
    public void Smtp_tab_shows_the_last_check_as_a_dot()
    {
        Render(null).Find("[data-tab=smtp] [data-smtp-dot]").GetAttribute("data-smtp-dot").Should().Be("ok");
    }

    [Test]
    public void A_failed_check_turns_the_dot_to_fail()
    {
        _email.Setup(e => e.CheckConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync((false, "auth failed"));

        Render(null).Find("[data-smtp-dot]").GetAttribute("data-smtp-dot").Should().Be("fail");
    }

    [Test]
    public void Non_admins_see_access_denied_and_no_tabs()
    {
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 5, Login = "dev", Role = UserRole.Developer });

        var cut = Render("users");

        cut.Find("[data-access-denied]").TextContent.Should().Contain("admin.accessDenied");
        cut.FindAll("[data-admin-tabs]").Should().BeEmpty();
        cut.FindAll("table").Should().BeEmpty();
        _admin.Verify(a => a.GetAllUsersAsync(), Times.Never);
    }

    [Test]
    public void Page_load_check_is_the_first_log_entry()
    {
        var cut = Render("smtp");

        var lines = cut.FindAll("[data-smtp-log] [data-log-line]").ToList();
        lines[0].GetAttribute("data-log-line").Should().Be("cmd");
        lines[0].TextContent.Should().Contain("$").And.Contain("smtp check");
        lines[1].GetAttribute("data-log-line").Should().Be("ok");
        lines[1].TextContent.Should().StartWith("ok:").And.Contain("admin.smtp.connected");
        _email.Verify(e => e.CheckConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Check_connection_appends_to_the_log()
    {
        var cut = Render("smtp");
        _email.Setup(e => e.CheckConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync((false, "auth failed"));

        cut.Find("[data-action=check]").Click();

        var lines = cut.FindAll("[data-smtp-log] [data-log-line]").ToList();
        lines.Should().HaveCount(4);
        lines[3].GetAttribute("data-log-line").Should().Be("err");
        lines[3].TextContent.Should().StartWith("err:").And.Contain("auth failed");
        cut.Find("[data-smtp-dot]").GetAttribute("data-smtp-dot").Should().Be("fail");
    }

    [Test]
    public void A_failed_send_adds_an_err_line()
    {
        _email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("relay denied"));
        var cut = Render("smtp");

        cut.Find("#smtp-to").Input("m.shevchenko@example.com");
        cut.Find("#smtp-subject").Input("hello");
        cut.Find("[data-action=send]").ParentElement!.ParentElement!.Submit();

        var lines = cut.FindAll("[data-smtp-log] [data-log-line]").ToList();
        lines[^2].TextContent.Should().Contain("smtp send m.shevchenko@example.com \"hello\"");
        lines[^1].GetAttribute("data-log-line").Should().Be("err");
        lines[^1].TextContent.Should().StartWith("err:").And.Contain("relay denied");
    }

    [Test]
    public void A_successful_send_adds_an_ok_line()
    {
        var cut = Render("smtp");

        cut.Find("#smtp-to").Input("m.shevchenko@example.com");
        cut.Find("[data-action=send]").ParentElement!.ParentElement!.Submit();

        _email.Verify(e => e.SendAsync("m.shevchenko@example.com", "ServiceDesk SMTP test", It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        var lines = cut.FindAll("[data-smtp-log] [data-log-line]").ToList();
        lines[^1].GetAttribute("data-log-line").Should().Be("ok");
        lines[^1].TextContent.Should().Contain("admin.smtp.sent");
    }

    [Test]
    public void An_invalid_recipient_is_logged_without_sending()
    {
        var cut = Render("smtp");

        cut.Find("#smtp-to").Input("not-an-email");
        cut.Find("[data-action=send]").ParentElement!.ParentElement!.Submit();

        _email.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        cut.FindAll("[data-smtp-log] [data-log-line]").Last().GetAttribute("data-log-line").Should().Be("err");
    }
}
