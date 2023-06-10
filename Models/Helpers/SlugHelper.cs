using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Notatez.Models.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string title)
    {
        // Replace spaces with hyphens
        string slug = title.Replace(" ", "-");

        // Remove any special characters or punctuation
        slug = RemoveSpecialCharacters(slug);

        // Remove consecutive hyphens
        slug = RemoveConsecutiveHyphens(slug);

        //// Remap international characters to ASCII equivalents
        //slug = RemapInternationalCharacters(slug);

        // Trim leading and trailing hyphens
        slug = slug.Trim('-');

        // Convert to lowercase
        slug = slug.ToLower();

        return slug;
    }

    private static string RemoveSpecialCharacters(string input)
    {
        string pattern = @"[^a-zA-Z0-9\-]";
        return Regex.Replace(input, pattern, "");
    }

    private static string RemoveConsecutiveHyphens(string input)
    {
        string pattern = @"-{2,}";
        return Regex.Replace(input, pattern, "-");
    }

    //private static string RemapInternationalCharacters(string input)
    //{
    //    StringBuilder sb = new StringBuilder();

    //    foreach (char c in input.Normalize(NormalizationForm.FormD))
    //    {
    //        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
    //        {
    //            sb.Append(RemapInternationalCharToAscii(c));
    //        }
    //    }

    //    return sb.ToString();
    //}

    // For edge cases I used
    // this one: https://johanbostrom.se/blog/how-to-create-a-url-and-seo-friendly-string-in-csharp-text-to-slug-generator/
    /// Remap international characters to ASCII equivalents
    // Add your custom mappings here
    // Example mappings for common characters are shown below

    //private static string RemapInternationalCharToAscii(char c)
    //{
    //    string s = c.ToString().ToLowerInvariant();
    //    switch (s)
    //    {
    //        case var _ when "àåáâäãåą".Contains(s[0]):
    //            return "a";
    //        case var _ when "èéêëę".Contains(s[0]):
    //            return "e";
    //        case var _ when "ìíîïı".Contains(s[0]):
    //            return "i";
    //        case var _ when "òóôõöøőð".Contains(s[0]):
    //            return "o";
    //        case var _ when "ùúûüŭů".Contains(s[0]):
    //            return "u";
    //        case var _ when "çćčĉ".Contains(s[0]):
    //            return "c";
    //        case var _ when "żźž".Contains(s[0]):
    //            return "z";
    //        case var _ when "śşšŝ".Contains(s[0]):
    //            return "s";
    //        case var _ when "ñń".Contains(s[0]):
    //            return "n";
    //        case var _ when "ýÿ".Contains(s[0]):
    //            return "y";
    //        case var _ when "ğĝ".Contains(s[0]):
    //            return "g";
    //        case var _ when s[0] == 'ř':
    //            return "r";
    //        case var _ when s[0] == 'ł':
    //            return "l";
    //        case var _ when s[0] == 'đ':
    //            return "d";
    //        case var _ when s[0] == 'ß':
    //            return "ss";
    //        case var _ when s[0] == 'þ':
    //            return "th";
    //        case var _ when s[0] == 'ĥ':
    //            return "h";
    //        case var _ when s[0] == 'ĵ':
    //            return "j";
    //        default:
    //            return "";
    //    }
    //}
}