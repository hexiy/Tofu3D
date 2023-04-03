using System.Linq;
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
		string[] paths = new string[] {loadSettings.Path};
		byte[][] pixelsCollection;
		if (loadSettings.Type == TextureType.Cubemap)
		{
			GL.ActiveTexture(TextureUnit.Texture0);

			paths = loadSettings.Path.Split(".bmp");
			for (var i = 0; i < paths.Length; i++)
			{
				paths[i] = paths[i] + ".bmp";
			}

			pixelsCollection = new byte[paths.Length - 1][];
		}
		else
		{
			pixelsCollection = new byte[paths.Length][];
		}


		TextureCache.BindTexture(id, loadSettings.Type);

		// byte[][] pixelsCollection = new byte[loadSettings.Paths?.Length > 1 ? loadSettings.Paths.Length : 1][];


		Vector2 imageSize = Vector2.Zero;
		for (int textureIndex = 0; textureIndex < pixelsCollection.Length; textureIndex++)
		{
			string path = paths[textureIndex];
			// path = loadSettings.Paths[textureIndex];
			Image<Rgba32> image = Image.Load<Rgba32>(path);
			imageSize = new Vector2(image.Width, image.Height);
			if (loadSettings.FlipX)
			{
				image.Mutate(x => x.Flip(FlipMode.Vertical));
			}

			pixelsCollection[textureIndex] = new byte[4 * image.Width * image.Height];
			image.Frames[0].CopyPixelDataTo(pixelsCollection[textureIndex]);

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
			                  LoadSettings = loadSettings
			                  // Paths = loadSettings.Paths
		                  };
		texture.InitAssetHandle(id);

		return texture;
	}
}