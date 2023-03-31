using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Tofu3D;

public static class TextureLoader
{
	public static Texture LoadTexture(TextureLoadSettings loadSettings)
	{
		int id = GL.GenTexture();
		if (loadSettings.Type == TextureType.Cubemap)
		{
			GL.ActiveTexture(TextureUnit.Texture0);
		}

		TextureCache.BindTexture(id, loadSettings.Type);

		List<byte>[] pixelsCollection = new List<byte>[loadSettings.Paths?.Length > 1 ? loadSettings.Paths.Length : 1];

		Vector2 imageSize = Vector2.Zero;
		for (int i = 0; i < pixelsCollection.Length; i++)
		{
			Image<Rgba32> image = Image.Load<Rgba32>(loadSettings.Paths[i]);
			imageSize = new Vector2(image.Width, image.Height);
			if (loadSettings.FlipX)
			{
				image.Mutate(x => x.Flip(FlipMode.Vertical));
			}

			pixelsCollection[i] = new(4 * image.Width * image.Height);

			for (int y = 0; y < image.Height; y++)
			{
				image.ProcessPixelRows(processPixels: accessor =>
				{
					Span<Rgba32> row = accessor.GetRowSpan(y);
					for (int x = 0; x < image.Width; x++)
					{
						pixelsCollection[i].Add(row[x].R);
						pixelsCollection[i].Add(row[x].G);
						pixelsCollection[i].Add(row[x].B);
						pixelsCollection[i].Add(row[x].A);
					}
				});
			}

			byte[] pixelsArray = pixelsCollection[i].ToArray();

			TextureTarget textureTarget = loadSettings.Type == TextureType.Texture2D ? TextureTarget.Texture2D : TextureTarget.TextureCubeMap;
			if (loadSettings.Type == TextureType.Texture2D)
			{
				GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelsArray);

				GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
				GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
				GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, loadSettings.FilterMode == TextureFilterMode.Bilinear ? (int) TextureMinFilter.Linear : (int) TextureMinFilter.Nearest);
				GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, loadSettings.FilterMode == TextureFilterMode.Bilinear ? (int) TextureMagFilter.Linear : (int) TextureMagFilter.Nearest);
			}
			else if (loadSettings.Type == TextureType.Cubemap)
			{
				GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelsArray);


				GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
				GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
				GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int) TextureWrapMode.ClampToEdge);
				GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
				GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
			}
		}


		ImGuiController.CheckGlError("texture load");

		Texture texture = new()
		                  {
			                  Id = id,
			                  Size = imageSize,
			                  Loaded = true,
			                  Path = loadSettings.Path,
			                  Paths = loadSettings.Paths
		                  };

		return texture;
	}
}