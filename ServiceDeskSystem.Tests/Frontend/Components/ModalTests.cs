using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using ServiceDeskSystem.Components.Shared;

namespace ServiceDeskSystem.Tests.Frontend.Components;

[TestFixture]
public class ModalTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void RendersNothingWhenClosed()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, false)
            .Add(m => m.Title, "Edit policy"));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Test]
    public void RendersTitleAndSubtitleWhenOpen()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.Title, "Edit policy")
            .Add(m => m.Subtitle, "SLA Policies"));

        cut.Find(".amodal-title").TextContent.Should().Be("Edit policy");
        cut.Find(".amodal-subtitle").TextContent.Should().Be("SLA Policies");
    }

    [Test]
    public void ShowsErrorMessageOnlyWhenProvided()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.Title, "Edit policy"));

        cut.FindAll(".amodal-error").Should().BeEmpty();

        cut.SetParametersAndRender(p => p.Add(m => m.ErrorMessage, "Policy already exists"));

        cut.Find(".amodal-error").TextContent.Should().Contain("Policy already exists");
    }

    [Test]
    public void SaveButtonIsDisabledAndShowsSavingTextWhileSaving()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.IsSaving, true)
            .Add(m => m.SaveText, "Save")
            .Add(m => m.SavingText, "Saving..."));

        var save = cut.Find("button.amodal-btn-save");
        save.HasAttribute("disabled").Should().BeTrue();
        save.TextContent.Should().Contain("Saving...");
        save.TextContent.Should().NotContain("Save ");
    }

    [Test]
    public void InvokesCallbacksForSaveAndCancel()
    {
        var saved = false;
        var closed = false;

        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.OnSave, EventCallback.Factory.Create(this, () => saved = true))
            .Add(m => m.OnClose, EventCallback.Factory.Create(this, () => closed = true)));

        cut.Find("button.amodal-btn-save").Click();
        cut.Find("button.amodal-btn-cancel").Click();

        saved.Should().BeTrue();
        closed.Should().BeTrue();
    }

    [Test]
    public void ClosesOnOverlayClickOnlyWhenEnabled()
    {
        var closed = 0;

        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.CloseOnOverlayClick, false)
            .Add(m => m.OnClose, EventCallback.Factory.Create(this, () => closed++)));

        cut.Find(".amodal-overlay").Click();
        closed.Should().Be(0);

        cut.SetParametersAndRender(p => p.Add(m => m.CloseOnOverlayClick, true));
        cut.Find(".amodal-overlay").Click();
        closed.Should().Be(1);
    }

    [Test]
    public void EscapeClosesEvenWhenOverlayClickIsDisabled()
    {
        var closed = 0;

        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.CloseOnOverlayClick, false)
            .Add(m => m.OnClose, EventCallback.Factory.Create(this, () => closed++)));

        cut.Find(".amodal-overlay").KeyDown(key: "Enter");
        closed.Should().Be(0);

        cut.Find(".amodal-overlay").KeyDown(key: "Escape");
        closed.Should().Be(1);
    }

    [Test]
    public void EscapeIsIgnoredWhileSaving()
    {
        var closed = 0;

        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.IsSaving, true)
            .Add(m => m.OnClose, EventCallback.Factory.Create(this, () => closed++)));

        cut.Find(".amodal-overlay").KeyDown(key: "Escape");

        closed.Should().Be(0);
    }

    [Test]
    public void ErrorIsAnnouncedAsAlert()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.ErrorMessage, "Policy already exists"));

        cut.Find(".amodal-error").GetAttribute("role").Should().Be("alert");
    }

    [Test]
    public void SaveUsesAccentByDefaultAndDangerWhenRequested()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p.Add(m => m.IsOpen, true));

        cut.Find("button.amodal-btn-save").ClassList.Should().Contain("btn-accent").And.NotContain("btn-danger");

        cut.SetParametersAndRender(p => p.Add(m => m.Danger, true));

        cut.Find("button.amodal-btn-save").ClassList.Should().Contain("btn-danger").And.NotContain("btn-accent");
    }

    [Test]
    public void HasNoIconsAndALabelledCloseButton()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.Title, "Edit policy"));

        cut.FindAll("svg").Should().BeEmpty();
        cut.Find(".amodal-close").GetAttribute("aria-label").Should().Be("common.close");
    }
}
