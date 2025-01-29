using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

public static class ScriptsManager
{
    private static List<string> unusedScripts = new List<string>();
    public static Assembly ScriptsAssembly;
    private static ScriptLoadContext _scriptLoadContext;

    static ScriptsManager()
    {
    }

    public static void CopyDllsToProjectFolder()
    {
        File.Copy(TofuPath.Combine(Folders.EngineBinPath, "Tofu3D.dll"), TofuPath.Combine(Folders.Dlls, "Tofu3D.dll"),
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

        ProjectFileGenerator.GenerateCsproj(Folders.ProjectFullPath, "tofuProject", scriptsFiles, referencesPaths);

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using MemoryStream ms = new MemoryStream();
        EmitResult result = compilation.Emit(ms);

        using FileStream fs = new FileStream(TofuPath.Combine(Folders.Dlls, "Scripts.dll"), FileMode.Create);
        compilation.Emit(fs);

        if (!result.Success)
        {
            string errors = string.Join(Environment.NewLine, result.Diagnostics.Select(diag => diag.ToString()));
            Console.WriteLine($"Compilation errors: {errors}");
            return;
        }

        ms.Seek(0, SeekOrigin.Begin);

        _scriptLoadContext = new ScriptLoadContext();
        ScriptsAssembly = Assembly.Load(ms.ToArray());
    }
    /*
    public static void CompileScriptsAssembly()
    {
        var scripts = Directory.GetFiles(Folders.Scripts);

        Environment.SetEnvironmentVariable("ROSLYN_COMPILER_LOCATION", "./roslyn", EnvironmentVariableTarget.Process);
        CSharpCodeProvider provider = new CSharpCodeProvider();
        Environment.SetEnvironmentVariable("ROSLYN_COMPILER_LOCATION", null, EnvironmentVariableTarget.Process);

        //CodeDomProvider provider = new CSharpCodeProvider();
        CompilerParameters parameters = new CompilerParameters(null, "Scripts.dll");
        parameters.ReferencedAssemblies.Add("Tofu3D.dll");
        // parameters.ReferencedAssemblies.Add("Assemblies/Microsoft.Xna.Framework.dll");
        parameters.ReferencedAssemblies.Add("MonoGame.Extended.dll");
        parameters.ReferencedAssemblies.Add("MonoGame.Framework.dll");
        parameters.ReferencedAssemblies.Add("System.Drawing.dll");
        parameters.ReferencedAssemblies.Add("System.dll");
        parameters.ReferencedAssemblies.Add("System.Xml.dll");

        parameters.GenerateInMemory = true;
        parameters.GenerateExecutable = false;

        //System.ComponentModel.TypeDescriptor.AddAttributes(typeof(Enum), new Attribute[] { new System.ComponentModel.EditorAttribute(typeof(Engine.UITypeEditors.EnumEditor), typeof(System.Drawing.Design.UITypeEditor)) });


        CompilerResults results = provider.CompileAssemblyFromFile(parameters, scripts);

        if (results.Errors.HasErrors)
        {
            StringBuilder sb = new StringBuilder();

            foreach (CompilerError error in results.Errors)
            {
                sb.AppendLine(String.Format("Line [{0}] in {1}: \n Error ({2}): {3}", error.Line, error.FileName,
                    error.ErrorNumber, error.ErrorText));
            }
        }

        Assembly assembly = results.CompiledAssembly;

        ScriptsAssembly = assembly;
    }*/
}