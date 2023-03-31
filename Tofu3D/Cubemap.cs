namespace Tofu3D;

public class Cubemap
{
	public List<Texture> Textures { get; private set; }

	public Cubemap(string[] texturePaths)
	{
		if (texturePaths.Length != 6)
		{
			return;
		}
	}
}