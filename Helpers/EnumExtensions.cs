using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HomeBridge.Helpers;

public static partial class EnumExtensions
{
    // Returns a human label for an enum value: its [Display(Name)] if set, else PascalCase split into words.
    public static string Humanize(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>()?.Name;
        return display ?? SpaceCamelCase().Replace(value.ToString(), " $1").Trim();
    }

    [GeneratedRegex("([A-Z])")]
    private static partial Regex SpaceCamelCase();
}
