using System.IO;

namespace Tofu3D;

public class AssetsWatcher
{
	static FileSystemWatcher _watcher;

	public static void StartWatching()
	{
		_watcher = new FileSystemWatcher(Folders.Assets);
		_watcher.IncludeSubdirectories = true;
		_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.LastAccess | NotifyFilters.Attributes;
		_watcher.EnableRaisingEvents = true;
		_watcher.Filter = "";
		_watcher.Changed += OnFileChanged;
		//_watcher.Created += OnFileCreated;
		_watcher.Error += (sender, args) => throw args.GetException();
	}

	static void OnFileChanged(object sender, FileSystemEventArgs e)
	{
		string assetsRelativePath = Path.Combine("Assets", Path.GetRelativePath("Assets", e.FullPath));
		// some files have junk after the extension
		if (assetsRelativePath.Contains(".sb"))
		{
			assetsRelativePath = assetsRelativePath.Substring(0, assetsRelativePath.IndexOf(".sb"));
		}

		Debug.Log($"File changed:{assetsRelativePath}");

		if (AssetsManager.IsShader(assetsRelativePath))
		{
			ShaderCache.QueueShaderReload(assetsRelativePath);
		}
	}
}