using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Locator;

public static class ProjectFileGenerator
{
    public static void GenerateCsproj(string projectDirectory, string projectName, string[] scriptFiles, string[] references)
    {

        for (int i = 0; i < scriptFiles.Length; i++)
        {
            scriptFiles[i] = Path.GetRelativePath(projectDirectory, scriptFiles[i]);
        }
        // Console.WriteLine($"Registered MSBuild?: {MSBuildLocator.IsRegistered}");
        var csProjPath = TofuPath.Combine(projectDirectory, $"{projectName}.csproj");

        // Create a new .csproj structure
        var project = ProjectRootElement.Create();

        // Set the Sdk (e.g., Microsoft.NET.Sdk)
        project.Sdk = "Microsoft.NET.Sdk";

        // Add the PropertyGroup for project settings
        var propertyGroup = project.AddPropertyGroup();
        propertyGroup.AddProperty("OutputType", "Library");
        propertyGroup.AddProperty("TargetFramework", "net9.0");
        propertyGroup.AddProperty("Nullable", "enable");

        // Add the ItemGroup for script files
        var itemGroup = project.AddItemGroup();
        foreach (var scriptFile in scriptFiles)
        {
            itemGroup.AddItem("Compile", scriptFile);
        }

        // Add the ItemGroup for references
        var refItemGroup = project.AddItemGroup();
        foreach (var reference in references)
        {
            if (reference.EndsWith(".dll"))
            {
                var referenceItem = refItemGroup.AddItem("Reference", Path.GetFileNameWithoutExtension(reference));
                referenceItem.AddMetadata("HintPath", reference);
            }
        }

        // Save the project
        Directory.CreateDirectory(projectDirectory); // Ensure the directory exists
        project.Save(csProjPath);

        Console.WriteLine($"Generated .csproj at: {csProjPath}");
    }
}