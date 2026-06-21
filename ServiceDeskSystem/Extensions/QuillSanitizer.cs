using System.Text.RegularExpressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Extensions;

/// <summary>
/// Sanitizes HTML produced by the Quill rich-text editor before rendering it in Blazor.
/// Only a strict whitelist of tags and attributes is allowed; everything else is stripped.
/// This prevents XSS while preserving Quill's formatting output.
/// </summary>
public static partial class QuillSanitizer
{
    // ── Whitelist ──────────────────────────────────────────────────────────────

    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
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
    private static readonly HashSet<string> AllowedAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "class",
    };

    // ── Compiled regexes ───────────────────────────────────────────────────────

    /// <summary>Matches any HTML tag: opening, closing, or self-closing.</summary>
    [GeneratedRegex(@"<(/?)(\w+)([^>]*)(/?)>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TagPattern();

    /// <summary>Matches individual attribute key=value pairs inside a tag.</summary>
    [GeneratedRegex("""(\w[\w-]*)(?:\s*=\s*(?:"([^"]*)"|'([^']*)'|(\S+)))?""", RegexOptions.IgnoreCase)]
    private static partial Regex AttrPattern();

    /// <summary>Matches href attribute specifically (for link sanitization).</summary>
    [GeneratedRegex("""href\s*=\s*(?:"([^"]*)"|'([^']*)'|(\S+))""", RegexOptions.IgnoreCase)]
    private static partial Regex HrefPattern();

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Sanitizes a Quill HTML string and returns a safe <see cref="MarkupString"/> for Blazor rendering.
    /// </summary>
    public static MarkupString Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return new MarkupString(string.Empty);
        }

        var result = TagPattern().Replace(html, match => SanitizeTag(match));
        return new MarkupString(result);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static string SanitizeTag(Match match)
    {
        var slash      = match.Groups[1].Value;   // "/" for closing tags
        var tagName    = match.Groups[2].Value;
        var attrString = match.Groups[3].Value;
        var selfClose  = match.Groups[4].Value;   // "/" for self-closing

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

            kept.Add($"""{name}="{HtmlEncoder.Default.Encode(value)}" """);
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
            || trimmed.StartsWith("/", StringComparison.Ordinal)
            || trimmed.StartsWith("#", StringComparison.Ordinal);
    }
}
