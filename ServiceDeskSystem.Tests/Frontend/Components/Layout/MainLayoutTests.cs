using Bunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Components.Layout;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Tests.Frontend.Components.Layout;

[TestFixture]
public class MainLayoutTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _host.Context.Services.AddSingleton(Mock.Of<INotificationService>());
        _host.Context.Services.AddSingleton(Mock.Of<IHttpClientFactory>());
        _host.Context.Services.AddSingleton(Mock.Of<IDbContextFactory<BugTrackerDbContext>>());

        _host.ToastService.SetupGet(t => t.Toasts).Returns([]);

        // Auth restore never finishes: the layout stays on its first (loading) render.
        _host.Auth.Setup(a => a.EnsureRestoredAsync()).Returns(new TaskCompletionSource().Task);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Wrapper_has_no_dark_class_in_dark_theme()
    {
        // The dark class lives on <html> only (prefs.js); a nested one would pin the theme
        // and break the command palette's live preview.
        _host.Theme.SetupGet(t => t.IsDarkMode).Returns(true);

        var cut = _host.Context.RenderComponent<MainLayout>();

        cut.FindAll(".dark").Should().BeEmpty();
    }

    [Test]
    public async Task Reloads_notifications_only_when_they_change_for_its_own_user()
    {
        var notifications = new Mock<INotificationService>();
        notifications.Setup(n => n.GetRecentForUserAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync([]);
        _host.Context.Services.AddSingleton(notifications.Object);
        _host.Auth.SetupGet(a => a.IsAuthenticated).Returns(true);
        var cut = _host.Context.RenderComponent<MainLayout>();

        await _host.Realtime.NotifyNotificationsChangedAsync([2]);
        notifications.Verify(n => n.GetUnreadCountAsync(It.IsAny<int>()), Times.Never);

        await _host.Realtime.NotifyNotificationsChangedAsync([1, 2]);
        cut.WaitForAssertion(() => notifications.Verify(n => n.GetUnreadCountAsync(1), Times.Once));
    }
}
