using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Components.Pages.Admin.Components;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Tests.Frontend.Components.Admin;

/// <summary>
/// EditRow and DeletePrompt behaviour, exercised through the products and tech stacks sections.
/// </summary>
[TestFixture]
public class AdminRowEditingTests
{
    private ComponentTestHost _host;
    private Mock<IAdminService> _admin;
    private int _changed;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();
        _admin = new Mock<IAdminService>();
        _host.Context.Services.AddSingleton(_admin.Object);
        _changed = 0;
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    private static List<TechStack> Stacks() =>
    [
        new TechStack { Id = 1, Name = ".NET", Type = "Backend", Products = [new Product { Id = 1 }] },
        new TechStack { Id = 2, Name = "Vue", Type = "Frontend" },
    ];

    private static List<Product> Products() =>
    [
        new Product { Id = 1, Name = "Billing", Description = "Payments", CurrentVersion = "2.4.1", TechStackId = 1, Tickets = [new Ticket { Id = 9 }] },
        new Product { Id = 2, Name = "Reports", Description = "Exports", CurrentVersion = "0.8.0", TechStackId = 1 },
    ];

    private IRenderedComponent<ProductsTab> RenderProducts() =>
        _host.Context.RenderComponent<ProductsTab>(p => p
            .Add(c => c.Products, Products())
            .Add(c => c.TechStacks, Stacks())
            .Add(c => c.OnProductsChanged, () => _changed++));

    private IRenderedComponent<TechStacksTab> RenderStacks() =>
        _host.Context.RenderComponent<TechStacksTab>(p => p
            .Add(c => c.TechStacks, Stacks())
            .Add(c => c.OnTechStacksChanged, () => _changed++));

    [Test]
    public void Edit_opens_the_row_with_the_record_values_and_focuses_the_first_field()
    {
        var cut = RenderProducts();

        cut.Find("tr[data-row='2'] [data-action=edit]").Click();

        cut.FindAll("[data-edit-row]").Should().ContainSingle();
        cut.FindAll("tr[data-row='2']").Should().BeEmpty();
        cut.Find("#product-name").GetAttribute("value").Should().Be("Reports");
        cut.Find("#product-version").GetAttribute("value").Should().Be("0.8.0");
        _host.Context.JSInterop.VerifyFocusAsyncInvoke();
    }

    [Test]
    public void Escape_closes_the_row_without_saving()
    {
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=edit]").Click();
        cut.Find("#product-name").Input("Changed");

        cut.Find("tr[data-edit-row]").KeyDown(key: "Escape");

        cut.FindAll("[data-edit-row]").Should().BeEmpty();
        cut.Find("tr[data-row='2']").TextContent.Should().Contain("Reports");
        _admin.Verify(a => a.UpdateProductAsync(It.IsAny<Product>()), Times.Never);
    }

    [Test]
    public void Cancel_closes_the_row_without_saving()
    {
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=edit]").Click();

        cut.Find("[data-action=cancel]").Click();

        cut.FindAll("[data-edit-row]").Should().BeEmpty();
        _admin.Verify(a => a.UpdateProductAsync(It.IsAny<Product>()), Times.Never);
    }

    [Test]
    public void Other_keys_do_not_close_the_row()
    {
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=edit]").Click();

        cut.Find("tr[data-edit-row]").KeyDown(key: "a");

        cut.FindAll("[data-edit-row]").Should().ContainSingle();
    }

    [Test]
    public void Enter_submits_the_form_and_saves()
    {
        _admin.Setup(a => a.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync(true);
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=edit]").Click();
        cut.Find("#product-name").Input("Reports v2");

        // Enter in a single-line input submits the form; save is the form's submit button.
        cut.Find("[data-action=save]").GetAttribute("type").Should().Be("submit");
        cut.Find("[data-edit-row] form").Submit();

        _admin.Verify(a => a.UpdateProductAsync(It.Is<Product>(p => p.Id == 2 && p.Name == "Reports v2")), Times.Once);
        cut.FindAll("[data-edit-row]").Should().BeEmpty();
        _changed.Should().Be(1);
    }

    [Test]
    public void A_failed_save_keeps_the_row_and_its_values_and_shows_the_error()
    {
        _admin.Setup(a => a.UpdateProductAsync(It.IsAny<Product>())).ThrowsAsync(new InvalidOperationException("db down"));
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=edit]").Click();
        cut.Find("#product-name").Input("Reports v2");

        cut.Find("[data-edit-row] form").Submit();

        cut.FindAll("[data-edit-row]").Should().ContainSingle();
        cut.Find("[data-edit-error]").TextContent.Should().Contain("db down");
        cut.Find("#product-name").GetAttribute("value").Should().Be("Reports v2");
        _changed.Should().Be(0);
    }

    [Test]
    public void An_empty_name_is_rejected_in_the_row()
    {
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=edit]").Click();
        cut.Find("#product-name").Input(string.Empty);

        cut.Find("[data-edit-row] form").Submit();

        cut.Find("[data-edit-error]").TextContent.Should().Contain("admin.nameRequired");
        _admin.Verify(a => a.UpdateProductAsync(It.IsAny<Product>()), Times.Never);
    }

    [Test]
    public void Opening_another_row_closes_the_first_and_drops_its_changes()
    {
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=edit]").Click();
        cut.Find("#product-name").Input("Unsaved");

        cut.Find("tr[data-row='1'] [data-action=edit]").Click();

        cut.FindAll("[data-edit-row]").Should().ContainSingle();
        cut.Find("#product-name").GetAttribute("value").Should().Be("Billing");
        cut.Find("tr[data-row='2']").TextContent.Should().Contain("Reports");
    }

    [Test]
    public void Add_opens_an_empty_row_at_the_top_and_closes_an_open_prompt()
    {
        _admin.Setup(a => a.CreateProductAsync(It.IsAny<Product>())).ReturnsAsync(new Product());
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=delete]").Click();

        cut.Find("[data-action=add]").Click();

        cut.FindAll("[data-delete-prompt]").Should().BeEmpty();
        cut.Find("tbody > tr").HasAttribute("data-edit-row").Should().BeTrue();
        cut.Find("#product-name").GetAttribute("value").Should().BeNullOrEmpty();

        cut.Find("#product-name").Input("New one");
        cut.Find("[data-edit-row] form").Submit();

        _admin.Verify(a => a.CreateProductAsync(It.Is<Product>(p => p.Name == "New one" && p.TechStackId == 1)), Times.Once);
    }

    [Test]
    public void Delete_shows_the_prompt_without_calling_the_service()
    {
        var cut = RenderProducts();

        cut.Find("tr[data-row='2'] [data-action=delete]").Click();

        var prompt = cut.Find("[data-delete-prompt]");
        prompt.TextContent.Should().Contain("rm").And.Contain("product").And.Contain("\"Reports\"");
        prompt.TextContent.Should().Contain("admin.deletenote");
        cut.FindAll("tr[data-row='2']").Should().BeEmpty();
        _admin.Verify(a => a.DeleteProductAsync(It.IsAny<int>()), Times.Never);
        _host.Context.JSInterop.VerifyFocusAsyncInvoke();
    }

    [Test]
    public void N_closes_the_prompt()
    {
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=delete]").Click();

        cut.Find("[data-action=keep]").Click();

        cut.FindAll("[data-delete-prompt]").Should().BeEmpty();
        cut.FindAll("tr[data-row='2']").Should().ContainSingle();
        _admin.Verify(a => a.DeleteProductAsync(It.IsAny<int>()), Times.Never);
    }

    [TestCase("n")]
    [TestCase("Escape")]
    public void N_and_escape_keys_close_the_prompt(string key)
    {
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=delete]").Click();

        cut.Find("tr[data-delete-prompt]").KeyDown(key: key);

        cut.FindAll("[data-delete-prompt]").Should().BeEmpty();
        _admin.Verify(a => a.DeleteProductAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void Enter_on_the_prompt_does_not_delete()
    {
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=delete]").Click();

        cut.Find("tr[data-delete-prompt]").KeyDown(key: "Enter");

        _admin.Verify(a => a.DeleteProductAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void Y_calls_the_service_once()
    {
        _admin.Setup(a => a.DeleteProductAsync(2)).ReturnsAsync(true);
        var cut = RenderProducts();
        cut.Find("tr[data-row='2'] [data-action=delete]").Click();

        cut.Find("[data-action=confirm]").Click();

        _admin.Verify(a => a.DeleteProductAsync(2), Times.Once);
        cut.FindAll("[data-delete-prompt]").Should().BeEmpty();
        _changed.Should().Be(1);
    }

    [Test]
    public void Y_key_confirms_the_delete()
    {
        _admin.Setup(a => a.DeleteTechStackAsync(2)).ReturnsAsync(true);
        var cut = RenderStacks();
        cut.Find("tr[data-row='2'] [data-action=delete]").Click();

        cut.Find("tr[data-delete-prompt]").KeyDown(key: "y");

        _admin.Verify(a => a.DeleteTechStackAsync(2), Times.Once);
    }

    [Test]
    public void Delete_is_disabled_for_a_product_with_tickets()
    {
        var cut = RenderProducts();

        var button = cut.Find("tr[data-row='1'] [data-action=delete]");
        button.HasAttribute("disabled").Should().BeTrue();
        button.GetAttribute("title").Should().Be("admin.cannotDeleteProductWithTickets");
        cut.Find("tr[data-row='2'] [data-action=delete]").HasAttribute("disabled").Should().BeFalse();
    }

    [Test]
    public void Delete_is_disabled_for_a_stack_with_products()
    {
        var cut = RenderStacks();

        var button = cut.Find("tr[data-row='1'] [data-action=delete]");
        button.HasAttribute("disabled").Should().BeTrue();
        button.GetAttribute("title").Should().Be("admin.cannotDeleteTechStackWithProducts");
        cut.Find("tr[data-row='2'] [data-action=delete]").HasAttribute("disabled").Should().BeFalse();
    }

    [Test]
    public void Sort_header_flips_direction_and_sets_aria_sort()
    {
        var cut = RenderProducts();

        cut.Find("button[data-sort=name]").ParentElement!.GetAttribute("aria-sort").Should().Be("ascending");
        RowNames(cut).Should().Equal("Billing", "Reports");

        cut.Find("button[data-sort=name]").Click();

        cut.Find("button[data-sort=name]").ParentElement!.GetAttribute("aria-sort").Should().Be("descending");
        cut.Find("button[data-sort=version]").ParentElement!.GetAttribute("aria-sort").Should().Be("none");
        RowNames(cut).Should().Equal("Reports", "Billing");
    }

    [Test]
    public void Tables_opt_out_of_legacy_table_css()
    {
        RenderProducts().Find("table").HasAttribute("data-term").Should().BeTrue();
    }

    private static List<string> RowNames(IRenderedComponent<ProductsTab> cut) =>
        cut.FindAll("tr[data-row] td:first-child").Select(td => td.TextContent.Trim()).ToList();
}
