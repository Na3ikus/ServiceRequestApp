using Bunit;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Components.Pages.Admin.Components;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Admin;

[TestFixture]
public class SlaTabTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    private static List<SlaPolicy> SamplePolicies() =>
    [
        new SlaPolicy
        {
            Id = 1,
            Name = "Critical — default",
            Priority = TicketPriority.Critical,
            ProductId = null,
            ResponseMinutes = 60,
            ResolutionMinutes = 240,
            WarningThresholdPercent = 75,
            EscalateOnBreach = true,
            IsActive = true,
        },
        new SlaPolicy
        {
            Id = 2,
            Name = "Low — archived",
            Priority = TicketPriority.Low,
            ProductId = 7,
            Product = new Product { Id = 7, Name = "HR Portal" },
            ResponseMinutes = 1440,
            ResolutionMinutes = 20160,
            WarningThresholdPercent = 90,
            IsActive = false,
        },
    ];

    private IRenderedComponent<SlaTab> Render(IList<SlaPolicy>? policies) =>
        _host.Context.RenderComponent<SlaTab>(p => p
            .Add(c => c.Policies, policies)
            .Add(c => c.Products, new List<Product>()));

    [Test]
    public void RendersOneRowPerPolicy()
    {
        var cut = Render(SamplePolicies());

        cut.FindAll("tr[data-row]").Should().HaveCount(2);
        cut.Markup.Should().Contain("Critical — default");
        cut.Markup.Should().Contain("Low — archived");
    }

    [Test]
    public void RendersDurationsInCompactForm()
    {
        var cut = Render(SamplePolicies());

        Cells(cut, 1).Should().Contain(["1h", "4h", "75%"]);
        Cells(cut, 2).Should().Contain(["1d", "14d", "90%"]);
    }

    [Test]
    public void ShowsProductNameForScopedPolicyAndFallbackForGlobal()
    {
        var cut = Render(SamplePolicies());

        Cells(cut, 1).Should().Contain("sla.allProducts");
        Cells(cut, 2).Should().Contain("HR Portal");
    }

    [Test]
    public void MarksPriorityWithPriorityTag()
    {
        var cut = Render(SamplePolicies());

        cut.Find("tr[data-row='1'] td:nth-child(2) [data-tone]").TextContent.Should().Contain("priority.critical");
        cut.Find("tr[data-row='2'] td:nth-child(2) [data-tone]").TextContent.Should().Contain("priority.low");
    }

    [Test]
    public void DistinguishesActiveFromInactivePolicies()
    {
        var cut = Render(SamplePolicies());

        cut.Find("tr[data-row='1'] button[role=switch]").GetAttribute("aria-checked").Should().Be("true");
        cut.Find("tr[data-row='2'] button[role=switch]").GetAttribute("aria-checked").Should().Be("false");
        cut.Find("tr[data-row='1']").HasAttribute("data-inactive").Should().BeFalse();
        cut.Find("tr[data-row='2']").HasAttribute("data-inactive").Should().BeTrue();
    }

    [Test]
    public void ShowsEscalationTagOnlyForEscalatingPolicies()
    {
        var cut = Render(SamplePolicies());

        cut.FindAll("[data-escalates]").Should().ContainSingle();
        cut.FindAll("tr[data-row='1'] [data-escalates]").Should().ContainSingle();
    }

    [Test]
    public void ShowsEmptyStateWhenNoPoliciesExist()
    {
        var cut = Render(new List<SlaPolicy>());

        cut.FindAll("tr[data-row]").Should().BeEmpty();
        cut.Markup.Should().Contain("sla.noPolicies");
    }

    [Test]
    public void ShowsLoadingStateWhilePoliciesAreNull()
    {
        var cut = Render(null);

        cut.FindAll("[data-loading]").Should().HaveCount(1);
        cut.FindAll("tr[data-row]").Should().BeEmpty();
    }

    [Test]
    public void SearchFiltersByPolicyName()
    {
        var cut = Render(SamplePolicies());

        cut.Find("input[data-search]").Input("archived");

        cut.FindAll("tr[data-row]").Should().HaveCount(1);
        cut.Markup.Should().Contain("Low — archived");
        cut.Markup.Should().NotContain("Critical — default");
    }

    [Test]
    public void SearchFiltersByPriorityName()
    {
        var cut = Render(SamplePolicies());

        cut.Find("input[data-search]").Input("critical");

        cut.FindAll("tr[data-row]").Should().HaveCount(1);
        cut.Markup.Should().Contain("Critical — default");
    }

    [Test]
    public void SearchFiltersByProductName()
    {
        var cut = Render(SamplePolicies());

        cut.Find("input[data-search]").Input("HR Portal");

        cut.FindAll("tr[data-row]").Should().HaveCount(1);
        cut.Markup.Should().Contain("Low — archived");
    }

    [Test]
    public void SearchWithNoMatchesOffersToClearTheQuery()
    {
        var cut = Render(SamplePolicies());

        cut.Find("input[data-search]").Input("nothing matches this");

        cut.FindAll("tr[data-row]").Should().BeEmpty();
        cut.Markup.Should().Contain("admin.noSearchResults");

        cut.Find("p button").Click();

        cut.FindAll("tr[data-row]").Should().HaveCount(2);
    }

    [Test]
    public void TogglingActiveStateCallsTheService()
    {
        var cut = Render(SamplePolicies());

        cut.Find("tr[data-row='1'] button[role=switch]").Click();

        _host.SlaPolicyService.Verify(s => s.SetActiveAsync(1, false, It.IsAny<int?>()), Times.Once);
    }

    [Test]
    public void RecalculateCallsTheService()
    {
        _host.SlaPolicyService
            .Setup(s => s.ReapplyToOpenTicketsAsync(It.IsAny<int?>()))
            .ReturnsAsync(3);

        var cut = Render(SamplePolicies());

        cut.Find("[data-action=recalculate]").Click();

        _host.SlaPolicyService.Verify(s => s.ReapplyToOpenTicketsAsync(It.IsAny<int?>()), Times.Once);
    }

    [Test]
    public void AddPolicyOpensAnEmptyEditRowAtTheTop()
    {
        var cut = Render(SamplePolicies());

        cut.FindAll("[data-edit-row]").Should().BeEmpty();

        cut.Find("[data-action=add]").Click();

        cut.Find("tbody > tr").HasAttribute("data-edit-row").Should().BeTrue();
        cut.Find("#sla-name").GetAttribute("value").Should().BeNullOrEmpty();
        cut.Find("[data-formatted=response]").TextContent.Should().Be("= 8h");
    }

    [Test]
    public void EditRowShowsTheFormattedDurationWhileTyping()
    {
        var cut = Render(SamplePolicies());
        cut.Find("tr[data-row='2'] [data-action=edit]").Click();

        cut.Find("[data-formatted=resolution]").TextContent.Should().Be("= 14d");

        cut.Find("#sla-response").Input("90");

        cut.Find("[data-formatted=response]").TextContent.Should().Be("= 1h 30m");
    }

    [Test]
    public void SavingAnEditedPolicyCallsTheService()
    {
        var cut = Render(SamplePolicies());
        cut.Find("tr[data-row='1'] [data-action=edit]").Click();

        cut.Find("#sla-name").Input("Critical — 24/7");
        cut.Find("[data-edit-row] form").Submit();

        _host.SlaPolicyService.Verify(
            s => s.UpdatePolicyAsync(It.Is<SlaPolicy>(p => p.Id == 1 && p.Name == "Critical — 24/7" && p.ProductId == null), It.IsAny<int?>()),
            Times.Once);
        cut.FindAll("[data-edit-row]").Should().BeEmpty();
    }

    [Test]
    public void DeletingAPolicyAsksFirst()
    {
        _host.SlaPolicyService.Setup(s => s.DeletePolicyAsync(2, It.IsAny<int?>())).ReturnsAsync(true);
        var cut = Render(SamplePolicies());

        cut.Find("tr[data-row='2'] [data-action=delete]").Click();

        cut.Find("[data-delete-prompt]").TextContent.Should().Contain("rm").And.Contain("policy").And.Contain("\"Low — archived\"");
        _host.SlaPolicyService.Verify(s => s.DeletePolicyAsync(It.IsAny<int>(), It.IsAny<int?>()), Times.Never);

        cut.Find("[data-action=confirm]").Click();

        _host.SlaPolicyService.Verify(s => s.DeletePolicyAsync(2, It.IsAny<int?>()), Times.Once);
    }

    private static List<string> Cells(IRenderedComponent<SlaTab> cut, int id) =>
        cut.FindAll($"tr[data-row='{id}'] td").Select(td => td.TextContent.Trim()).ToList();
}
