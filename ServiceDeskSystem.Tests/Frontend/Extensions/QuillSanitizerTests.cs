using FluentAssertions;
using NUnit.Framework;
using ServiceDeskSystem.Extensions;

namespace ServiceDeskSystem.Tests.Frontend.Extensions;

[TestFixture]
public class QuillSanitizerTests
{
    private static string Sanitize(string html) => QuillSanitizer.Sanitize(html).Value;

    [Test]
    public void Keeps_quill_formatting()
    {
        Sanitize("<p class=\"ql-align-center\"><strong>bold</strong> &amp; <em>it</em></p><ul><li>one</li></ul>")
            .Should().Be("<p class=\"ql-align-center\"><strong>bold</strong> &amp; <em>it</em></p><ul><li>one</li></ul>");
    }

    [TestCase("<img src=x onerror=alert(1)>")]
    [TestCase("<script>alert(1)</script>")]
    [TestCase("<svg onload=alert(1)>")]
    [TestCase("<iframe src=\"javascript:alert(1)\"></iframe>")]
    public void Drops_tags_off_the_list(string html)
    {
        Sanitize(html).Should().NotContain("<img").And.NotContain("<script").And.NotContain("<svg").And.NotContain("<iframe");
    }

    [TestCase("<<x>img src=x onerror=alert(1)>")]
    [TestCase("<<script>script>alert(1)<</script>/script>")]
    [TestCase("<<b>img src=x onerror=alert(1)>")]
    public void A_removed_tag_cannot_glue_the_text_around_it_into_a_new_tag(string html)
    {
        var result = Sanitize(html);

        result.Should().NotContain("<img").And.NotContain("<script");
        result.Should().Contain("&lt;");
    }

    [Test]
    public void Drops_event_handlers_and_styles()
    {
        Sanitize("<p onclick=\"alert(1)\" style=\"position:fixed\">x</p>").Should().Be("<p>x</p>");
    }

    [Test]
    public void Keeps_only_quill_classes()
    {
        Sanitize("<div class=\"fixed inset-0 z-50 ql-indent-1\">x</div>").Should().Be("<div class=\"ql-indent-1\">x</div>");
        Sanitize("<div class=\"fixed inset-0\">x</div>").Should().Be("<div>x</div>");
    }

    [TestCase("javascript:alert(1)")]
    [TestCase(" JaVaScRiPt:alert(1)")]
    [TestCase("data:text/html;base64,PHNjcmlwdD4=")]
    public void Drops_unsafe_links(string href)
    {
        Sanitize($"<a href=\"{href}\">x</a>").Should().Be("<a>x</a>");
    }

    [Test]
    public void Safe_links_open_in_a_new_tab()
    {
        Sanitize("<a href=\"https://example.com\">x</a>")
            .Should().Be("<a href=\"https://example.com\" target=\"_blank\" rel=\"noopener noreferrer\">x</a>");
    }
}
