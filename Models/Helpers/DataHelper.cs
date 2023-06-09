using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Notatez.Models.Helpers;

public static class DataHelper
{
    // Get the name before the @ sign on email
    public static string GetUsernameFromEmail(string email)
    {
        // Find the index of the @ symbol.
        int atSignIndex = email.IndexOf("@");

        // If the @ symbol is not found, return the entire email address.
        if (atSignIndex == -1)
        {
            return email;
        }

        // Return the substring of the email address up to the @ symbol.
        return email.Substring(0, atSignIndex);
    }

    // Compare inputted string data with the existing data
    public static bool CompareText(string input, string existing)
    {
        // Remove trailing spaces from both strings.
        input = input.Trim();
        existing = existing.Trim();

        // Convert both strings to lowercase.
        input = input.ToLower();
        existing = existing.ToLower();

        // Compare the two strings.
        return input == existing;
    }

    // Shorten the content or data
    // Or if the text is encoded, to decode it first
    // Then ignore all HTML elements
    public static string GetPlainText(string htmlContent, bool decode = false)
    {
        if (decode)
        {
            // Decode HTML entities
            htmlContent = HttpUtility.HtmlDecode(htmlContent);
        }

        // Remove HTML tags using regular expressions
        string plainText = Regex.Replace(htmlContent, "<.*?>", string.Empty);
        plainText = plainText.Trim();

        return plainText;
    }

    public static string GetShortText(string htmlContent, int maxLength, bool decode = false)
    {
        if (decode)
        {
            // Decode HTML entities
            htmlContent = HttpUtility.HtmlDecode(htmlContent);
        }

        // Remove HTML tags using regular expressions
        string plainText = Regex.Replace(htmlContent, "<.*?>", string.Empty);
        plainText = plainText.Trim();
        // Trim the text to the specified maxLength
        plainText = plainText.Length > maxLength ? plainText.Substring(0, maxLength) : plainText;

        return plainText;
    }


    public static string EncodeData(string content)
    {
        return WebUtility.HtmlEncode(content);
    }

    public static string DecodeData(string content)
    {
        return WebUtility.HtmlDecode(content);
    }
}
