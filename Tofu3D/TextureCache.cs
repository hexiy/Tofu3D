using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Tofu3D;

public static class TextureCache
{
	/*static Dictionary<int, Texture> _cachedTextures = new();
	static int _textureInUse = -1;

	public static Texture GetTexture(TextureLoadSettings textureLoadSettings)
	{
		int hash = GetHash(textureLoadSettings.Path);
		if (_cachedTextures.ContainsKey(hash) == false)
		{
			Texture texture = AssetManager.Load<Texture>(textureLoadSettings);
			_cachedTextures.Add(hash, texture);
			return texture;
		}

		return _cachedTextures[hash];
	}

	public static void DeleteTexture(string texturePath)
	{
		if (_cachedTextures.ContainsKey(GetHash(texturePath)))
		{
			GL.DeleteTexture(_cachedTextures[GetHash(texturePath)].TextureId);

			_cachedTextures.Remove(GetHash(texturePath));
		}
	}

	public static int GetHash(string texturePath)
	{
		return texturePath.GetHashCode();
	}*/

	public static void BindTexture(int id, TextureType textureType = TextureType.Texture2D)
	{
		/*if (id == _textureInUse)
		{
			//return;
		}

		_textureInUse = id;*/

		TextureTarget textureTarget = textureType == TextureType.Texture2D ? TextureTarget.Texture2D : TextureTarget.TextureCubeMap;
		GL.BindTexture(textureTarget, id);
	}
}