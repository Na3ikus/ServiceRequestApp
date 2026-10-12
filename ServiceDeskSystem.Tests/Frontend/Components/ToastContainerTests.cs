using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI;

namespace ServiceDeskSystem.Tests.Frontend.Components;

[TestFixture]
public class ToastContainerTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp() => _host = new ComponentTestHost();

    [TearDown]
    public void TearDown() => _host.Dispose();

    [TestCase(ToastType.Success, "ok")]
    [TestCase(ToastType.Error, "err")]
    [TestCase(ToastType.Warning, "warn")]
    [TestCase(ToastType.Info, "info")]
    public void Shows_mono_label_per_type(ToastType type, string label)
    {
        var toasts = new List<ToastMessage> { new() { Message = "Saved", Type = type } };

        var cut = _host.Context.RenderComponent<ToastContainer>(p => p.Add(t => t.Toasts, toasts));

        cut.Find("[data-toast-label]").TextContent.Should().Be(label);
        cut.Find("[data-toast-message]").TextContent.Should().Be("Saved");
        cut.FindAll("svg").Should().BeEmpty();
    }

    [TestCase(ToastType.Success, "text-accent-ink")]
    [TestCase(ToastType.Error, "text-danger")]
    [TestCase(ToastType.Warning, "text-danger")]
    [TestCase(ToastType.Info, "text-muted")]
    public void Label_uses_token_colour_per_type(ToastType type, string colour)
    {
        var toasts = new List<ToastMessage> { new() { Message = "Saved", Type = type } };

        var cut = _host.Context.RenderComponent<ToastContainer>(p => p.Add(t => t.Toasts, toasts));

        cut.Find("[data-toast-label]").ClassList.Should().Contain(colour);
    }

    [Test]
    public void Container_is_a_polite_live_region()
    {
        var cut = _host.Context.RenderComponent<ToastContainer>(p => p.Add(t => t.Toasts, new List<ToastMessage>()));

        cut.Find(".toast-container").GetAttribute("aria-live").Should().Be("polite");
    }

    [Test]
    public void Close_button_removes_that_toast()
    {
        var toast = new ToastMessage { Message = "Saved", Type = ToastType.Success };
        ToastMessage? removed = null;

        var cut = _host.Context.RenderComponent<ToastContainer>(p => p
            .Add(t => t.Toasts, new List<ToastMessage> { toast })
            .Add(t => t.OnRemoveToast, EventCallback.Factory.Create<ToastMessage>(this, m => removed = m)));

        var close = cut.Find("[data-toast-close]");
        close.GetAttribute("aria-label").Should().Be("common.close");
        close.Click();

        removed.Should().BeSameAs(toast);
    }
}
