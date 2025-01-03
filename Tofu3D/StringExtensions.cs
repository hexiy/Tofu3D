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
}