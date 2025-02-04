namespace Tofu3D;

public static class StringExtensions
{
    public static string TrimAfter(this string str, char character)
    {
        if (str.Contains(character) == false)
        {
            return str;
        }

        return str.Remove(str.IndexOf(character));
    }
    public static string SplitCamelCase(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }
}