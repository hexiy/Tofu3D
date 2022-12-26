namespace Tofu3D;

public class AssetsManager
{
	public static bool IsShader(string path)
	{
		return path.Contains(".glsl");
	}
}