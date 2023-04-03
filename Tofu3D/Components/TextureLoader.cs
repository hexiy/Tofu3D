using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Tofu3D;

public class TextureLoader : AssetLoader<Texture>
{
	public override void DisposeAsset(Asset<Texture> asset)
	{
		throw new NotImplementedException();
	}

	public override Asset<Texture> LoadAsset(IAssetLoadSettings assetLoadSettings)
	{
		TextureLoadSettings loadSettings = assetLoadSettings as TextureLoadSettings;
		int id = GL.GenTexture();
		if (loadSettings.Type == TextureType.Cubemap)
		{
			GL.ActiveTexture(TextureUnit.Texture0);
		}

		TextureCache.BindTexture(id, loadSettings.Type);

		// byte[][] pixelsCollection = new byte[loadSettings.Paths?.Length > 1 ? loadSettings.Paths.Length : 1][];
		byte[][] pixelsCollection = new byte[1][];

		Vector2 imageSize = Vector2.Zero;
		for (int textureIndex = 0; textureIndex < pixelsCollection.Length; textureIndex++)
		{
			string path = loadSettings.Path;
			// path = loadSettings.Paths[textureIndex];
			Image<Rgba32> image = Image.Load<Rgba32>(path);
			imageSize = new Vector2(image.Width, image.Height);
			if (loadSettings.FlipX)
			{
				image.Mutate(x => x.Flip(FlipMode.Vertical));
			}

			pixelsCollection[textureIndex] = new byte[4 * image.Width * image.Height];
			image.Frames[0].CopyPixelDataTo(pixelsCollection[textureIndex]);

			/*pixelsCollection[textureIndex] = new byte[4 * image.Width * image.Height];

			int pixelIndex = 0;
			for (int y = 0; y < image.Height; y++)
			{
				image.ProcessPixelRows(processPixels: accessor =>
				{
					Span<Rgba32> row = accessor.GetRowSpan(y);
					for (int x = 0; x < image.Width; x++)
					{
						pixelsCollection[textureIndex][pixelIndex] = row[x].R;
						pixelIndex++;
						pixelsCollection[textureIndex][pixelIndex] = row[x].G;

						pixelIndex++;
						pixelsCollection[textureIndex][pixelIndex] = row[x].B;

						pixelIndex++;
						pixelsCollection[textureIndex][pixelIndex] = row[x].A;
						pixelIndex++;
					}
				});
			}*/


			TextureTarget textureTarget = loadSettings.Type == TextureType.Texture2D ? TextureTarget.Texture2D : TextureTarget.TextureCubeMap;
			if (loadSettings.Type == TextureType.Texture2D)
			{
				GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelsCollection[textureIndex]);
			}
			else if (loadSettings.Type == TextureType.Cubemap)
			{
				GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + textureIndex, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelsCollection[textureIndex]);
			}

			GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int) loadSettings.WrapMode);
			GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int) loadSettings.WrapMode);
			GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int) loadSettings.WrapMode);
			GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int) loadSettings.FilterMode);
			GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int) loadSettings.FilterMode);
		}


		ImGuiController.CheckGlError("texture load");

		Texture texture = new()
		                  {
			                  Size = imageSize,
			                  Loaded = true,
			                  AssetPath = loadSettings.Path,
			                  // Paths = loadSettings.Paths
		                  };
		texture.InitAssetHandle(id);

		return texture;
	}
}