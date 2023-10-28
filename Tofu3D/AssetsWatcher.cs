using System.IO;
using System.Linq;

namespace Tofu3D;

public class AssetsWatcher
{
    private FileSystemWatcher _watcher;

    private Queue<FileChangedInfo> _changedFilesQueue = new();

    // ShaderCache can register for ".shader" file changes, so we only check the extension once 
    private Dictionary<AssetSupportedFileNameExtensions, Action<FileChangedInfo>> _fileWithExtensionChangedConsumers =
        new();

    public void RegisterFileChangedCallback(Action<FileChangedInfo> fileChanged, params string[] extensions)
    {
        AssetSupportedFileNameExtensions assetSupportedFileNameExtensions = new(extensions);
        _fileWithExtensionChangedConsumers[assetSupportedFileNameExtensions] = fileChanged;

        foreach (string supportedExtension in extensions) _watcher.Filters.Add($"{supportedExtension}");
    }

    // public static void RegisterFileChangedCallback(AssetSupportedFileNameExtensions supportedFileNameExtensions, Action<string> fileChanged)
    // {
    // 	_fileWithExtensionChangedConsumers[supportedFileNameExtensions] = fileChanged;
    // 	foreach (string supportedExtension in supportedFileNameExtensions.Extensions)
    // 	{
    // 		_watcher.Filters.Add($"*.{supportedExtension}");
    // 	}
    // }

    public void StartWatching()
    {
        _watcher = new FileSystemWatcher(Folders.Assets);
        _watcher.IncludeSubdirectories = true;
        _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.LastAccess |
                                NotifyFilters.Attributes;
        _watcher.EnableRaisingEvents = true;
        _watcher.Filter = "";
        _watcher.Changed += OnFileManipulated;
        _watcher.Deleted += OnFileManipulated;
        //_watcher.Created += OnFileCreated;
        _watcher.Error += (sender, args) => throw args.GetException();
    }

    private void OnFileManipulated(object sender, FileSystemEventArgs e)
    {
        // if (File.Exists(e.FullPath) == false)
        // {
        // 	return;
        // }

        if (e.FullPath.Contains("~")) return;

        string assetsRelativePath = Path.Combine("Assets", Path.GetRelativePath("Assets", e.FullPath));
        // some files have junk after the extension
        if (assetsRelativePath.Contains(".sb"))
            assetsRelativePath = assetsRelativePath.Substring(0, assetsRelativePath.IndexOf(".sb"));

        // if (_changedFilesQueue.Contains(assetsRelativePath))
        // {
        // 	return;
        // }
        FileChangedInfo fileChangedInfo = new()
        {
            Path = assetsRelativePath,
            ChangeType = e.ChangeType
        };

        // lock (_changedFilesQueue)
        // {
        // 	_changedFilesQueue.Enqueue(fileChangedInfo);
        // }

        if (AssetUtils.IsShader(assetsRelativePath)) Tofu.ShaderManager.QueueShaderReload(assetsRelativePath);
    }

    /// <summary>
    /// At the end of every frame we process all the file changes in case they accumulate multiple times in that frame
    /// </summary>
    public void ProcessChangedFilesQueue()
    {
        lock (_changedFilesQueue)
        {
            foreach (FileChangedInfo fileManipulatedInfo in _changedFilesQueue)
            {
                if (Global.Debug)
                    Debug.Log($"File {fileManipulatedInfo.ChangeType.ToString()}:{fileManipulatedInfo.Path}");

                string fileExtension = Path.GetExtension(fileManipulatedInfo.Path);

                foreach (KeyValuePair<AssetSupportedFileNameExtensions, Action<FileChangedInfo>>
                             fileWithExtensionChangedConsumer in _fileWithExtensionChangedConsumers)
                    if (fileWithExtensionChangedConsumer.Key.Extensions.Contains(fileExtension) ||
                        fileWithExtensionChangedConsumer.Key.Extensions.Contains("*"))
                        fileWithExtensionChangedConsumer.Value.Invoke(fileManipulatedInfo);
            }

            ClearChangedFilesQueue();
        }
    }

    private void ClearChangedFilesQueue()
    {
        _changedFilesQueue.Clear();
    }
}