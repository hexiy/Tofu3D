using System.IO;
using System.Linq;

namespace TofuEngine;

public static class Program
{
    private static void Main(string[] args)
    {
        Debug.StartTimer("Engine start time");

        TofuEngine.Debug.Log($"args: {string.Join(" | ", args)}");

        // foreach (string s in args)
        // {
        // Console.WriteLine($"arg: {s}");
        // }

        string projectPath = string.Empty;

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-project")
            {
                // cause if theres a space in the project path it gets split into multiple strings...
                List<string> pathParts = new List<string>();
                for (int j = i + 1; j < args.Length; j++)
                {
                    // found next flag, so we're at the end of the project path
                    if (args[j].StartsWith("-"))
                        break;

                    pathParts.Add(args[j]);
                }

                projectPath = string.Join(" ", pathParts);
                projectPath = projectPath.Trim('\'', '"');
                break;
            }
        }

        if (string.IsNullOrEmpty(projectPath))
        {
            Console.WriteLine("No project path specified, not launching the editor");
            return;
        }

        Tofu.Launch(projectPath);
    }
}