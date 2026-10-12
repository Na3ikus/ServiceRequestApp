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

        cut.Find("[data-modal-title]").TextContent.Should().Be("Edit policy");
        cut.Find("[data-modal-subtitle]").TextContent.Should().Be("SLA Policies");
    }

    [Test]
    public void ShowsErrorMessageOnlyWhenProvided()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.Title, "Edit policy"));

        cut.FindAll("[data-modal-error]").Should().BeEmpty();

        cut.SetParametersAndRender(p => p.Add(m => m.ErrorMessage, "Policy already exists"));

        cut.Find("[data-modal-error]").TextContent.Should().Contain("Policy already exists");
    }

    [Test]
    public void SaveButtonIsDisabledAndShowsSavingTextWhileSaving()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.IsSaving, true)
            .Add(m => m.SaveText, "Save")
            .Add(m => m.SavingText, "Saving..."));

        var save = cut.Find("button[data-modal-save]");
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

        cut.Find("button[data-modal-save]").Click();
        cut.Find("button[data-modal-cancel]").Click();

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

        cut.Find("[data-modal]").Click();
        closed.Should().Be(0);

        cut.SetParametersAndRender(p => p.Add(m => m.CloseOnOverlayClick, true));
        cut.Find("[data-modal]").Click();
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

        cut.Find("[data-modal]").KeyDown(key: "Enter");
        closed.Should().Be(0);

        cut.Find("[data-modal]").KeyDown(key: "Escape");
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

        cut.Find("[data-modal]").KeyDown(key: "Escape");

        closed.Should().Be(0);
    }

    [Test]
    public void ErrorIsAnnouncedAsAlert()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.ErrorMessage, "Policy already exists"));

        cut.Find("[data-modal-error]").GetAttribute("role").Should().Be("alert");
    }

    [Test]
    public void SaveUsesAccentByDefaultAndDangerWhenRequested()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p.Add(m => m.IsOpen, true));

        cut.Find("button[data-modal-save]").ClassList.Should().Contain("btn-accent").And.NotContain("btn-danger");

        cut.SetParametersAndRender(p => p.Add(m => m.Danger, true));

        cut.Find("button[data-modal-save]").ClassList.Should().Contain("btn-danger").And.NotContain("btn-accent");
    }

    [Test]
    public void HasNoIconsAndALabelledCloseButton()
    {
        var cut = _host.Context.RenderComponent<Modal>(p => p
            .Add(m => m.IsOpen, true)
            .Add(m => m.Title, "Edit policy"));

        cut.FindAll("svg").Should().BeEmpty();
        cut.Find("[data-modal-close]").GetAttribute("aria-label").Should().Be("common.close");
    }
}
