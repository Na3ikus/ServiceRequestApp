using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ServiceDeskSystem.Components.Pages.Auth;

namespace ServiceDeskSystem.Tests.Frontend.Components.Auth;

[TestFixture]
public class LoginTests
{
    private ComponentTestHost _host;

    [SetUp]
    public void SetUp()
    {
        _host = new ComponentTestHost();

        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns("Development");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AuthSettings:ShowDemoAccounts"] = "true",
            })
            .Build();

        var httpContextAccessor = new Mock<IHttpContextAccessor>();

        _host.Context.Services.AddSingleton(env.Object);
        _host.Context.Services.AddSingleton<IConfiguration>(config);
        _host.Context.Services.AddSingleton(httpContextAccessor.Object);

        _host.Auth.SetupGet(a => a.IsAuthenticated).Returns(false);
    }

    [TearDown]
    public void TearDown() => _host.Dispose();

    [Test]
    public void Demo_rows_render_role_tags_when_enabled()
    {
        var cut = _host.Context.RenderComponent<Login>();

        var roleTags = cut.FindAll("[data-tone]").ToList()
            .Where(t => t.TextContent.StartsWith("admin.role", StringComparison.Ordinal))
            .ToList();
        roleTags.Should().HaveCount(3);

        cut.Markup.Should().Contain("admin");
        cut.Markup.Should().Contain("o.kovalenko");
        cut.Markup.Should().Contain("m.shevchenko");
    }
}
