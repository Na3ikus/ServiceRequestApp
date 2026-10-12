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
}
