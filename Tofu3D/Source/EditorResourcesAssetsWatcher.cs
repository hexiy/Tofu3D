using System.IO;
using System.Linq;

namespace TofuEngine;

// Reflects changes done to shaders in this project EditorResources folder to bin folder where the base shaders are stored
public class EditorResourcesAssetsWatcher
{
    private string _editorResourcesShadersAppContextPath;
    private FileSystemWatcher _watcher;

    public void StartWatching()
    {
        var a = AppContext.BaseDirectory;
        _editorResourcesShadersAppContextPath = Path.GetFullPath(Path.Combine(a, "../../../EditorResources/Shaders"));
        _watcher = new FileSystemWatcher(_editorResourcesShadersAppContextPath);
        _watcher.IncludeSubdirectories = true;
        _watcher.NotifyFilter = NotifyFilters.LastWrite; // | NotifyFilters.Size | NotifyFilters.LastAccess |
        //NotifyFilters.Attributes;
        _watcher.EnableRaisingEvents = true;
        _watcher.Filter = "";
        _watcher.Changed += OnFileManipulated;
        _watcher.Deleted += OnFileManipulated;
        _watcher.Created += OnFileManipulated;
        _watcher.Renamed += OnFileManipulated;

        _watcher.Error += (sender, args) => throw args.GetException();
    }

    private void OnFileManipulated(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.Contains("~"))
        {
            return;
        }

        var eFullPath = Path.GetFullPath(e.FullPath);
        string engineResourcesShaderRelativePath =
            Path.GetRelativePath(_editorResourcesShadersAppContextPath, eFullPath);
        IEnumerable<string> shadersInBinFolder = Directory.EnumerateFiles(Folders.EngineResourcesShaders);
        foreach (string shaderInBinFolder in shadersInBinFolder)
        {
            string binShaderRelativePath = Path.GetRelativePath(Folders.EngineResourcesShaders, shaderInBinFolder);
            if (engineResourcesShaderRelativePath.Equals(binShaderRelativePath, StringComparison.Ordinal))
            {
                var a = Path.DirectorySeparatorChar + Path.GetRelativePath("/", eFullPath);

                if (Path.Exists(a) == false)
                {
                    continue;
                }

                _watcher.EnableRaisingEvents = false;
                File.Delete(shaderInBinFolder);
                File.Copy(a, shaderInBinFolder, overwrite: false);
                _watcher.EnableRaisingEvents = true;

                Tofu.ShaderManager.QueueShaderReload(shaderInBinFolder);

                break;
            }
        }
    }
}