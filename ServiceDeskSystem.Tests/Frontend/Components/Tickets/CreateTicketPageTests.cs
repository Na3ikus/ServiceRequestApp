using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Components.Pages.Tickets;
using ServiceDeskSystem.Components.UI.Elements;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Tests.Frontend.Components.Tickets;

[TestFixture]
public class CreateTicketPageTests
{
    private ComponentTestHost _host;
    private Mock<ITicketService> _tickets;
    private Ticket? _created;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _tickets = new Mock<ITicketService>();
        _tickets.Setup(t => t.GetProductsAsync()).ReturnsAsync([new Product { Id = 3, Name = "Accounting Pro" }]);
        _tickets.Setup(t => t.CreateTicketAsync(It.IsAny<Ticket>()))
            .ReturnsAsync((Ticket t) =>
            {
                t.Id = 77;
                _created = t;
                return t;
            });
        _host.Context.Services.AddSingleton(_tickets.Object);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private void SignIn(UserRole role) =>
        _host.Auth.SetupGet(a => a.CurrentUser).Returns(new User { Id = 5, Login = "olena", Role = role });

    private IRenderedComponent<CreateTicket> Render() => _host.Context.RenderComponent<CreateTicket>();

    private static void PickType(IRenderedComponent<CreateTicket> cut, string type) =>
        cut.Find($"[data-field=type] button[role=radio]:nth-child({Array.IndexOf(["Bug", "Support", "Consultation", "Project"], type) + 1})").Click();

    private static void FillTitleAndDescription(IRenderedComponent<CreateTicket> cut)
    {
        cut.Find("#create-title").Input("Monthly report is empty");
        var editor = cut.FindComponents<RichTextEditor>()[0];
        cut.InvokeAsync(() => editor.Instance.UpdateContent("<p>Blank screen after 2.4</p>")).GetAwaiter().GetResult();
    }

    private string CurrentUri => _host.Context.Services.GetRequiredService<FakeNavigationManager>().Uri;

    [Test]
    public void Has_a_path_header_one_form_and_no_hero()
    {
        SignIn(UserRole.User);

        var cut = Render();

        cut.Find("h1").TextContent.Should().Be("~/tickets/new");
        cut.Markup.Should().Contain("create.subtitle").And.NotContain("page-hero").And.NotContain("gradient");
        cut.FindAll("[data-field=type] button[role=radio]").Select(b => b.TextContent.Trim())
            .Should().Equal("tickettype.bug", "tickettype.support", "tickettype.consultation", "tickettype.project");
        cut.FindAll("[data-field=type] [aria-checked=true]").Should().BeEmpty("no type is chosen by default");
    }

    [Test]
    public void Submitting_without_a_type_shows_a_message_and_creates_nothing()
    {
        SignIn(UserRole.User);
        var cut = Render();
        FillTitleAndDescription(cut);
        cut.Find("#create-product").Change("3");

        cut.Find("form").Submit();

        cut.Find("[data-error=type]").TextContent.Should().Be("create.typeRequired");
        cut.Find("[data-field=type] [role=radiogroup]").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("[data-field=type] [role=radiogroup]").GetAttribute("aria-describedby").Should().Be(cut.Find("[data-error=type]").Id);
        _tickets.Verify(t => t.CreateTicketAsync(It.IsAny<Ticket>()), Times.Never);

        PickType(cut, "Support");

        cut.FindAll("[data-error=type]").Should().BeEmpty();
        cut.Find("[data-type-description]").TextContent.Should().Be("ticketType.support.desc");
    }

    [Test]
    public void Empty_form_reports_every_required_field()
    {
        SignIn(UserRole.User);
        var cut = Render();

        cut.Find("form").Submit();

        cut.FindAll("[data-error]").Select(e => e.TextContent).Should().BeEquivalentTo(
            "create.typeRequired", "create.titleRequired", "create.descriptionRequired", "create.productRequired");
        cut.Find("#create-title").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Test]
    public void Bug_details_appear_only_for_bugs()
    {
        SignIn(UserRole.User);
        var cut = Render();

        cut.FindAll("[data-bug-details]").Should().BeEmpty();

        PickType(cut, "Bug");
        cut.FindAll("[data-bug-details]").Should().ContainSingle();

        PickType(cut, "Consultation");
        cut.FindAll("[data-bug-details]").Should().BeEmpty();
    }

    [Test]
    public void Product_is_hidden_for_projects()
    {
        SignIn(UserRole.User);
        var cut = Render();

        cut.FindAll("[data-field=product]").Should().ContainSingle();

        PickType(cut, "Project");

        cut.FindAll("[data-field=product]").Should().BeEmpty();
    }

    [Test]
    public void Priority_is_only_for_staff()
    {
        SignIn(UserRole.User);
        Render().FindAll("[data-field=priority]").Should().BeEmpty();

        SignIn(UserRole.Developer);
        Render().FindAll("[data-field=priority]").Should().ContainSingle();
    }

    [Test]
    public void Critical_priority_needs_the_confirmation()
    {
        SignIn(UserRole.Admin);
        var cut = Render();
        PickType(cut, "Bug");
        FillTitleAndDescription(cut);
        cut.Find("#create-product").Change("3");
        cut.Find("#create-priority").Change("Critical");

        cut.Find("[data-critical]").ClassList.Should().Contain("border-danger");
        cut.Find("form").Submit();

        cut.Find("[data-critical] [role=alert]").TextContent.Should().Be("create.criticalRequired");
        _tickets.Verify(t => t.CreateTicketAsync(It.IsAny<Ticket>()), Times.Never);

        cut.Find("[data-critical] input[type=checkbox]").Change(true);
        cut.Find("form").Submit();

        _created!.Priority.Should().Be(TicketPriority.Critical);
        _created.IsPriorityAssessed.Should().BeTrue();
    }

    [Test]
    public void Creating_goes_to_the_new_ticket()
    {
        SignIn(UserRole.User);
        var cut = Render();
        PickType(cut, "Bug");
        FillTitleAndDescription(cut);
        cut.Find("#create-product").Change("3");
        cut.Find("#create-version").Input("2.4.0");

        cut.Find("[data-preview-title]").TextContent.Should().Be("Monthly report is empty");
        cut.Find("form").Submit();

        CurrentUri.Should().EndWith("/ticket/77");
        _created!.Type.Should().Be(TicketType.Bug);
        _created.ProductId.Should().Be(3);
        _created.AffectedVersion.Should().Be("2.4.0");
        _created.Priority.Should().Be(TicketPriority.Medium);
        _created.IsPriorityAssessed.Should().BeFalse("users leave the priority to triage");
        _created.AuthorId.Should().Be(5);
    }

    [Test]
    public void Project_is_created_without_a_product()
    {
        SignIn(UserRole.Developer);
        var cut = Render();
        PickType(cut, "Project");
        FillTitleAndDescription(cut);

        cut.Find("form").Submit();

        _created!.Type.Should().Be(TicketType.Project);
        _created.ProductId.Should().BeNull();
        CurrentUri.Should().EndWith("/ticket/77");
    }

    [Test]
    public void Cancel_goes_home()
    {
        SignIn(UserRole.User);
        var cut = Render();

        cut.Find("[data-cancel]").Click();

        CurrentUri.Should().Be("http://localhost/");
    }
}
