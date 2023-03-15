using System.IO;

namespace Tofu3D;

public class AssetsManager
{
	public static bool IsShader(string path)
	{
		return path.Contains(".glsl");
	}

	public static bool Exists(string path)
	{
		return File.Exists(path);
	}
}