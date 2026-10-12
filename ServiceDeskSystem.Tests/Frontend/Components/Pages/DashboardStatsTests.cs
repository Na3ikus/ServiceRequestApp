using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Components.UI.Elements;

namespace ServiceDeskSystem.Tests.Frontend.Components.Pages;

[TestFixture]
public class DashboardStatsTests
{
    private ComponentTestHost _host;
    private Mock<ITicketStatisticsService> _stats;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _stats = new Mock<ITicketStatisticsService>();
        _stats.Setup(s => s.GetTotalTicketsCountAsync()).ReturnsAsync(20);
        _stats.Setup(s => s.GetOpenTicketsCountAsync()).ReturnsAsync(5);
        _stats.Setup(s => s.GetUserTicketsCountAsync(It.IsAny<int>())).ReturnsAsync(4);
        _host.Context.Services.AddSingleton(_stats.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private IRenderedComponent<DashboardStats> Render(int critical)
    {
        _stats.Setup(s => s.GetCriticalTicketsCountAsync()).ReturnsAsync(critical);
        var cut = _host.Context.RenderComponent<DashboardStats>();
        cut.WaitForAssertion(() => cut.FindAll("[data-stat] p.text-40").ToList()[0].TextContent.Should().Be("20"));
        return cut;
    }

    [Test]
    public void Renders_four_mono_figures_with_hash_labels_and_no_icons_or_gradients()
    {
        var cut = Render(critical: 2);

        var stats = cut.FindAll("[data-stat]");
        stats.Should().HaveCount(4);
        stats.Select(s => s.QuerySelector("p")!.TextContent).Should().AllSatisfy(t => t.Should().StartWith("# "));
        cut.FindAll("svg").Should().BeEmpty();
        cut.Markup.Should().NotContain("gradient").And.NotContain("shadow");
    }

    [Test]
    public void Shows_share_of_total()
    {
        var cut = Render(critical: 2);

        cut.WaitForAssertion(() => cut.FindAll("[data-stat]").ToList()[1].InnerHtml.Should().Contain("width: 25%"));
    }

    [Test]
    public void Critical_figure_is_danger_only_when_above_zero()
    {
        var hot = Render(critical: 2);
        hot.WaitForAssertion(() => hot.FindAll("[data-stat] p.text-40").ToList()[2].ClassList.Should().Contain("text-danger"));

        var calm = Render(critical: 0);
        calm.FindAll("[data-stat] p.text-40").ToList()[2].ClassList.Should().NotContain("text-danger");
    }
}
