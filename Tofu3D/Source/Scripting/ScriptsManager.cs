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
                TofuPath.Combine(Folders.EngineResources,
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
        File.Copy(TofuPath.Combine(Folders.EngineResources, "Tofu3D.dll"), TofuPath.Combine(Folders.Dlls, "Tofu3D.dll"),
            overwrite: true);
    }

    public static void CompileScriptsAssembly()
    {
        if (_scriptLoadContext != null)
        {
            _scriptLoadContext.Unload();
            _scriptLoadContext = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        string[] scriptsFiles = Directory.GetFiles(Folders.Scripts, "*.cs");

        IEnumerable<SyntaxTree> syntaxTrees = scriptsFiles
            .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file)));

        string assemblyName = "Scripts.dll";

        string[] referencesPaths = new[]
        {
            typeof(object).Assembly.Location,
            // Assembly.Load("netstandard").Location,
            Assembly.Load("System.Runtime").Location,
            Assembly.Load("Tofu3D").Location,
        };
        PortableExecutableReference[] references = new PortableExecutableReference[referencesPaths.Length];
        for (int i = 0; i < referencesPaths.Length; i++)
        {
            references[i] = MetadataReference.CreateFromFile(referencesPaths[i]);
        }

        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName)
            .AddSyntaxTrees(syntaxTrees)
            .WithReferences(references)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream ms = new();
        EmitResult emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
        {
            string errors = string.Join("\n", emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage()));
            throw new Exception($"Scripts.dll compilation failed:\n{errors}");
        }

        ms.Seek(0, SeekOrigin.Begin);


        _scriptLoadContext = new ScriptLoadContext();
        ScriptsAssembly = Assembly.Load(ms.ToArray());
        // ScriptsAssembly = _scriptLoadContext.LoadFromStream(ms);
    }
}