using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Components.Forms;
using ServiceDeskSystem.Application.Services.Attachments;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Toasts.Models;
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
    private Mock<IAttachmentService> _attachments;
    private Ticket? _created;
    private readonly List<(int TicketId, string Name, int Length)> _uploads = [];

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

        _uploads.Clear();
        _attachments = new Mock<IAttachmentService>();
        _attachments.Setup(a => a.UploadAttachmentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<int>(), null))
            .ReturnsAsync((int ticketId, string name, string _, Stream stream, int _, int? _) =>
            {
                _uploads.Add((ticketId, name, (int)stream.Length));
                return (true, (string?)null, (Attachment?)null);
            });
        _host.Context.Services.AddSingleton(_attachments.Object);
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

    private static void PickFiles(IRenderedComponent<CreateTicket> cut, params InputFileContent[] files) =>
        cut.FindComponent<InputFile>().UploadFiles(files);

    private static InputFileContent TextFile(string name, string text = "content") => InputFileContent.CreateFromText(text, name);

    private static void FillValidSupportTicket(IRenderedComponent<CreateTicket> cut)
    {
        PickType(cut, "Support");
        FillTitleAndDescription(cut);
        cut.Find("#create-product").Change("3");
    }

    [Test]
    public void Picked_files_are_listed_and_can_be_removed()
    {
        SignIn(UserRole.User);
        var cut = Render();

        PickFiles(cut, TextFile("error.log"), TextFile("screen.png"));

        cut.FindAll("[data-file]").Select(f => f.GetAttribute("data-file")).Should().Equal("error.log", "screen.png");
        cut.Find("[data-preview-files]").TextContent.Should().Be("2");

        cut.Find("[data-file='error.log'] [data-remove-file]").Click();

        cut.FindAll("[data-file]").Select(f => f.GetAttribute("data-file")).Should().Equal("screen.png");
    }

    [Test]
    public void Files_of_a_bad_type_or_over_the_size_limit_are_refused()
    {
        SignIn(UserRole.User);
        var cut = Render();

        PickFiles(
            cut,
            TextFile("setup.exe"),
            InputFileContent.CreateFromBinary(new byte[AttachmentRules.MaxFileSize + 1], "huge.zip"),
            TextFile("notes.txt"));

        cut.FindAll("[data-file]").Select(f => f.GetAttribute("data-file")).Should().Equal("notes.txt");
        var errors = cut.FindAll("[data-file-error] li").Select(e => e.TextContent).ToList();
        errors.Should().Equal("setup.exe: create.fileTypeNotAllowed", "huge.zip: details.fileTooLarge");
    }

    [Test]
    public void No_more_than_five_files_are_kept()
    {
        SignIn(UserRole.User);
        var cut = Render();

        PickFiles(cut, TextFile("1.txt"), TextFile("2.txt"), TextFile("3.txt"), TextFile("4.txt"));
        PickFiles(cut, TextFile("5.txt"), TextFile("6.txt"));

        cut.FindAll("[data-file]").Should().HaveCount(AttachmentRules.MaxFilesOnCreate);
        cut.FindAll("[data-file-error] li").Select(e => e.TextContent).Should().Equal("6.txt: create.tooManyFiles");
    }

    [Test]
    public void Files_are_uploaded_to_the_new_ticket_before_opening_it()
    {
        SignIn(UserRole.User);
        var cut = Render();
        FillValidSupportTicket(cut);
        PickFiles(cut, TextFile("error.log", "stack trace"), TextFile("notes.txt", "hi"));

        cut.Find("form").Submit();

        _uploads.Should().Equal((77, "error.log", 11), (77, "notes.txt", 2));
        CurrentUri.Should().EndWith("/ticket/77");
        _host.ToastService.Verify(t => t.ShowToastAsync(It.IsAny<string>(), ToastType.Warning), Times.Never);
    }

    [Test]
    public void A_failed_upload_still_opens_the_ticket_and_says_so()
    {
        SignIn(UserRole.User);
        _attachments.Setup(a => a.UploadAttachmentAsync(77, "error.log", It.IsAny<string>(), It.IsAny<Stream>(), 5, null))
            .ReturnsAsync((false, "disk full", (Attachment?)null));
        var cut = Render();
        FillValidSupportTicket(cut);
        PickFiles(cut, TextFile("error.log"), TextFile("notes.txt"));

        cut.Find("form").Submit();

        _uploads.Select(u => u.Name).Should().Equal("notes.txt");
        CurrentUri.Should().EndWith("/ticket/77");
        _host.ToastService.Verify(t => t.ShowToastAsync("create.filesNotAttached", ToastType.Warning), Times.Once);
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
