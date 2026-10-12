using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Components.Pages.Admin.Components;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Admin;

[TestFixture]
public class UsersTabTests
{
    private ComponentTestHost _host;
    private Mock<IAdminService> _admin;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _admin = new Mock<IAdminService>();
        _host.Context.Services.AddSingleton(_admin.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    // The signed-in user (ComponentTestHost) is #1, an admin.
    private static List<User> Users() =>
    [
        new User { Id = 1, Login = "admin", Role = UserRole.Admin, IsActive = true, Person = new Person { FirstName = "Nazar", LastName = "Hutsol" } },
        new User { Id = 2, Login = "o.kovalenko", Role = UserRole.Developer, IsActive = true, Person = new Person { FirstName = "Oleh", LastName = "Kovalenko" } },
        new User { Id = 3, Login = "m.shevchenko", Role = UserRole.User, IsActive = false, Person = new Person { FirstName = "Maria", LastName = "Shevchenko" } },
        new User { Id = 4, Login = "boss", Role = UserRole.Admin, IsActive = true, Person = new Person { FirstName = "Ivan", LastName = "Bosenko" } },
    ];

    private IRenderedComponent<UsersTab> Render() =>
        _host.Context.RenderComponent<UsersTab>(p => p.Add(c => c.Users, Users()));

    private static string CheckedRole(IRenderedComponent<UsersTab> cut, int id) =>
        cut.Find($"tr[data-row='{id}'] [role=radio][aria-checked=true]").TextContent.Trim();

    private static AngleSharp.Dom.IElement RoleButton(IRenderedComponent<UsersTab> cut, int id, string text) =>
        cut.FindAll($"tr[data-row='{id}'] [role=radio]").Single(b => b.TextContent.Trim() == text);

    [Test]
    public void A_role_change_calls_the_service()
    {
        _admin.Setup(a => a.UpdateUserRoleAsync(3, UserRole.Developer)).ReturnsAsync(true);
        var cut = Render();

        RoleButton(cut, 3, "dev").Click();

        _admin.Verify(a => a.UpdateUserRoleAsync(3, UserRole.Developer), Times.Once);
        CheckedRole(cut, 3).Should().Be("dev");
        cut.FindAll("[data-role-error]").Should().BeEmpty();
    }

    [Test]
    public void A_failed_role_change_reverts_the_control_and_says_not_saved()
    {
        _admin.Setup(a => a.UpdateUserRoleAsync(3, UserRole.Admin)).ReturnsAsync(false);
        _admin.Setup(a => a.UpdateUserRoleAsync(3, UserRole.Developer)).ReturnsAsync(true);
        var cut = Render();

        RoleButton(cut, 3, "admin").Click();

        CheckedRole(cut, 3).Should().Be("user");
        cut.Find("tr[data-row='3'] [data-role-error]").TextContent.Should().Contain("admin.notsaved");

        RoleButton(cut, 3, "dev").Click();

        CheckedRole(cut, 3).Should().Be("dev");
        cut.FindAll("[data-role-error]").Should().BeEmpty();
    }

    [Test]
    public void A_role_change_that_throws_also_reverts()
    {
        _admin.Setup(a => a.UpdateUserRoleAsync(2, UserRole.User)).ThrowsAsync(new InvalidOperationException("boom"));
        var cut = Render();

        RoleButton(cut, 2, "user").Click();

        CheckedRole(cut, 2).Should().Be("dev");
        cut.FindAll("tr[data-row='2'] [data-role-error]").Should().ContainSingle();
    }

    [Test]
    public void Your_own_row_shows_a_role_tag_instead_of_the_control()
    {
        var cut = Render();

        cut.FindAll("tr[data-row='1'] [role=radiogroup]").Should().BeEmpty();
        cut.Find("tr[data-row='1'] [data-tone]").TextContent.Should().Contain("admin.roleAdmin");
        cut.FindAll("tr[data-row='4'] [role=radiogroup]").Should().ContainSingle();
    }

    [Test]
    public void Admins_have_no_delete_and_a_disabled_switch()
    {
        var cut = Render();

        cut.FindAll("tr[data-row='4'] [data-action=delete]").Should().BeEmpty();
        cut.Find("tr[data-row='4']").TextContent.Should().Contain("—");
        cut.Find("tr[data-row='4'] button[role=switch]").HasAttribute("disabled").Should().BeTrue();
        cut.FindAll("tr[data-row='2'] [data-action=delete]").Should().ContainSingle();
        cut.Find("tr[data-row='2'] button[role=switch]").HasAttribute("disabled").Should().BeFalse();
    }

    [Test]
    public void The_active_switch_toggles_through_the_service()
    {
        _admin.Setup(a => a.ToggleUserActiveStatusAsync(3)).ReturnsAsync(true);
        var cut = Render();
        cut.Find("tr[data-row='3']").HasAttribute("data-inactive").Should().BeTrue();

        cut.Find("tr[data-row='3'] button[role=switch]").Click();

        _admin.Verify(a => a.ToggleUserActiveStatusAsync(3), Times.Once);
        cut.Find("tr[data-row='3'] button[role=switch]").GetAttribute("aria-checked").Should().Be("true");
        cut.Find("tr[data-row='3']").HasAttribute("data-inactive").Should().BeFalse();
    }

    [Test]
    public void Delete_asks_first_and_reports_users_with_tickets_in_a_toast()
    {
        _admin.Setup(a => a.DeleteUserAsync(2)).ReturnsAsync(false);
        var cut = Render();

        cut.Find("tr[data-row='2'] [data-action=delete]").Click();

        cut.Find("[data-delete-prompt]").TextContent.Should().Contain("user").And.Contain("\"o.kovalenko\"");
        _admin.Verify(a => a.DeleteUserAsync(It.IsAny<int>()), Times.Never);

        cut.Find("[data-action=confirm]").Click();

        _admin.Verify(a => a.DeleteUserAsync(2), Times.Once);
        _host.ToastService.Verify(t => t.ShowToastAsync("admin.cannotDeleteUserWithTickets", ServiceDeskSystem.Application.Services.Toasts.Models.ToastType.Error, It.IsAny<int>()), Times.Once);
    }

    [Test]
    public void Search_matches_login_and_names()
    {
        var cut = Render();

        cut.Find("input[data-search]").Input("shev");
        cut.FindAll("tr[data-row]").Should().ContainSingle();

        cut.Find("input[data-search]").Input("Oleh");
        cut.FindAll("tr[data-row]").Should().ContainSingle();

        cut.Find("input[data-search]").Input("o.koval");
        cut.FindAll("tr[data-row]").Should().ContainSingle();
    }

    [Test]
    public void Toolbar_points_to_register_instead_of_an_add_button()
    {
        var cut = Render();

        cut.Find("[data-register-note]").TextContent.Should().Contain("admin.usersregisternote");
        cut.FindAll("[data-action=add]").Should().BeEmpty();
    }
}
