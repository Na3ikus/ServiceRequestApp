using System.Collections.Generic;
using System.Linq;
using Bunit;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Components.Pages.Admin.Components;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components;

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

        cut.FindAll("tr.atab-row").Should().HaveCount(2);
        cut.Markup.Should().Contain("Critical — default");
        cut.Markup.Should().Contain("Low — archived");
    }

    [Test]
    public void RendersDurationsInCompactForm()
    {
        var cut = Render(SamplePolicies());
        var markup = cut.Markup;

        markup.Should().Contain("1h");
        markup.Should().Contain("4h");
        markup.Should().Contain("1d");
        markup.Should().Contain("14d");
    }

    [Test]
    public void ShowsProductNameForScopedPolicyAndFallbackForGlobal()
    {
        var cut = Render(SamplePolicies());

        cut.Markup.Should().Contain("HR Portal");
        cut.Markup.Should().Contain("sla.allProducts");
    }

    [Test]
    public void MarksPriorityWithMatchingChipClass()
    {
        var cut = Render(SamplePolicies());

        cut.FindAll("span.sla-priority-chip.is-critical").Should().HaveCount(1);
        cut.FindAll("span.sla-priority-chip.is-low").Should().HaveCount(1);
    }

    [Test]
    public void DistinguishesActiveFromInactivePolicies()
    {
        var cut = Render(SamplePolicies());

        cut.FindAll("button.sla-status-toggle.is-active").Should().HaveCount(1);
        cut.FindAll("button.sla-status-toggle.is-inactive").Should().HaveCount(1);
    }

    [Test]
    public void ShowsEscalationBadgeOnlyForEscalatingPolicies()
    {
        var cut = Render(SamplePolicies());

        cut.Markup.Split("sla.escalates").Length.Should().Be(2);
    }

    [Test]
    public void ShowsEmptyStateWhenNoPoliciesExist()
    {
        var cut = Render(new List<SlaPolicy>());

        cut.FindAll("tr.atab-row").Should().BeEmpty();
        cut.Find(".atab-empty-title").TextContent.Should().Contain("sla.noPolicies");
    }

    [Test]
    public void ShowsLoadingStateWhilePoliciesAreNull()
    {
        var cut = Render(null);

        cut.FindAll(".atab-loading").Should().HaveCount(1);
        cut.FindAll("tr.atab-row").Should().BeEmpty();
    }

    [Test]
    public void SearchFiltersByPolicyName()
    {
        var cut = Render(SamplePolicies());

        cut.Find("input.atab-search-input").Input("archived");

        cut.FindAll("tr.atab-row").Should().HaveCount(1);
        cut.Markup.Should().Contain("Low — archived");
        cut.Markup.Should().NotContain("Critical — default");
    }

    [Test]
    public void SearchFiltersByPriorityName()
    {
        var cut = Render(SamplePolicies());

        cut.Find("input.atab-search-input").Input("critical");

        cut.FindAll("tr.atab-row").Should().HaveCount(1);
        cut.Markup.Should().Contain("Critical — default");
    }

    [Test]
    public void SearchFiltersByProductName()
    {
        var cut = Render(SamplePolicies());

        cut.Find("input.atab-search-input").Input("HR Portal");

        cut.FindAll("tr.atab-row").Should().HaveCount(1);
        cut.Markup.Should().Contain("Low — archived");
    }

    [Test]
    public void SearchWithNoMatchesOffersToClearTheQuery()
    {
        var cut = Render(SamplePolicies());

        cut.Find("input.atab-search-input").Input("nothing matches this");

        cut.FindAll("tr.atab-row").Should().BeEmpty();
        cut.Find(".atab-empty-title").TextContent.Should().Contain("admin.noSearchResults");
        cut.FindAll(".atab-empty-clear").Should().HaveCount(1);
    }

    [Test]
    public void TogglingActiveStateCallsTheService()
    {
        var cut = Render(SamplePolicies());

        cut.Find("button.sla-status-toggle.is-active").Click();

        _host.SlaPolicyService.Verify(
            s => s.SetActiveAsync(1, false, It.IsAny<int?>()),
            Times.Once);
    }

    [Test]
    public void RecalculateCallsTheService()
    {
        _host.SlaPolicyService
            .Setup(s => s.ReapplyToOpenTicketsAsync(It.IsAny<int?>()))
            .ReturnsAsync(3);

        var cut = Render(SamplePolicies());

        cut.FindAll("button.atab-add-btn").First().Click();

        _host.SlaPolicyService.Verify(s => s.ReapplyToOpenTicketsAsync(It.IsAny<int?>()), Times.Once);
    }

    [Test]
    public void ModalIsClosedUntilAddPolicyIsClicked()
    {
        var cut = Render(SamplePolicies());

        cut.FindAll(".amodal-overlay").Should().BeEmpty();

        cut.FindAll("button.atab-add-btn").Last().Click();

        cut.FindAll(".amodal-overlay").Should().HaveCount(1);
        cut.Markup.Should().Contain("sla.addPolicy");
    }
}
