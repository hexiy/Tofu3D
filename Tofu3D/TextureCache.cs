using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Tofu3D;

public static class TextureCache
{
	static Dictionary<int, Texture> _cachedTextures = new();
	static int _textureInUse = -1;

	static Texture LoadAndCreateTexture(string texturePath, bool flipX = true, bool smooth = false)
	{
		int id = GL.GenTexture();
		BindTexture(id);

		Image<Rgba32> image = Image.Load<Rgba32>(texturePath);
		if (flipX)
		{
			image.Mutate(x => x.Flip(FlipMode.Vertical));
		}

		List<byte> pixels = new(4 * image.Width * image.Height);

		for (int y = 0; y < image.Height; y++)
		{
			image.ProcessPixelRows(processPixels: accessor =>
			{
				Span<Rgba32> row = accessor.GetRowSpan(y);
				for (int x = 0; x < image.Width; x++)
				{
					pixels.Add(row[x].R);
					pixels.Add(row[x].G);
					pixels.Add(row[x].B);
					pixels.Add(row[x].A);
				}
			});
		}

		GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());

		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, smooth ? (int) TextureMinFilter.Linear : (int) TextureMinFilter.Nearest);
		GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, smooth ? (int) TextureMagFilter.Linear : (int) TextureMagFilter.Nearest);

		Texture texture = new();
		texture.Id = id;
		texture.Size = new Vector2(image.Width, image.Height);
		texture.Loaded = true;
		texture.Path = texturePath;

		_cachedTextures.Add(GetHash(texturePath), texture);
		return texture;
	}

	static Texture LoadAndCreateCubemapTexture(string texturePath)
	{
		int id = GL.GenTexture();
		BindTexture(id, TextureTarget.TextureCubeMap);

		Image<Rgba32> image = Image.Load<Rgba32>(texturePath);

		List<byte> pixels = new(4 * image.Width * image.Height);

		for (int y = 0; y < image.Height; y++)
		{
			image.ProcessPixelRows(processPixels: accessor =>
			{
				Span<Rgba32> row = accessor.GetRowSpan(y);
				for (int x = 0; x < image.Width; x++)
				{
					pixels.Add(row[x].R);
					pixels.Add(row[x].G);
					pixels.Add(row[x].B);
					pixels.Add(row[x].A);
				}
			});
		}

		for (int i = 0; i < 6; i++)
		{
			//load the face image here, not one shared texture ofc
			GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());
		}


		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToEdge);
		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
		GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

		ImGuiController.CheckGlError("cubemap");
		Texture texture = new()
		                  {
			                  Id = id,
			                  Size = new Vector2(image.Width, image.Height),
			                  Loaded = true,
			                  Path = texturePath
		                  };

		_cachedTextures.Add(GetHash(texturePath), texture);

		return texture;
	}

	public static Texture GetCubemapTexture(string texturePath)
	{
		if (_cachedTextures.ContainsKey(GetHash(texturePath)) == false)
		{
			return LoadAndCreateCubemapTexture(texturePath);
		}

		return _cachedTextures[GetHash(texturePath)];
	}

	public static Texture GetTexture(string texturePath, bool flipX = true, bool smooth = false)
	{
		if (_cachedTextures.ContainsKey(GetHash(texturePath)) == false)
		{
			return LoadAndCreateTexture(texturePath, flipX, smooth);
		}

		return _cachedTextures[GetHash(texturePath)];
	}

	public static void DeleteTexture(string texturePath)
	{
		if (_cachedTextures.ContainsKey(GetHash(texturePath)))
		{
			GL.DeleteTexture(_cachedTextures[GetHash(texturePath)].Id);

			_cachedTextures.Remove(GetHash(texturePath));
		}
	}

	public static int GetHash(string texturePath)
	{
		return texturePath.GetHashCode();
	}

	public static void BindTexture(int id, TextureTarget textureTarget = TextureTarget.Texture2D)
	{
		if (id == _textureInUse)
		{
			//return;
		}

		_textureInUse = id;
		GL.BindTexture(textureTarget, id);
	}
}