using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Extensions;

/// <summary>
/// Sanitizes HTML produced by the Quill rich-text editor before rendering it in Blazor.
/// Only a strict whitelist of tags and attributes is allowed; everything else is stripped.
/// This prevents XSS while preserving Quill's formatting output.
/// </summary>
public static partial class QuillSanitizer
{
    private static readonly HashSet<string> AllowedTags = new (StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "hr",
        "strong", "b", "em", "i", "u", "s", "strike", "code", "pre",
        "h1", "h2", "h3", "h4",
        "blockquote",
        "ul", "ol", "li",
        "a",
        "span", "div",
    };

    // Only these attributes are ever kept (on any tag)
    private static readonly HashSet<string> AllowedAttributes = new (StringComparer.OrdinalIgnoreCase)
    {
        "class",
    };

    /// <summary>
    /// Sanitizes a Quill HTML string and returns a safe <see cref="MarkupString"/> for Blazor rendering.
    /// </summary>
    /// <param name="html">The raw HTML string from Quill editor.</param>
    /// <returns>A sanitized <see cref="MarkupString"/>.</returns>
    public static MarkupString Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return new MarkupString(string.Empty);
        }

        // Text between tags is escaped, so dropping a tag can never join the text around it into a
        // new one: "<<x>img src=x onerror=...>" must not come out as a live <img>.
        var result = new StringBuilder(html.Length);
        var position = 0;
        foreach (Match match in TagPattern().Matches(html))
        {
            result.Append(EscapeText(html[position..match.Index]));
            result.Append(SanitizeTag(match));
            position = match.Index + match.Length;
        }

        result.Append(EscapeText(html[position..]));
        return new MarkupString(result.ToString());
    }

    // Entities such as &amp; stay as they are; only the characters that could open or close a tag change.
    private static string EscapeText(string text) =>
        text.Replace("<", "&lt;", StringComparison.Ordinal).Replace(">", "&gt;", StringComparison.Ordinal);

    [GeneratedRegex(@"<(/?)(\w+)([^>]*)(/?)>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TagPattern();

    [GeneratedRegex("""(\w[\w-]*)(?:\s*=\s*(?:"([^"]*)"|'([^']*)'|(\S+)))?""", RegexOptions.IgnoreCase)]
    private static partial Regex AttrPattern();

    [GeneratedRegex("""href\s*=\s*(?:"([^"]*)"|'([^']*)'|(\S+))""", RegexOptions.IgnoreCase)]
    private static partial Regex HrefPattern();

    [GeneratedRegex(@"^ql-[a-z0-9-]+$")]
    private static partial Regex QuillClassPattern();

    private static string SanitizeTag(Match match)
    {
        var slash = match.Groups[1].Value;
        var tagName = match.Groups[2].Value;
        var attrString = match.Groups[3].Value;
        var selfClose = match.Groups[4].Value;

        // Drop any tag not on the whitelist
        if (!AllowedTags.Contains(tagName))
        {
            return string.Empty;
        }

        // Closing tags have no attributes
        if (slash == "/")
        {
            return $"</{tagName}>";
        }

        var sanitizedAttrs = SanitizeAttributes(tagName, attrString);
        var sc = selfClose == "/" ? " /" : string.Empty;

        return string.IsNullOrEmpty(sanitizedAttrs)
            ? $"<{tagName}{sc}>"
            : $"<{tagName} {sanitizedAttrs}{sc}>";
    }

    private static string SanitizeAttributes(string tagName, string attrString)
    {
        if (string.IsNullOrWhiteSpace(attrString))
        {
            return string.Empty;
        }

        var kept = new List<string>();

        // Handle <a href="..."> specially — validate the URL
        if (tagName.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            var hrefMatch = HrefPattern().Match(attrString);
            if (hrefMatch.Success)
            {
                var href = hrefMatch.Groups[1].Success ? hrefMatch.Groups[1].Value
                         : hrefMatch.Groups[2].Success ? hrefMatch.Groups[2].Value
                         : hrefMatch.Groups[3].Value;

                if (IsSafeUrl(href))
                {
                    kept.Add($"""href="{HtmlEncoder.Default.Encode(href)}" target="_blank" rel="noopener noreferrer" """);
                }
            }
        }

        // Keep whitelisted attributes (e.g. class for Quill styles)
        foreach (Match attr in AttrPattern().Matches(attrString))
        {
            var name = attr.Groups[1].Value;
            if (!AllowedAttributes.Contains(name))
            {
                continue;
            }

            var value = attr.Groups[2].Success ? attr.Groups[2].Value
                      : attr.Groups[3].Success ? attr.Groups[3].Value
                      : attr.Groups[4].Value;

            // Only Quill's own classes: app classes (fixed inset-0 z-50 ...) could cover the page.
            var quillClasses = value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(c => QuillClassPattern().IsMatch(c));
            var classes = string.Join(' ', quillClasses);
            if (classes.Length > 0)
            {
                kept.Add($"""{name}="{HtmlEncoder.Default.Encode(classes)}" """);
            }
        }

        return string.Join(string.Empty, kept).TrimEnd();
    }

    /// <summary>Returns true only for http/https/mailto/relative URLs.</summary>
    private static bool IsSafeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        // Reject javascript:, data:, vbscript: and similar
        var trimmed = url.TrimStart().ToUpperInvariant();
        return trimmed.StartsWith("HTTP://", StringComparison.Ordinal)
            || trimmed.StartsWith("HTTPS://", StringComparison.Ordinal)
            || trimmed.StartsWith("MAILTO:", StringComparison.Ordinal)
            || trimmed.StartsWith('/')
            || trimmed.StartsWith('#');
    }
}
