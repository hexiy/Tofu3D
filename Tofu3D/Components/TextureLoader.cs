using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tofu3D;

public class TextureLoader : AssetLoader<Texture>
{
    public override Texture SaveAsset(ref Texture asset, AssetLoadSettingsBase loadSettings) =>
        throw new NotImplementedException();

    public override void UnloadAsset(Asset<Texture> asset)
    {
        GL.DeleteTexture(asset.Handle.Id);
    }

    public override Asset<Texture> LoadAsset(AssetLoadSettingsBase assetLoadSettings)
    {
        var loadSettings = assetLoadSettings as TextureLoadSettings;
        var id = GL.GenTexture();
        var path = loadSettings.Path;
        if (File.Exists(path) == false)
        {
            path = Folders.GetResourcePath("purple.png");
        }

        TextureHelper.BindTexture(id);

        var imageSize = Vector2.Zero;

        var image = Image.Load<Rgba32>(path);
        imageSize = new Vector2(image.Width, image.Height);

        var pixels = new byte[4 * image.Width * image.Height];
        image.Frames[0].CopyPixelDataTo(pixels);

        var textureTarget = TextureTarget.Texture2D;

        GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, pixels);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)loadSettings.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)loadSettings.WrapMode);
        GL.TexParameter(textureTarget, TextureParameterName.TextureWrapR, (int)loadSettings.WrapMode);
        // GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)loadSettings.FilterMode);
        // GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)loadSettings.FilterMode);

        // crashes the engine on macos
        if (OperatingSystem.IsWindows)
        {
            GL.TextureParameter(id, TextureParameterName.TextureMinFilter, (int)loadSettings.FilterMode + 257);
            GL.TextureParameter(id, TextureParameterName.TextureLodBias, -0.4f);
        }

        ImGuiController.CheckGlError("texture load");

        Texture texture = new()
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