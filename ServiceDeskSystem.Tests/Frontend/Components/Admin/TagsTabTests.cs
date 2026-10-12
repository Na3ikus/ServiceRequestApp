using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Components.Pages.Admin.Components;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Tests.Frontend.Components.Admin;

[TestFixture]
public class TagsTabTests
{
    private ComponentTestHost _host;
    private Mock<ITagService> _tags;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _tags = new Mock<ITagService>();
        _host.Context.Services.AddSingleton(_tags.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private IRenderedComponent<TagsTab> Render() =>
        _host.Context.RenderComponent<TagsTab>(p => p.Add(c => c.Tags, new List<Tag>
        {
            new () { Id = 1, Name = "regression", Color = "#10B981", Tickets = [new Ticket { Id = 1 }, new Ticket { Id = 2 }] },
            new () { Id = 2, Name = "ux", Color = "#64748B" },
        }));

    [Test]
    public void Picking_a_preset_updates_the_hex_input_and_the_preview()
    {
        var cut = Render();
        cut.Find("[data-action=add]").Click();
        cut.Find("#tag-name").Input("perf");

        cut.Find("[data-preset='#EF4444']").Click();

        cut.Find("#tag-color-hex").GetAttribute("value").Should().Be("#EF4444");
        cut.Find("[data-preset='#EF4444']").GetAttribute("aria-pressed").Should().Be("true");
        var preview = cut.Find("[data-tag-preview]");
        preview.TextContent.Should().Contain("perf");
        preview.InnerHtml.Should().Contain("background-color: #EF4444");
    }

    [Test]
    public void Edit_row_starts_with_the_tag_values()
    {
        var cut = Render();

        cut.Find("tr[data-row='2'] [data-action=edit]").Click();

        cut.Find("#tag-name").GetAttribute("value").Should().Be("ux");
        cut.Find("#tag-color-hex").GetAttribute("value").Should().Be("#64748B");
        cut.Find("[data-preset='#64748B']").GetAttribute("aria-pressed").Should().Be("true");
    }

    [Test]
    public void Saving_a_new_tag_calls_the_service()
    {
        _tags.Setup(t => t.CreateTagAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(new Tag());
        var cut = Render();
        cut.Find("[data-action=add]").Click();
        cut.Find("#tag-name").Input("perf");
        cut.Find("[data-preset='#8B5CF6']").Click();

        cut.Find("[data-edit-row] form").Submit();

        _tags.Verify(t => t.CreateTagAsync("perf", "#8B5CF6", It.IsAny<int?>()), Times.Once);
        cut.FindAll("[data-edit-row]").Should().BeEmpty();
    }

    [Test]
    public void Delete_prompt_mentions_the_tickets_that_lose_the_tag()
    {
        _host.Localization.Setup(l => l.Translate("admin.tagDeleteDetail")).Returns("{0} ticket(s) will lose this tag");
        _tags.Setup(t => t.DeleteTagAsync(1, It.IsAny<int?>())).ReturnsAsync(true);
        var cut = Render();

        cut.Find("tr[data-row='1'] [data-action=delete]").Click();

        cut.Find("[data-delete-note]").TextContent.Should().Contain("2 ticket(s) will lose this tag");
        _tags.Verify(t => t.DeleteTagAsync(It.IsAny<int>(), It.IsAny<int?>()), Times.Never);

        cut.Find("[data-action=confirm]").Click();

        _tags.Verify(t => t.DeleteTagAsync(1, It.IsAny<int?>()), Times.Once);
    }

    [Test]
    public void Sorting_by_tickets_puts_the_busiest_tag_last_then_first()
    {
        var cut = Render();

        cut.Find("button[data-sort=count]").Click();
        cut.FindAll("tr[data-row]").Select(r => r.GetAttribute("data-row")).Should().Equal("2", "1");

        cut.Find("button[data-sort=count]").Click();
        cut.FindAll("tr[data-row]").Select(r => r.GetAttribute("data-row")).Should().Equal("1", "2");
    }
}
