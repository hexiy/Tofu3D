using System.IO;

namespace TofuEngine;

public struct FileChangedInfo
{
    public string Path { get; init; }
    public WatcherChangeTypes ChangeType { get; init; }
}