using System.Text.RegularExpressions;

namespace Velora.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLowerInvariant().Trim();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", "-");
            str = Regex.Replace(str, @"-+", "-");
            return str.Trim('-');
        }
    }

    public static class CurrencyHelper
    {
        public static string FormatPKR(decimal amount) => $"PKR {amount:N0}";
    }

    public static class DateHelper
    {
        public static string TimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;
            if (span.TotalMinutes < 1)   return "just now";
            if (span.TotalHours < 1)     return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalDays < 1)      return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7)      return $"{(int)span.TotalDays}d ago";
            if (span.TotalDays < 30)     return $"{(int)(span.TotalDays / 7)}w ago";
            if (span.TotalDays < 365)    return $"{(int)(span.TotalDays / 30)}mo ago";
            return $"{(int)(span.TotalDays / 365)}y ago";
        }
    }
}
