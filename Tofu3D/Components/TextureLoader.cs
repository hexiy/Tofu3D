using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Tofu3D;

public class TextureLoader : AssetLoader<Texture>
{
	public override void SaveAsset(Texture asset, AssetLoadSettingsBase loadSettings)
	{
		throw new NotImplementedException();
	}

	public override void UnloadAsset(Asset<Texture> asset)
	{
		GL.DeleteTexture(asset.AssetRuntimeHandle.Id);
	}

	public override Asset<Texture> LoadAsset(AssetLoadSettingsBase assetLoadSettings)
	{
		TextureLoadSettings loadSettings = assetLoadSettings as TextureLoadSettings;
		int id = GL.GenTexture();
		string path = loadSettings.Path;


		TextureHelper.BindTexture(id, TextureType.Texture2D);


		Vector2 imageSize = Vector2.Zero;

		Image<Rgba32> image = Image.Load<Rgba32>(path);
		imageSize = new Vector2(image.Width, image.Height);
		if (loadSettings.FlipX)
		{
			image.Mutate(x => x.Flip(FlipMode.Vertical));
		}

		byte[] pixels = new byte[4 * image.Width * image.Height];
		image.Frames[0].CopyPixelDataTo(pixels);

		TextureTarget textureTarget = TextureTarget.Texture2D;

		GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);


		GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int) loadSettings.WrapMode);
		GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int) loadSettings.WrapMode);
		GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int) loadSettings.WrapMode);
		GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int) loadSettings.FilterMode);
		GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int) loadSettings.FilterMode);


		ImGuiController.CheckGlError("texture load");

		Texture texture = new()
		                  {
			                  Size = imageSize,
			                  Loaded = true,
			                  AssetPath = loadSettings.Path,
			                  LoadSettings = loadSettings
			                  // Paths = loadSettings.Paths
		                  };
		texture.InitAssetRuntimeHandle(id);

		return texture;
	}
}