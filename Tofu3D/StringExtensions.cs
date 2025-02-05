using System.IO;
using System.Linq;

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


    public static string? GetExecutablePathFromMacosAppBundlePath(string appBundlePath)
    {
        if (!Directory.Exists(appBundlePath) || !appBundlePath.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
            return null;

        string infoPath = Path.Combine(appBundlePath, "Contents", "Info.plist");
        string executableName = "";

        if (File.Exists(infoPath))
        {
            try
            {
                string plistContent = File.ReadAllText(infoPath);
                int execKeyIndex = plistContent.IndexOf("<key>CFBundleExecutable</key>");
                if (execKeyIndex != -1)
                {
                    int startIndex = plistContent.IndexOf("<string>", execKeyIndex) + "<string>".Length;
                    int endIndex = plistContent.IndexOf("</string>", startIndex);
                    executableName = plistContent.Substring(startIndex, endIndex - startIndex);
                }
            }
            catch
            {
            }
        }

        string macOSPath = Path.Combine(appBundlePath, "Contents", "MacOS");

        if (Directory.Exists(macOSPath))
        {
            if (!string.IsNullOrEmpty(executableName))
            {
                string exactPath = Path.Combine(macOSPath, executableName);
                if (File.Exists(exactPath))
                    return exactPath;
            }

            try
            {
                string[] files = Directory.GetFiles(macOSPath);

                return files.FirstOrDefault();
            }
            catch
            {
                // Handle potential directory access errors
                return null;
            }
        }

        return null;
    }
}