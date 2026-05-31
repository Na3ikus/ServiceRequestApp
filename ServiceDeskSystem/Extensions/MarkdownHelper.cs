using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace ServiceDeskSystem.Extensions;

/// <summary>
/// A lightweight Markdown helper for formatting chat messages and ticket descriptions.
/// Protects against XSS by HTML-encoding first, then applying specific formatting rules.
/// </summary>
public static class MarkdownHelper
{
    public static MarkupString Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new MarkupString(string.Empty);
        }

        // 1. Always HTML encode first to prevent XSS
        var encoded = HtmlEncoder.Default.Encode(text);

        // 2. Bold: **text**
        encoded = Regex.Replace(encoded, @"\*\*(.+?)\*\*", "<strong class=\"font-bold text-gray-900 dark:text-white\">$1</strong>", RegexOptions.Singleline);

        // 3. Italic: *text* or _text_
        encoded = Regex.Replace(encoded, @"\*(.+?)\*", "<em class=\"italic\">$1</em>", RegexOptions.Singleline);
        encoded = Regex.Replace(encoded, @"_(.+?)_", "<em class=\"italic\">$1</em>", RegexOptions.Singleline);

        // 4. Inline Code: `code`
        encoded = Regex.Replace(encoded, @"\`(.+?)\`", "<code class=\"px-1.5 py-0.5 rounded-md bg-gray-100 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 font-mono text-sm text-pink-600 dark:text-pink-400\">$1</code>", RegexOptions.Singleline);

        // 5. Links: [text](url)
        encoded = Regex.Replace(encoded, @"\[(.+?)\]\((.+?)\)", "<a href=\"$2\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"text-blue-600 dark:text-blue-400 hover:underline font-medium\">$1</a>", RegexOptions.Singleline);

        // 6. Blockquotes: > text
        encoded = Regex.Replace(encoded, @"^&gt;\s+(.+)$", "<blockquote class=\"border-l-4 border-gray-300 dark:border-gray-600 pl-4 py-1 my-2 text-gray-600 dark:text-gray-400 italic bg-gray-50 dark:bg-gray-800/50 rounded-r-lg\">$1</blockquote>", RegexOptions.Multiline);

        // 7. Newlines to <br />
        encoded = encoded.Replace("\n", "<br />", StringComparison.Ordinal);

        return new MarkupString(encoded);
    }
}
