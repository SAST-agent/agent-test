using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class SimpleJsonArrayParser
{
    // 仅用于简单的 JSON string 数组：["a","b"]
    public static HashSet<string> ParseStringArray(string json)
    {
        var set = new HashSet<string>();

        if (string.IsNullOrWhiteSpace(json)) return set;

        // 粗略匹配 "..."
        foreach (Match m in Regex.Matches(json, "\"(.*?)\""))
        {
            var s = m.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(s))
                set.Add(s);
        }

        return set;
    }
}
