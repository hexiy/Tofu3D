using System.IO;

namespace Tofu3D;

public struct FileChangedInfo
{
	public string Path { get; init; }
	public WatcherChangeTypes ChangeType { get; init; }
}