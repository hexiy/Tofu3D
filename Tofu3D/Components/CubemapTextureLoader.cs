using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tofu3D;

public class CubemapTextureLoader : AssetLoader<CubemapTexture>
{
	public override CubemapTexture SaveAsset(ref CubemapTexture asset, AssetLoadSettingsBase loadSettings)
	{
		throw new NotImplementedException();
	}

	public override void UnloadAsset(Asset<CubemapTexture> asset)
	{
		GL.DeleteTexture(asset.Handle.Id);
	}

	public override Asset<CubemapTexture> LoadAsset(AssetLoadSettingsBase assetLoadSettings)
	{
		CubemapTextureLoadSettings loadSettings = assetLoadSettings as CubemapTextureLoadSettings;
		int id = GL.GenTexture();
		string[] paths = loadSettings.Paths;
		byte[][] pixelsCollection = new byte[paths.Length][];
		Vector2 imageSize = Vector2.Zero;

		GL.ActiveTexture(TextureUnit.Texture0);

		TextureHelper.BindTexture(id, TextureType.Cubemap);

		for (int textureIndex = 0; textureIndex < pixelsCollection.Length; textureIndex++)
		{
			string path = paths[textureIndex];
			// path = loadSettings.Paths[textureIndex];
			Image<Rgba32> image = Image.Load<Rgba32>(path);
			imageSize = new Vector2(image.Width, image.Height);

			pixelsCollection[textureIndex] = new byte[4 * image.Width * image.Height];
			image.Frames[0].CopyPixelDataTo(pixelsCollection[textureIndex]);

			TextureTarget textureTarget = TextureTarget.TextureCubeMap;

			GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + textureIndex, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelsCollection[textureIndex]);

			GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int) loadSettings.WrapMode);
			GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int) loadSettings.WrapMode);
			GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int) loadSettings.WrapMode);
			GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int) loadSettings.FilterMode);
			GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int) loadSettings.FilterMode);
		}


		ImGuiController.CheckGlError("texture load");

		CubemapTexture texture = new()
		                         {
			                         Size = imageSize,
			                         Loaded = true,
			                         Path = loadSettings.Path,
			                         LoadSettings = loadSettings
		                         };
		texture.InitAssetRuntimeHandle(id);

		return texture;
	}
}