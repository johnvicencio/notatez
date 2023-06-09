using System.Text.RegularExpressions;

namespace Notatez.Models.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string title)
    {
        // Replace spaces with hyphens
        string slug = title.Replace(" ", "-");

        // Remove any special characters or punctuation
        slug = Regex.Replace(slug, @"[^a-zA-Z0-9\-]", "");

        return slug.ToLower();
    }

}