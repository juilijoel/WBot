using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WBot
{
    public static class Extensions
    {
        public static bool IsUrlValid(this string url)
        {
            string pattern = @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return reg.IsMatch(url);
        }

        public static string FormatUrl(this string url)
        {
            return url.Replace("https://", "").Replace("http://", "").Replace("www.", "");
        }
    }
}
