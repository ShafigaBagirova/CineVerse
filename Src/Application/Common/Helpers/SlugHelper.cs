using System.Text;
using System.Text.RegularExpressions;

namespace Application.Common.Helpers;

public static class SlugHelper
{
    public static string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        value = value.ToLowerInvariant().Trim();

        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        value = sb.ToString().Normalize(NormalizationForm.FormC);

        value = Regex.Replace(value, @"[^a-z0-9\s-]", "");
        value = Regex.Replace(value, @"\s+", "-");
        value = Regex.Replace(value, @"-+", "-");

        return value.Trim('-');
    }
}
