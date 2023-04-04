using System.IO;

namespace Tofu3D;

public class Folders
{
	public static string GetPathRelativeToEngineFolder(string path)
	{
		if (path.Length == 0)
		{
			return Assets;
		}

		return Path.GetRelativePath(EngineFolderPath, path);
	}

	public static string EngineFolderPath
	{
		get { return Environment.CurrentDirectory; }
	}
	public static string Assets
	{
		get { return Path.Combine(Environment.CurrentDirectory, "Assets"); }
	}
	public static string Textures
	{
		get { return Path.Combine(Assets, "2D"); }
	}
	public static string Shaders
	{
		get { return Path.Combine(Assets, "Shaders"); }
	}
	public static string Materials
	{
		get { return Path.Combine(Assets, "Materials"); }
	}
	public static string Models
	{
		get { return Path.Combine(Assets, "Models"); }
	}
}