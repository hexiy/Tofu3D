using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

public static class ScriptsManager
{
    public static Assembly ScriptsAssembly;
    private static ScriptLoadContext _scriptLoadContext;

    private static readonly string _componentScriptTemplate;

    static ScriptsManager()
    {
        _componentScriptTemplate =
            File.ReadAllText(
                TofuPath.Combine(Folders.EditorResources,
                    "ScriptTemplates", "ComponentScriptTemplate.txt"));
    }

    public static void CreateCustomComponentFile(string componentName, string path)
    {
        string componentFileContent = _componentScriptTemplate;
        componentFileContent = componentFileContent.Replace("#SCRIPTNAME#", componentName);
        File.WriteAllText(path, componentFileContent);
    }

    public static void CopyDllsToProjectFolder()
    {
        File.Copy(TofuPath.Combine(Folders.EditorResources, "Tofu3D.dll"), TofuPath.Combine(Folders.Dlls, "Tofu3D.dll"),
            overwrite: true);
    }

    public static void CompileScriptsAssembly()
    {
        ScriptsAssembly = typeof(Scripts.Component).Assembly;
    }
}